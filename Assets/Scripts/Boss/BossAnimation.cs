using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace PolarityBreach.Boss
{
    public class BossAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private BossHealth health;

        [Header("Attack Animations (visual)")]
        [SerializeField] private float minTimeBetweenAnimations = 3f;
        [SerializeField] private float maxTimeBetweenAnimations = 6f;

        [Header("Take Hit")]
        [SerializeField] private float takeHitCooldown = 0.4f;

        private float lastTakeHitTime = float.NegativeInfinity;
        private Coroutine attackRoutine;
        private bool isDead;

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (agent == null) agent = GetComponent<NavMeshAgent>();
            if (health == null) health = GetComponent<BossHealth>();
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.OnDamaged += PlayTakeHit;
                health.OnDied += PlayDeath;
            }

            attackRoutine = StartCoroutine(RandomAttackLoop());
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDamaged -= PlayTakeHit;
                health.OnDied -= PlayDeath;
            }

            if (attackRoutine != null) StopCoroutine(attackRoutine);
        }

        private void Update()
        {
            if (animator == null || isDead) return;
            animator.SetFloat("Speed", agent != null ? agent.velocity.magnitude : 0f);
        }

        private IEnumerator RandomAttackLoop()
        {
            while (!isDead)
            {
                yield return new WaitForSeconds(Random.Range(minTimeBetweenAnimations, maxTimeBetweenAnimations));
                if (isDead) yield break;

                animator.SetTrigger(Random.value < 0.5f ? "Attack" : "Jump");
            }
        }

        private void PlayTakeHit()
        {
            if (isDead) return;
            if (Time.time < lastTakeHitTime + takeHitCooldown) return;

            lastTakeHitTime = Time.time;
            animator.SetTrigger("TakeHit");
        }

        private void PlayDeath()
        {
            isDead = true;
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            animator.SetTrigger("Death");
        }
    }
}