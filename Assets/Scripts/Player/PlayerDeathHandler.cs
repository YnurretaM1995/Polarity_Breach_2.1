using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using PolarityBreach.UI;

namespace PolarityBreach.Player
{
    public class PlayerDeathHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private MonoBehaviour[] scriptsToDisable;

        [Header("Freeze On Death")]
        [SerializeField] private Transform[] enemyContainers;
        [SerializeField] private GameObject enemyProjectilePool;

        [Header("Timing")]
        [SerializeField] private float deathAnimationDuration = 2f;
        [SerializeField] private float groundHoldDuration = 1.5f;

        private PlayerHealth health;
        private Rigidbody rb;

        private void Awake()
        {
            health = GetComponent<PlayerHealth>();
            rb = GetComponent<Rigidbody>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        private void OnEnable()
        {
            health.OnDied += HandleDeath;
        }

        private void OnDisable()
        {
            health.OnDied -= HandleDeath;
        }

        private void HandleDeath()
        {
            StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {
            FreezeEnemies();

            if (enemyProjectilePool != null)
                enemyProjectilePool.SetActive(false);

            for (int i = 0; i < scriptsToDisable.Length; i++)
            {
                if (scriptsToDisable[i] != null)
                    scriptsToDisable[i].enabled = false;
            }

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (animator != null)
                animator.SetTrigger("Death");

            yield return new WaitForSeconds(deathAnimationDuration + groundHoldDuration);

            GameOverScreen.Show();
        }

        private void FreezeEnemies()
        {
            for (int i = 0; i < enemyContainers.Length; i++)
            {
                if (enemyContainers[i] == null) continue;

                MonoBehaviour[] scripts = enemyContainers[i].GetComponentsInChildren<MonoBehaviour>(true);
                for (int s = 0; s < scripts.Length; s++)
                {
                    if (scripts[s] != null) scripts[s].enabled = false;
                }

                NavMeshAgent[] agents = enemyContainers[i].GetComponentsInChildren<NavMeshAgent>(true);
                for (int a = 0; a < agents.Length; a++)
                {
                    if (agents[a] != null && agents[a].isOnNavMesh) agents[a].isStopped = true;
                }
            }
        }
    }
}