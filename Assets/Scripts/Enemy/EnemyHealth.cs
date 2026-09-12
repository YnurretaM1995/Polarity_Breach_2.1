using System;
using PolarityBreach.Audio;
using PolarityBreach.Feedback;
using UnityEngine;
using PolarityBreach.PolaritySystem.Interfaces;
using Random = UnityEngine.Random;

namespace PolarityBreach.Enemy
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 30f;

        [Header("SFX")]
        [SerializeField] private AudioClip[] deathSounds;
        [SerializeField] private AudioClip deathSound;
        [SerializeField, Range(0f, 1f)] private float deathSoundVolume = 1f;
        [SerializeField] private bool playDeathSoundAs2D = true;

        private float currentHealth;

        public event Action OnDied;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => currentHealth <= 0f;

        private void OnEnable()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead) return;

            currentHealth -= amount;
            
            Vector3 spawnPosition = transform.position + new Vector3(0f, 1.5f, 0f);
            
            if (SpawnsDamagePopups.Instance != null)
            {
                SpawnsDamagePopups.Instance.DamageDone(amount, spawnPosition, false);
            }

            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                PlayDeathSfx();
                OnDied?.Invoke();
            }
        }

        private void PlayDeathSfx()
        {
            AudioClip clip = GetRandomDeathSound();

            if (playDeathSoundAs2D)
            {
                AudioHandler.Play2DSound(clip, deathSoundVolume);
                return;
            }

            AudioHandler.Play3DSound(clip, transform.position, deathSoundVolume);
        }

        private AudioClip GetRandomDeathSound()
        {
            if (deathSounds != null && deathSounds.Length > 0)
            {
                return deathSounds[Random.Range(0, deathSounds.Length)];
            }

            return deathSound;
        }
    }
}
