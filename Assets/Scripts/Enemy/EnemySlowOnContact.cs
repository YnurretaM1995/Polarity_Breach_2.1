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
        [SerializeField] private float slowRadius = 2f;
        [SerializeField] private float slowRefreshDuration = 0.15f;
        [SerializeField] private string playerTag = "Player";

        [Header("Knockback")]
        [SerializeField] private float knockbackDistance = 3f;
        [SerializeField] private float knockbackDuration = 0.2f;
        [SerializeField] private float shakeOffRadius = 2.5f;
        [SerializeField] private float stunDuration = 0.5f;

        private NavMeshAgent agent;
        private NavMeshAgentPusher pusher;
        private EnemyPursuitAI pursuitAI;
        private Collider enemyCollider;
        private PlayerStatsData targetPlayerStats;
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
            FindPlayerIfMissing();
        }

        private void Update()
        {
            TryApplySlowInRadius();
        }

        private void OnTriggerStay(Collider other)
        {
            TryApplySlow(other);
        }

        private void OnCollisionStay(Collision collision)
        {
            TryApplySlow(collision.collider);
        }

        private void TryApplySlow(Collider other)
        {
            PlayerStatsData playerStats = other.GetComponentInParent<PlayerStatsData>();
            if (playerStats == null) return;

            playerStats.ApplyTemporaryMovementSlow(this, slowRefreshDuration);
        }

        private void TryApplySlowInRadius()
        {
            if (knockbackRoutine != null) return;

            FindPlayerIfMissing();
            if (targetPlayerStats == null) return;

            Vector3 flatOffset = transform.position - targetPlayerStats.transform.position;
            flatOffset.y = 0f;

            if (flatOffset.sqrMagnitude <= slowRadius * slowRadius)
            {
                targetPlayerStats.ApplyTemporaryMovementSlow(this, slowRefreshDuration);
            }
        }

        private void FindPlayerIfMissing()
        {
            if (targetPlayerStats != null) return;

            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player == null) return;

            targetPlayerStats = player.GetComponentInParent<PlayerStatsData>();
        }

        private void HandleAnyPlayerPolaritySwitched(Transform playerTransform)
        {
            if (!ShouldShakeOffFrom(playerTransform)) return;

            PlayerStatsData playerStats = playerTransform.GetComponentInParent<PlayerStatsData>();
            if (playerStats != null)
            {
                playerStats.ResetMovementSpeedMultiplier();
            }

            KnockbackFromPlayer(playerTransform);
        }

        private bool ShouldShakeOffFrom(Transform playerTransform)
        {
            if (playerTransform == null) return false;

            Vector3 flatOffset = transform.position - playerTransform.position;
            flatOffset.y = 0f;
            return flatOffset.sqrMagnitude <= shakeOffRadius * shakeOffRadius;
        }

        private void KnockbackFromPlayer(Transform player)
        {
            if (agent == null) return;
            if (knockbackRoutine != null) return;

            Vector3 direction = transform.position - player.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
            {
                direction = -player.forward;
            }

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
        }
    }
}
