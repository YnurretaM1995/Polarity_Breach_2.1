using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using PolarityBreach.Menus;
using PolarityBreach.UI;
using PolarityBreach.Audio;
using UnityEngine.Serialization;

namespace PolarityBreach.Player
{
    public class PlayerDeathHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private GameMusicController musicController;
        [SerializeField] private MonoBehaviour[] scriptsToDisable;

        [Header("Animation")]
        [SerializeField] private string deathTriggerName = "Death";
        [SerializeField] private string deathStateName = "Death";
        [SerializeField] private float deathTransitionDuration = 0.05f;

        [Header("Freeze On Death")]
        [SerializeField] private Transform[] enemyContainers;
        [SerializeField] private GameObject enemyProjectilePool;

        [Header("Timing")]
        [SerializeField] private float deathAnimationDuration = 2f;
        [SerializeField] private float groundHoldDuration = 1.5f;

        [FormerlySerializedAs("deathSFX")]
        [Header("SFX")] 
        [SerializeField] private AudioClip deathSfx;

        private PlayerHealth health;
        private Rigidbody rb;

        private void Awake()
        {
            health = GetComponent<PlayerHealth>();
            rb = GetComponent<Rigidbody>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (musicController == null)
                musicController = FindFirstObjectByType<GameMusicController>();
        }

        private void OnEnable()
        {
            if (health == null) return;

            health.OnDied += HandleDeath;
        }

        private void OnDisable()
        {
            if (health == null) return;

            health.OnDied -= HandleDeath;
        }

        private void HandleDeath()
        {
            StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {
            if (musicController != null)
                musicController.StopMusic();

            AudioHandler.Play3DSound(deathSfx, transform.position);
            
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

            PlayDeathAnimation();

            yield return new WaitForSecondsRealtime(deathAnimationDuration + groundHoldDuration);

            GameOverScreen.Show();
        }

        private void PlayDeathAnimation()
        {
            if (animator == null)
            {
                Debug.LogWarning("PlayerDeathHandler: No Animator assigned for death animation.");
                return;
            }

            animator.enabled = true;
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.SetFloat("Speed", 0f);
            animator.ResetTrigger(deathTriggerName);
            animator.SetTrigger(deathTriggerName);

            int deathStateHash = Animator.StringToHash(deathStateName);
            if (animator.HasState(0, deathStateHash))
            {
                animator.CrossFadeInFixedTime(deathStateHash, deathTransitionDuration, 0, 0f);
            }

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
