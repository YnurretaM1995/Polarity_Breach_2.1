using System.Collections.Generic;
using UnityEngine;

namespace PolarityBreach.Player
{
    public class PlayerStatsData : MonoBehaviour
    {
        [Header("Player Stats")] 
        public float movementSpeed;
        public float maxHealth;
        public float polaritySwitchCooldown;

        [Header("Runtime Modifiers")]
        [SerializeField, Min(0f)] private float movementSpeedMultiplier = 1f;
        [SerializeField, Range(0f, 1f)] private float oneMeleeSlowMultiplier = 0.8f;
        [SerializeField, Range(0f, 1f)] private float twoMeleeSlowMultiplier = 0.6f;
        [SerializeField, Range(0f, 1f)] private float threeMeleeSlowMultiplier = 0.4f;
        [SerializeField, Range(0f, 1f)] private float maxMeleeSlowMultiplier = 0.2f;
        private readonly Dictionary<object, MovementSlowRequest> movementSlowRequests = new Dictionary<object, MovementSlowRequest>();
        private readonly List<object> expiredSlowSources = new List<object>();

        [Header("Weapon Stats")] 
        public float attackSpeedDelay;
        public float attackDamage;
        public float attackSpeed;
        public float knockBackPower;

        [Header("Unlock Dash Skill")]
        public bool dashUnlocked;
        [Header("Dash Stats")] 
        public float dashSpeed;
        public float dashDuration;
        public float dashCooldown;
        
        [Header("Unlock ChargeShot Skill")]
        public bool chargeShotUnlocked;
        [Header("ChargeShot Stats")] 
        public float chargeShotDamageMultiplier = 1.5f;
        public float chargeShotSpeed;
        public float chargeShotKnockBackPower;
        public float chargeTime;

        [Header("Debug")] 
        public bool godMode;

        public float CurrentMovementSpeed => movementSpeed * movementSpeedMultiplier;

        private void Update()
        {
            RefreshMovementSpeedMultiplier();
        }

        private void OnDisable()
        {
            ResetMovementSpeedMultiplier();
        }

        public void ApplyTemporaryMovementSlow(object source, float duration)
        {
            if (source == null) return;

            movementSlowRequests[source] = new MovementSlowRequest
            {
                expiresAt = Time.time + duration
            };

            RefreshMovementSpeedMultiplier();
        }

        public void ResetMovementSpeedMultiplier()
        {
            movementSlowRequests.Clear();
            movementSpeedMultiplier = 1f;
        }

        private void RefreshMovementSpeedMultiplier()
        {
            if (movementSlowRequests.Count > 0)
            {
                expiredSlowSources.Clear();

                foreach (KeyValuePair<object, MovementSlowRequest> pair in movementSlowRequests)
                {
                    if (Time.time >= pair.Value.expiresAt)
                        expiredSlowSources.Add(pair.Key);
                }

                for (int i = 0; i < expiredSlowSources.Count; i++)
                    movementSlowRequests.Remove(expiredSlowSources[i]);
            }

            movementSpeedMultiplier = GetMeleeSlowMultiplier(movementSlowRequests.Count);
        }

        private float GetMeleeSlowMultiplier(int activeSlowCount)
        {
            if (activeSlowCount <= 0) return 1f;
            if (activeSlowCount == 1) return oneMeleeSlowMultiplier;
            if (activeSlowCount == 2) return twoMeleeSlowMultiplier;
            if (activeSlowCount == 3) return threeMeleeSlowMultiplier;

            return maxMeleeSlowMultiplier;
        }

        private struct MovementSlowRequest
        {
            public float expiresAt;
        }
    }
}
