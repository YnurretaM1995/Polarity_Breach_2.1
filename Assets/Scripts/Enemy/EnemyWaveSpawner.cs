using PolarityBreach.Audio;
using PolarityBreach.PolaritySystem;
using PolarityBreach.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace PolarityBreach.Enemy
{
    public enum SpawnPattern
    {
        RandomCluster,
        LineShape
        //VShape,
        //CircleShape,
        //zigzagShape
    }

    [System.Serializable]
    public class EnemySpawnGroup
    {
        public EnemyPool enemyPool;
        public int enemyCount = 5;
        public SpawnPattern pattern = SpawnPattern.RandomCluster;
        public float spacing = 2f;
        public Polarity polarity = Polarity.White;

        [Header("Shooting Settings")]
        public int projectilesPerShot = 1;
        public float spreadAngle = 15f;
    }
    [System.Serializable]
    public class EnemyWave
    {
        public EnemySpawnGroup[] groups;
    }

    public class EnemyWaveSpawner : MonoBehaviour
    {
        public event Action OnRoomCleared;
        
        [Header("References")]
         private EnemyPool enemyPool;

        [SerializeField] private Transform[] possibleSpawnPoints;

        [Header("Wave Settings")] [SerializeField]
        private EnemyWave[] waves;

        [SerializeField] private bool autoStart = true;
        [SerializeField] private bool showWarningBeforeFirstWave;
        [SerializeField] private float timeBetweenSpawns = 0.3f;
        [SerializeField] private float timeBetweenWaves = 3f;
        [SerializeField] private float enemySpawnHeight = 0.5f;

        [Header("Spawn Warning")]
        [SerializeField] private GameObject whiteSpawnWarningPrefab;
        [SerializeField] private GameObject blackSpawnWarningPrefab;
        [SerializeField] private float spawnWarningDuration = 1f;

        [Header("Spawn Warning SFX")]
        [SerializeField] private AudioClip[] spawnWarningSounds;
        [SerializeField] private AudioClip spawnWarningSound;
        [SerializeField, Range(0f, 1f)] private float spawnWarningSoundVolume = 1f;
        [SerializeField] private bool playSpawnWarningSoundAs2D;
        [SerializeField] private bool playSpawnWarningSoundOncePerGroup = true;

        [Header("Enemy Appears SFX")]
        [FormerlySerializedAs("phaseInSounds")]
        [SerializeField] private AudioClip[] appearSounds;
        [FormerlySerializedAs("phaseInSound")]
        [SerializeField] private AudioClip appearSound;
        [FormerlySerializedAs("phaseInSoundVolume")]
        [SerializeField, Range(0f, 1f)] private float appearSoundVolume = 1f;
        [FormerlySerializedAs("playPhaseInSoundAs2D")]
        [SerializeField] private bool playAppearSoundAs2D;
        [SerializeField] private bool playAppearSoundOncePerGroup = true;

        [Header("Random Cluster Settings")] [SerializeField]
        private float clusterRadius = 3f;

        private List<Enemy> activeEnemies = new List<Enemy>();
        private Dictionary<Enemy, EnemyPool> enemyPoolsByEnemy = new Dictionary<Enemy, EnemyPool>();
        private int aliveEnemies;
        public int AliveEnemies => aliveEnemies;
        public int EnemiesRemaining => aliveEnemies + spawningEnemies;
        public bool HasEnemiesRemaining => EnemiesRemaining > 0;
        public EnemyPool Pool => enemyPool;
        private int spawningEnemies;
        private bool isRunning;
        private bool roomCleared;

        public bool IsRunning => isRunning;
        public bool RoomCleared => roomCleared;

        private void Start()
        {
            if (autoStart)
            {
                StartCoroutine(RunWaves());
            }
        }

        private IEnumerator RunWaves()
        {
            yield return null;

            isRunning = true;
            roomCleared = false;

            for (int waveIndex = 0; waveIndex < waves.Length; waveIndex++)
            {
                yield return WaitForGameplayReady();

                Debug.Log("Starting wave " + (waveIndex + 1));

                bool shouldShowWaveWarning = showWarningBeforeFirstWave || waveIndex > 0;

                if (shouldShowWaveWarning && WaveWarningUI.Instance != null)
                    yield return WaveWarningUI.Instance.PlayWarning();

                yield return StartCoroutine(SpawnWave(waves[waveIndex]));

                yield return new WaitUntil(() => aliveEnemies <= 0);

                Debug.Log("Wave " + (waveIndex + 1) + " completed");

                yield return new WaitForSeconds(timeBetweenWaves);
            }

            Debug.Log("Room cleared!");
            isRunning = false;
            roomCleared = true;
            OnRoomCleared?.Invoke();
        }

        private IEnumerator WaitForGameplayReady()
        {
            while (Time.timeScale <= 0f || UIQueue.IsBlocking)
            {
                yield return null;
            }
        }

        private IEnumerator SpawnWave(EnemyWave wave)
        {
            for (int groupIndex = 0; groupIndex < wave.groups.Length; groupIndex++)
            {
                EnemySpawnGroup group = wave.groups[groupIndex];
                EnemyPool groupEnemyPool = group.enemyPool != null ? group.enemyPool : enemyPool;
                Transform spawnPoint = GetRandomSpawnPoint();

                if (groupEnemyPool == null)
                {
                    Debug.LogWarning("No enemy pool assigned.");
                    yield break;
                }

                if (spawnPoint == null)
                {
                    Debug.LogWarning("No spawn points assigned.");
                    yield break;
                }

                Vector3[] offsets = GetPatternOffsets(group.pattern, group.enemyCount, group.spacing);
                Vector3[] spawnPositions = new Vector3[offsets.Length];

                for (int i = 0; i < offsets.Length; i++)
                {
                    Vector3 spawnPosition = spawnPoint.position + offsets[i];
                    spawnPosition.y = enemySpawnHeight;
                    spawnPositions[i] = spawnPosition;
                }

                spawningEnemies += spawnPositions.Length;

                yield return StartCoroutine(ShowSpawnWarnings(spawnPositions, group.polarity));

                bool playedAppearSoundForGroup = false;

                for (int i = 0; i < spawnPositions.Length; i++)
                {
                    spawningEnemies = Mathf.Max(0, spawningEnemies - 1);
                    bool spawnedEnemy = SpawnEnemyAtPosition(spawnPositions[i], group.polarity, groupEnemyPool, group);

                    if (spawnedEnemy)
                    {
                        if (playAppearSoundOncePerGroup)
                        {
                            if (!playedAppearSoundForGroup)
                            {
                                PlayAppearSfx(spawnPositions[i]);
                                playedAppearSoundForGroup = true;
                            }
                        }
                        else
                        {
                            PlayAppearSfx(spawnPositions[i]);
                        }
                    }

                    yield return new WaitForSeconds(timeBetweenSpawns);
                }
            }
        }

        private IEnumerator ShowSpawnWarnings(Vector3[] spawnPositions, Polarity polarity)
        {
            GameObject spawnWarningPrefab = polarity == Polarity.White
                ? whiteSpawnWarningPrefab
                : blackSpawnWarningPrefab;

            if (spawnWarningPrefab == null || spawnWarningDuration <= 0f)
            {
                yield break;
            }

            GameObject[] warnings = new GameObject[spawnPositions.Length];

            for (int i = 0; i < spawnPositions.Length; i++)
            {
                warnings[i] = Instantiate(spawnWarningPrefab, spawnPositions[i], Quaternion.identity);

                if (!playSpawnWarningSoundOncePerGroup)
                {
                    PlaySpawnWarningSfx(spawnPositions[i]);
                }
            }

            if (playSpawnWarningSoundOncePerGroup && spawnPositions.Length > 0)
            {
                PlaySpawnWarningSfx(GetAveragePosition(spawnPositions));
            }

            yield return new WaitForSeconds(spawnWarningDuration);

            for (int i = 0; i < warnings.Length; i++)
            {
                if (warnings[i] != null)
                {
                    Destroy(warnings[i]);
                }
            }
        }

        private Transform GetRandomSpawnPoint()
        {
            if (possibleSpawnPoints == null || possibleSpawnPoints.Length == 0)
                return null;


            int randomIndex = UnityEngine.Random.Range(0, possibleSpawnPoints.Length);
            return possibleSpawnPoints[randomIndex];
        }

        private bool SpawnEnemyAtPosition(Vector3 spawnPosition, Polarity polarity, EnemyPool pool, EnemySpawnGroup group)
        {
            Enemy enemy = pool.GetEnemy(spawnPosition);

            if (enemy == null)
            {
                Debug.LogWarning("EnemyPool did not return an enemy.");
                return false;
            }

            PolarityComponent polarityComponent = enemy.GetComponent<PolarityComponent>();

            if (polarityComponent != null)
            {
                polarityComponent.SetPolarity(polarity);
            }

            EnemyShooter shooter = enemy.GetComponent<EnemyShooter>();

            if (shooter != null)
            {
                shooter.SetWaveShootingSettings(group.projectilesPerShot, group.spreadAngle);
            }

            aliveEnemies++;
            enemy.Spawn(this);
            activeEnemies.Add(enemy);
            enemyPoolsByEnemy[enemy] = pool;
            return true;
        }

        private void PlaySpawnWarningSfx(Vector3 position)
        {
            AudioClip clip = GetRandomSpawnWarningSound();
            if (clip == null) return;

            if (playSpawnWarningSoundAs2D)
            {
                AudioHandler.Play2DSound(clip, spawnWarningSoundVolume);
                return;
            }

            AudioHandler.Play3DSound(clip, position, spawnWarningSoundVolume);
        }

        private AudioClip GetRandomSpawnWarningSound()
        {
            if (spawnWarningSounds != null && spawnWarningSounds.Length > 0)
            {
                return spawnWarningSounds[UnityEngine.Random.Range(0, spawnWarningSounds.Length)];
            }

            return spawnWarningSound;
        }

        private void PlayAppearSfx(Vector3 position)
        {
            AudioClip clip = GetRandomAppearSound();
            if (clip == null) return;

            if (playAppearSoundAs2D)
            {
                AudioHandler.Play2DSound(clip, appearSoundVolume);
                return;
            }

            AudioHandler.Play3DSound(clip, position, appearSoundVolume);
        }

        private AudioClip GetRandomAppearSound()
        {
            if (appearSounds != null && appearSounds.Length > 0)
            {
                return appearSounds[UnityEngine.Random.Range(0, appearSounds.Length)];
            }

            return appearSound;
        }

        private Vector3 GetAveragePosition(Vector3[] positions)
        {
            Vector3 average = Vector3.zero;

            for (int i = 0; i < positions.Length; i++)
            {
                average += positions[i];
            }

            return average / positions.Length;
        }

        private Vector3[] GetPatternOffsets(SpawnPattern pattern, int enemyCount, float spacing)
        {
            if (pattern == SpawnPattern.LineShape)
                return GetLineOffsets(enemyCount, spacing);

            return GetRandomClusterOffsets(enemyCount);
        }

        private Vector3[] GetRandomClusterOffsets(int enemyCount)
        {
            Vector3[] offsets = new Vector3[enemyCount];

            for (int i = 0; i < enemyCount; i++)
            {
                Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * clusterRadius;
                offsets[i] = new Vector3(randomCircle.x, 0f, randomCircle.y);
            }

            return offsets;
        }

        private Vector3[] GetLineOffsets(int enemyCount, float spacing)
        {
            Vector3[] offsets = new Vector3[enemyCount];

            float startX = -(enemyCount - 1) * spacing * 0.5f;

            for (int i = 0; i < enemyCount; i++)
            {
                float x = startX + i * spacing;
                offsets[i] = new Vector3(x, 0f, 0f);
            }

            return offsets;
        }
        
        public void SpawnWaveByIndex(int waveIndex)
        {
            if (waveIndex < 0 || waveIndex >= waves.Length)
            {
                return;
            }

            StartCoroutine(SpawnWave(waves[waveIndex]));
        }

        public void EnemyDied(Enemy enemy)
        {
            aliveEnemies--;
            activeEnemies.Remove(enemy);

            if (enemyPoolsByEnemy.TryGetValue(enemy, out EnemyPool pool))
            {
                enemyPoolsByEnemy.Remove(enemy);
                pool.ReturnEnemy(enemy);
                return;
            }

            enemyPool.ReturnEnemy(enemy);
        }

        public void DebugCompleteCurrentWave()
        {
            SkipCurrentWave();
        }

        public void DebugStopAndClearEnemies()
        {
            StopAllCoroutines();
            SkipCurrentWave();
            aliveEnemies = 0;
            spawningEnemies = 0;
            activeEnemies.Clear();
            enemyPoolsByEnemy.Clear();
        }

        [ContextMenu("Debug Clear Room")]
        public void DebugClearRoom()
        {
            if (roomCleared) return;

            DebugStopAndClearEnemies();
            isRunning = false;
            roomCleared = true;
            OnRoomCleared?.Invoke();
        }

        public void SkipCurrentWave()
        {
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i] != null)
                    activeEnemies[i].Kill();
            }
        }
    }
}
