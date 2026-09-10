using PolarityBreach.Enemy;
using PolarityBreach.PolaritySystem;
using UnityEngine;

namespace PolarityBreach.Boss
{
    public class BossPhaseController : MonoBehaviour
    {
        [Header("Attacks")]
        [SerializeField] private BossPhaseOneAttack phaseOneAttack;
        [SerializeField] private BossPhaseTwoAttack phaseTwoAttack;
        [SerializeField] private float phase3_4BeamRotationSpeed = 10f;

        [Header("Shield")]
        [SerializeField] private BossShield bossShield;

        [Header("Enemy Waves")]
        [SerializeField] private EnemyWaveSpawner enemyWaveSpawner;
        [SerializeField] private bool spawnWaveOnStart = true;
        [SerializeField] private int firstTransitionWaveIndex;
        [SerializeField] private int phase4WaveIndex = 3;

        [Header("Player Shooting")]
        [SerializeField] private TestShooter playerShooter;

        private BossHealth health;
        private PolarityComponent shieldPolarity;
        private int currentPhase = 1;
        private int nextTransitionWaveIndex;
        private bool phase4EnemiesSpawned;

        private void Awake()
        {
            health = GetComponent<BossHealth>();

            if (phaseOneAttack == null)
            {
                phaseOneAttack = GetComponent<BossPhaseOneAttack>();
            }

            if (phaseTwoAttack == null)
            {
                phaseTwoAttack = GetComponent<BossPhaseTwoAttack>();
            }

            if (enemyWaveSpawner == null)
            {
                enemyWaveSpawner = GetComponent<EnemyWaveSpawner>();
            }

            if (playerShooter == null)
            {
                playerShooter = FindFirstObjectByType<TestShooter>();
            }

            if (bossShield != null)
            {
                shieldPolarity = bossShield.GetComponent<PolarityComponent>();
            }
        }

        private void Start()
        {
            if (bossShield != null)
            {
                bossShield.gameObject.SetActive(false);
            }

            health.SetShielded(false);
            health.OnWeakPointDestroyed += StartShieldTransition;
            health.OnDied += HandleBossDied;

            nextTransitionWaveIndex = firstTransitionWaveIndex;

            StopAllAttacks();
            StartCurrentPhase();

            if (spawnWaveOnStart)
            {
                SpawnNextTransitionWave();
            }
        }

        private void Update()
        {
            if (phaseTwoAttack == null) return;

            if (currentPhase == 3 || currentPhase == 4)
            {
                phaseTwoAttack.SetCustomRotationSpeed(phase3_4BeamRotationSpeed);
            }
        }

        private void StartShieldTransition()
        {
            if (health.IsDead) return;

            StopAllAttacks();
            health.SetShielded(true);

            if (shieldPolarity != null)
            {
                shieldPolarity.Toggle();
            }

            if (bossShield != null)
            {
                bossShield.gameObject.SetActive(true);
                bossShield.OnShieldDestroyed += HandleShieldDestroyed;
            }

            SpawnNextTransitionWave();
        }

        private void HandleShieldDestroyed()
        {
            bossShield.OnShieldDestroyed -= HandleShieldDestroyed;

            health.SetShielded(false);
            currentPhase++;

            if (currentPhase > 4)
            {
                currentPhase = 4;
            }

            StartCurrentPhase();
        }

        private void StartCurrentPhase()
        {
            StopAllAttacks();

            if (currentPhase == 1)
            {
                StartPhaseOneAttack();
                return;
            }

            if (currentPhase == 2)
            {
                StartPhaseTwoAttack();
                return;
            }

            if (currentPhase == 3)
            {
                StartPhaseOneAttack();
                StartPhaseTwoAttack(phase3_4BeamRotationSpeed);
                return;
            }

            if (currentPhase == 4)
            {
                StartPhaseOneAttack();
                StartPhaseTwoAttack(phase3_4BeamRotationSpeed);
                StartPhase4Wave();
            }
        }

        private void SpawnNextTransitionWave()
        {
            if (enemyWaveSpawner == null) return;

            enemyWaveSpawner.SpawnWaveByIndex(nextTransitionWaveIndex);
            nextTransitionWaveIndex++;
        }

        private void StartPhase4Wave()
        {
            if (phase4EnemiesSpawned) return;
            if (enemyWaveSpawner == null) return;
            if (phase4WaveIndex < 0)
            {
                Debug.LogWarning("Boss phase 4 enemy wave is disabled.");
                return;
            }

            phase4EnemiesSpawned = true;
            enemyWaveSpawner.SpawnWaveByIndex(phase4WaveIndex);
        }

        private void StartPhaseOneAttack()
        {
            if (phaseOneAttack != null)
            {
                phaseOneAttack.StartPhase();
            }
        }

        private void StartPhaseTwoAttack(float rotationSpeed)
        {
            if (phaseTwoAttack != null)
            {
                phaseTwoAttack.StartPhase(rotationSpeed);
            }
        }

        private void StartPhaseTwoAttack()
        {
            if (phaseTwoAttack != null)
            {
                phaseTwoAttack.StartPhase();
            }
        }

        private void StopAllAttacks()
        {
            if (phaseOneAttack != null)
            {
                phaseOneAttack.StopPhase();
            }

            if (phaseTwoAttack != null)
            {
                phaseTwoAttack.StopPhase();
            }
        }

        private void HandleBossDied()
        {
            StopAllAttacks();

            if (playerShooter != null)
            {
                playerShooter.SetShootingEnabled(false);
            }

            if (enemyWaveSpawner != null)
            {
                enemyWaveSpawner.DebugStopAndClearEnemies();
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnWeakPointDestroyed -= StartShieldTransition;
                health.OnDied -= HandleBossDied;
            }

            if (bossShield != null)
            {
                bossShield.OnShieldDestroyed -= HandleShieldDestroyed;
            }
        }
    }
}
