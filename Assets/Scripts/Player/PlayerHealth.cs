using System;
using PolarityBreach.Feedback;
using UnityEngine;
using PolarityBreach.PolaritySystem.Interfaces;

namespace PolarityBreach.Player
{
    [RequireComponent(typeof(PlayerStatsData))]
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private BloodOverlay bloodOverlay;
        [SerializeField] private CameraControlScript cameraShake;

        private PlayerStatsData playerStats;
        
        public float CurrentHealth { get; private set; }
        public float MaxHealth => playerStats != null ? playerStats.maxHealth : 100f;
        public bool IsDead => CurrentHealth <= 0f;
        
        public bool GodMode { get; set; } = false;
        private bool IsInvincible => GodMode || (playerStats != null && playerStats.godMode);
        
        public event Action OnDied;

        private void Awake()
        {
            playerStats = GetComponent<PlayerStatsData>();
        }

        private void Start()
        {
            FullHeal();
        }

        public void TakeDamage(float amount)
        {
            if (IsInvincible || IsDead) return;
            
            CurrentHealth -= amount;
            
            int integerDamageValue = Mathf.RoundToInt(amount);
            
            Vector3 spawnPosition = transform.position + new Vector3(0f, 1.5f, 0f);
            
            if (SpawnsDamagePopups.Instance != null)
            {
                SpawnsDamagePopups.Instance.DamageDone(integerDamageValue, spawnPosition, false);
            }
            
            bloodOverlay?.OnDamaged();
            cameraShake?.Shake(1.25f);
            
            if (CurrentHealth <= 0f)
            {
                CurrentHealth = 0f;
                Die();
            }
        }

        public void FullHeal()
        {
            if (playerStats == null) return;
            CurrentHealth = playerStats.maxHealth;
        }

        public void ReplenishHealth(float amount)
        {
            if (playerStats == null || IsDead) return;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        }

        private void Die()
        {
            Debug.Log("Player has died.");
            OnDied?.Invoke();
        }
        
        public void IncreaseMaxHealth(float amount)
        {
            if (playerStats == null) return;
            ReplenishHealth(amount);
        }
    }
}
