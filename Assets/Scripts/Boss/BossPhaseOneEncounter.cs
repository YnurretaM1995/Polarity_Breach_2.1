using PolarityBreach.Enemy;
using UnityEngine;

namespace PolarityBreach.Boss
{
    public class BossPhaseOneEncounter : MonoBehaviour
    {
        [SerializeField] BossHealth _bossHealth;
        [SerializeField] private EnemyWaveSpawner enemyWaveSpawner;

        private int nextWaveIndex;

        private void Start()
        {
            SpawnNextWave();
            
            _bossHealth.OnWeakPointDestroyed += HandleWeakPointDestroyed;
        }
        
        private void OnDestroy()
        {
            _bossHealth.OnWeakPointDestroyed -= HandleWeakPointDestroyed;
        }

        private void HandleWeakPointDestroyed()
        {
            if (_bossHealth.IsDead) return;

            SpawnNextWave();
        }

        private void SpawnNextWave()
        {
            enemyWaveSpawner.SpawnWaveByIndex(nextWaveIndex);
            nextWaveIndex++;
        }
    }
}
