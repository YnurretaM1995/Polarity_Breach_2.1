using UnityEngine;
using UnityEngine.AI;

namespace PolarityBreach.Boss
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class BossMovement : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float stoppingDistance = 6f;
        [SerializeField] private float repathInterval = 0.25f;

        private NavMeshAgent agent;
        private BossHealth health;
        private float nextRepathTime;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<BossHealth>();
            agent.stoppingDistance = stoppingDistance;

            if (target == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) target = playerObj.transform;
            }
        }

        private void Update()
        {
            if (target == null || health == null) return;

            if (health.IsDead)
            {
                if (agent.isOnNavMesh) agent.isStopped = true;
                return;
            }

            if (Time.time >= nextRepathTime)
            {
                nextRepathTime = Time.time + repathInterval;
                if (agent.isOnNavMesh) agent.SetDestination(target.position);
            }
        }
    }
}