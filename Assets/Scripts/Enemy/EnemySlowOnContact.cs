using System.Collections;
using PolarityBreach.Player;
using PolarityBreach.PolaritySystem;
using UnityEngine;
using UnityEngine.AI;

namespace PolarityBreach.Enemy
{
    public class EnemySlowOnContact : MonoBehaviour
    {
        [Header("Slow")]
        [SerializeField] private float slowMultiplier = 0.5f;

        [Header("Knockback")]
        [SerializeField] private float knockbackDistance = 3f;
        [SerializeField] private float knockbackDuration = 0.2f;
        [SerializeField] private float shakeOffRadius = 2.5f;
        [SerializeField] private float stunDuration = 0.5f;

        private NavMeshAgent agent;
        private NavMeshAgentPusher pusher;
        private EnemyPursuitAI pursuitAI;
        private Collider enemyCollider;
        private bool canHit = true;
        private PlayerStatsData slowedPlayerStats;
        private float originalPlayerSpeed;
        private Coroutine knockbackRoutine;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            pusher = GetComponent<NavMeshAgentPusher>();
            if (pusher == null) pusher = gameObject.AddComponent<NavMeshAgentPusher>();
            pursuitAI = GetComponent<EnemyPursuitAI>();
            enemyCollider = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            PlayerPolarityController.OnAnyPlayerPolaritySwitched += HandleAnyPlayerPolaritySwitched;
        }

        private void OnTriggerEnter(Collider other)
        {
            TryStartSlow(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryStartSlow(other);
        }

        private void OnTriggerExit(Collider other)
        {
            TryStopSlow(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            TryStartSlow(collision.collider);
        }

        private void OnCollisionStay(Collision collision)
        {
            TryStartSlow(collision.collider);
        }

        private void OnCollisionExit(Collision collision)
        {
            TryStopSlow(collision.collider);
        }

        private void TryStartSlow(Collider other)
        {
            if (!canHit) return;
            if (slowedPlayerStats != null) return;

            PlayerStatsData playerStats = other.GetComponentInParent<PlayerStatsData>();
            if (playerStats == null) return;

            StartSlow(playerStats);
        }

        private void TryStopSlow(Collider other)
        {
            if (slowedPlayerStats == null) return;

            PlayerStatsData playerStats = other.GetComponentInParent<PlayerStatsData>();
            if (playerStats != slowedPlayerStats) return;

            ClearSlow();
            canHit = true;
        }

        private void StartSlow(PlayerStatsData playerStats)
        {
            canHit = false;

            slowedPlayerStats = playerStats;
            originalPlayerSpeed = playerStats.movementSpeed;
            playerStats.movementSpeed *= slowMultiplier;
        }

        private void HandleAnyPlayerPolaritySwitched(Transform playerTransform)
        {
            if (!ShouldShakeOffFrom(playerTransform)) return;

            ClearSlow();
            KnockbackFromPlayer(playerTransform);
        }

        private bool ShouldShakeOffFrom(Transform playerTransform)
        {
            if (playerTransform == null) return false;

            if (slowedPlayerStats != null && slowedPlayerStats.transform == playerTransform)
            {
                return true;
            }

            Vector3 flatOffset = transform.position - playerTransform.position;
            flatOffset.y = 0f;
            return flatOffset.sqrMagnitude <= shakeOffRadius * shakeOffRadius;
        }

        private void ClearSlow()
        {
            if (slowedPlayerStats != null)
            {
                slowedPlayerStats.movementSpeed = originalPlayerSpeed;
            }

            slowedPlayerStats = null;
        }

        private void KnockbackFromPlayer(Transform player)
        {
            if (agent == null) return;

            Vector3 direction = transform.position - player.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
            {
                direction = -player.forward;
            }

            if (knockbackRoutine != null) return;

            knockbackRoutine = StartCoroutine(KnockbackRoutine(direction.normalized));
        }

        private IEnumerator KnockbackRoutine(Vector3 knockbackDirection)
        {
            bool pursuitWasEnabled = pursuitAI != null && pursuitAI.enabled;

            if (enemyCollider != null)
            {
                enemyCollider.enabled = false;
            }

            if (pursuitAI != null)
            {
                pursuitAI.enabled = false;
            }

            agent.ResetPath();
            agent.isStopped = false;
            pusher.PushBack(knockbackDirection, knockbackDistance, knockbackDuration);

            yield return new WaitForSeconds(knockbackDuration);

            agent.ResetPath();
            agent.isStopped = true;

            yield return new WaitForSeconds(stunDuration);

            if (pursuitAI != null)
            {
                pursuitAI.enabled = pursuitWasEnabled;
            }

            agent.isStopped = false;
            canHit = true;

            if (enemyCollider != null)
            {
                enemyCollider.enabled = true;
            }

            knockbackRoutine = null;
        }

        private void OnDisable()
        {
            PlayerPolarityController.OnAnyPlayerPolaritySwitched -= HandleAnyPlayerPolaritySwitched;

            if (knockbackRoutine != null)
            {
                StopCoroutine(knockbackRoutine);
                knockbackRoutine = null;
            }

            ClearSlow();
            canHit = true;
        }
    }
}
