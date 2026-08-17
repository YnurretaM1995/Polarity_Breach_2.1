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
        [SerializeField] private BossMovement movement;

        [Header("Attack Animations (visual)")]
        [SerializeField] private float minTimeBetweenAnimations = 3f;
        [SerializeField] private float maxTimeBetweenAnimations = 6f;
        [SerializeField] private float attackAnimationDuration = 1f;

        [Header("Take Hit")]
        [SerializeField] private float takeHitCooldown = 0.4f;
        [SerializeField] private float takeHitDuration = 0.5f;

        private float lastTakeHitTime = float.NegativeInfinity;
        private Coroutine attackRoutine;
        private Coroutine busyRoutine;
        private bool isDead;

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (agent == null) agent = GetComponent<NavMeshAgent>();
            if (health == null) health = GetComponent<BossHealth>();
            if (movement == null) movement = GetComponent<BossMovement>();
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
                SetBusy(attackAnimationDuration);

                yield return new WaitForSeconds(attackAnimationDuration);
            }
        }

        private void PlayTakeHit()
        {
            if (isDead) return;
            if (Time.time < lastTakeHitTime + takeHitCooldown) return;

            lastTakeHitTime = Time.time;
            animator.SetTrigger("TakeHit");
            SetBusy(takeHitDuration);
        }

        private void PlayDeath()
        {
            isDead = true;
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            animator.SetTrigger("Death");
        }

        private void SetBusy(float duration)
        {
            if (busyRoutine != null) StopCoroutine(busyRoutine);
            busyRoutine = StartCoroutine(BusyRoutine(duration));
        }


        private IEnumerator BusyRoutine(float duration)
        {
            if (movement != null) movement.IsBusy = true;
            yield return new WaitForSeconds(duration);
            if (movement != null && !isDead) movement.IsBusy = false;
            busyRoutine = null;
        }
    }
}