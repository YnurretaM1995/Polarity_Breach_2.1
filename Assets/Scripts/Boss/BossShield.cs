using PolarityBreach.Enemy;
using PolarityBreach.PolaritySystem.Interfaces;
using UnityEngine;
using System;

namespace PolarityBreach.Boss
{
    public class BossShield : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private EnemyPool enemyPool;
        [SerializeField] private EnemyWaveSpawner enemyWaveSpawner;
        
        private float currentHealth;

        public event Action OnShieldDestroyed;
        
        public bool IsActive => gameObject.activeInHierarchy;
        public bool IsInvulnerable => HasEnemiesAlive();

        private void Awake()
        {
            FindEnemyPoolIfMissing();
        }

        void OnEnable()
        {
            currentHealth = maxHealth;
            FindEnemyPoolIfMissing();
        }
        
        public void TakeDamage(float amount)
        {
            if (HasEnemiesAlive())
            {
                Debug.Log("Shield is invulnerable while enemies are alive.");
                return;
            }
            
            currentHealth -= amount;
            currentHealth = Mathf.Max(currentHealth, 0f);

            if (currentHealth <= 0f)
            {
                OnShieldDestroyed?.Invoke();
                gameObject.SetActive(false);
            }
        }

        private bool HasEnemiesAlive()
        {
            FindEnemyPoolIfMissing();

            if (enemyWaveSpawner != null)
            {
                return enemyWaveSpawner.AliveEnemies > 0;
            }

            if (enemyPool != null)
            {
                return enemyPool.HasActiveEnemies;
            }

            return false;
        }

        private void FindEnemyPoolIfMissing()
        {
            if (enemyPool != null) return;

            if (enemyWaveSpawner != null)
            {
                enemyPool = enemyWaveSpawner.Pool;
            }

            if (enemyPool == null)
            {
                enemyPool = FindFirstObjectByType<EnemyPool>();
            }
        }
    }
}
