using PolarityBreach.Audio;
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

        [Header("SFX")]
        [SerializeField] private AudioClip[] vulnerableHitSounds;
        [SerializeField] private AudioClip vulnerableHitSound;
        [SerializeField, Range(0f, 1f)] private float vulnerableHitSoundVolume = 1f;
        [SerializeField] private AudioClip breakSound;
        [SerializeField, Range(0f, 1f)] private float breakSoundVolume = 1f;
        [SerializeField] private bool playShieldSoundsAs2D;
        
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
                PlayShieldSfx(breakSound, breakSoundVolume);
                OnShieldDestroyed?.Invoke();
                gameObject.SetActive(false);
                return;
            }

            PlayShieldSfx(GetRandomVulnerableHitSound(), vulnerableHitSoundVolume);
        }

        private void PlayShieldSfx(AudioClip clip, float volume)
        {
            if (playShieldSoundsAs2D)
            {
                AudioHandler.Play2DSound(clip, volume);
            }
            else
            {
                AudioHandler.Play3DSound(clip, transform.position, volume);
            }
        }

        private AudioClip GetRandomVulnerableHitSound()
        {
            if (vulnerableHitSounds != null && vulnerableHitSounds.Length > 0)
            {
                return vulnerableHitSounds[UnityEngine.Random.Range(0, vulnerableHitSounds.Length)];
            }

            return vulnerableHitSound;
        }

        private bool HasEnemiesAlive()
        {
            if (enemyWaveSpawner != null)
                return enemyWaveSpawner.HasEnemiesRemaining;

            FindEnemyPoolIfMissing();

            if (enemyPool != null)
                return enemyPool.HasActiveEnemies;

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
