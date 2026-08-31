using PolarityBreach.PolaritySystem;
using PolarityBreach.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        [SerializeField] private float timeBetweenSpawns = 0.3f;
        [SerializeField] private float timeBetweenWaves = 3f;
        [SerializeField] private float enemySpawnHeight = 0.5f;

        [Header("Spawn Warning")]
        [SerializeField] private GameObject whiteSpawnWarningPrefab;
        [SerializeField] private GameObject blackSpawnWarningPrefab;
        [SerializeField] private float spawnWarningDuration = 1f;

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
            isRunning = true;
            roomCleared = false;

            for (int waveIndex = 0; waveIndex < waves.Length; waveIndex++)
            {
                Debug.Log("Starting wave " + (waveIndex + 1));

                if (WaveWarningUI.Instance != null)
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

                for (int i = 0; i < spawnPositions.Length; i++)
                {
                    spawningEnemies = Mathf.Max(0, spawningEnemies - 1);
                    SpawnEnemyAtPosition(spawnPositions[i], group.polarity, groupEnemyPool, group);
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

        private void SpawnEnemyAtPosition(Vector3 spawnPosition, Polarity polarity, EnemyPool pool, EnemySpawnGroup group)
        {
            Enemy enemy = pool.GetEnemy(spawnPosition);

            if (enemy == null)
            {
                Debug.LogWarning("EnemyPool did not return an enemy.");
                return;
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
