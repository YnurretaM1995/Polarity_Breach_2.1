using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPursuitAI : MonoBehaviour
{
    private enum State { Idle, Chase, HoldRange }

    [Header("Target")]
    [Tooltip("Leave empty to auto-find object tagged 'Player'.")]
    [SerializeField] private Transform target;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 18f;
    [SerializeField] private float loseTargetRadius = 20f;
    [SerializeField] private bool requireLineOfSight = true;
    [SerializeField] private LayerMask obstacleLayers;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float angularSpeed = 720f;
    [Tooltip("Distance from target the agent stops. Set to 0 for melee enemies that need to physically touch the player to deal contact damage. Ranged enemies typically don't use this (they stop via preferredDistance instead).")]
    [SerializeField] private float stoppingDistance = 0f;

    [Header("Enemy Type")]
    [Tooltip("Melee = walks into player. Ranged = stops/backs off at preferredDistance.")]
    [SerializeField] private bool isRangedEnemy = false;
    [SerializeField] private float preferredDistance = 12f;
    [SerializeField] private float preferredDistanceBuffer = 0.75f; 

    [Header("Idle Behaviour")]
    [Tooltip("If true, enemy stands still until it detects the player. If false, add your own patrol logic.")]
    [SerializeField] private bool idleUntilDetected = true;

    private NavMeshAgent agent;
    private State state = State.Idle;
    private bool canSeeTargetCached;

    public Transform Target => target;
    public bool IsEngaged => state == State.Chase || state == State.HoldRange;
    public bool CanSeeTarget => canSeeTargetCached;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ApplyAgentSettings();

        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) target = playerObj.transform;
        }
    }

    private void ApplyAgentSettings()
    {
        if (agent == null) return;

        agent.speed = moveSpeed;
        agent.acceleration = acceleration;
        agent.angularSpeed = angularSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.updateRotation = true;
        agent.updateUpAxis = false;
    }

    private void OnEnable()
    {
        state = State.Idle;
        canSeeTargetCached = false;
    }

    public void ResetForPool()
    {
        state = State.Idle;
        if (agent != null)
        {
            agent.ResetPath();
            agent.isStopped = false;
        }
    }

    private void Update()
    {
        if (target == null) return;

        float distToTarget = Vector3.Distance(transform.position, target.position);
        bool canSeeTarget = !requireLineOfSight || HasLineOfSight();
        canSeeTargetCached = canSeeTarget;

        switch (state)
        {
            case State.Idle:
                if (idleUntilDetected)
                    agent.isStopped = true;

                if (distToTarget <= detectionRadius && canSeeTarget)
                    state = State.Chase;
                break;

            case State.Chase:
                agent.isStopped = false;

                if (distToTarget > loseTargetRadius || !canSeeTarget)
                {
                    state = idleUntilDetected ? State.Idle : State.Chase;
                    if (idleUntilDetected) break;
                }

                if (isRangedEnemy && distToTarget <= preferredDistance + preferredDistanceBuffer)
                {
                    state = State.HoldRange;
                    break;
                }

                agent.SetDestination(target.position);
                break;

            case State.HoldRange:
                if (distToTarget > preferredDistance + preferredDistanceBuffer)
                {
                    state = State.Chase;
                }
                else if (distToTarget < preferredDistance - preferredDistanceBuffer)
                {
                    Vector3 fleeDir = (transform.position - target.position).normalized;
                    Vector3 fleeTarget = transform.position + fleeDir * (preferredDistance - distToTarget);
                    agent.isStopped = false;
                    agent.SetDestination(fleeTarget);
                }
                else
                {
                    agent.isStopped = true;
                    FaceTarget();
                }

                if (distToTarget > loseTargetRadius || !canSeeTarget)
                    state = idleUntilDetected ? State.Idle : State.Chase;
                break;
        }
    }

    private void FaceTarget()
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f);
    }

    private bool HasLineOfSight()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 destination = target.position + Vector3.up * 0.5f;
        Vector3 dir = destination - origin;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dir.magnitude, obstacleLayers))
        {
            return false;
        }
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseTargetRadius);

        if (isRangedEnemy)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, preferredDistance);
        }
    }

    private void OnValidate()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        ApplyAgentSettings();

        if (isRangedEnemy && detectionRadius <= preferredDistance + preferredDistanceBuffer)
        {
            Debug.LogWarning($"[{name}] EnemyPursuitAI: detectionRadius ({detectionRadius}) should be " +
                $"larger than preferredDistance + buffer ({preferredDistance + preferredDistanceBuffer}), " +
                "otherwise the enemy will detect the player already 'too close' and flee instead of chasing first.", this);
        }
    }
}
