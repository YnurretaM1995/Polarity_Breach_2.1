using PolarityBreach.Audio;
using PolarityBreach.PolaritySystem;
using UnityEngine;

namespace PolarityBreach.Enemy
{
    [RequireComponent(typeof(EnemyPursuitAI))]
    public class EnemyShooter : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Transform firePoint;

        [SerializeField] private EnemyProjectilePool projectilePool;
        [SerializeField] private EnemyAnimation enemyAnimation;

        [Header("Firing")] [SerializeField] private float fireRate = 1.5f;
        [SerializeField] private float projectileSpeed = 12f;
        [SerializeField] private float maxFireRange = 12f;
        [SerializeField] private int damage = 10;
        [SerializeField] private int projectilesPerShot = 3;
        [SerializeField] private float spreadAngle = 15f;

        [SerializeField] private AudioClip shootSound;

        private EnemyPursuitAI pursuitAI;
        private float fireCooldown;
        private Collider[] ownColliders;
        private PolarityComponent _polarity;

        private void Awake()
        {
            pursuitAI = GetComponent<EnemyPursuitAI>();
            _polarity = GetComponent<PolarityComponent>();
            if (enemyAnimation == null) enemyAnimation = GetComponent<EnemyAnimation>();
            if (firePoint == null) firePoint = transform;
            ownColliders = GetComponentsInChildren<Collider>();

            if (projectilePool == null)
            {
                GameObject poolObject = GameObject.Find("EnemyProjectilePool");

                if (poolObject != null)
                {
                    projectilePool = poolObject.GetComponent<EnemyProjectilePool>();
                }
            }
        }

        private void Update()
        {
            if (fireCooldown > 0f)
                fireCooldown -= Time.deltaTime;

            if (!CanFire()) return;

            if (fireCooldown <= 0f)
            {
                Fire();
                fireCooldown = 1f / Mathf.Max(fireRate, 0.01f);
            }
        }

        private bool CanFire()
        {
            if (pursuitAI.Target == null) return false;
            if (!pursuitAI.IsEngaged) return false;
            if (!pursuitAI.CanSeeTarget) return false;

            float dist = Vector3.Distance(transform.position, pursuitAI.Target.position);
            return dist <= maxFireRange;
        }

        private void Fire()
        {
            if (projectilePool == null || pursuitAI.Target == null) return;
            if (enemyAnimation != null) enemyAnimation.PlayAttack();

            Vector3 targetPoint = pursuitAI.Target.position + Vector3.up * 0.5f;
            Vector3 baseDirection = (targetPoint - firePoint.position).normalized;

            float startAngle = -spreadAngle;
            float angleStep = projectilesPerShot > 1 ? (spreadAngle * 2f) / (projectilesPerShot - 1) : 0f;

            for (int i = 0; i < projectilesPerShot; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * baseDirection;

                Projectile proj = projectilePool.GetProjectile(firePoint.position, Quaternion.LookRotation(dir));
                if (proj == null) return;

                var projPolarity = proj.GetComponent<PolarityComponent>();
                if (projPolarity != null && _polarity != null)
                    projPolarity.SetPolarity(_polarity.CurrentPolarity);


                Collider projCollider = proj.GetComponent<Collider>();
                if (projCollider != null)
                {
                    foreach (Collider ownCollider in ownColliders)
                    {
                        if (ownCollider != null)
                            Physics.IgnoreCollision(projCollider, ownCollider);
                    }
                }

                AudioHandler.Play3DSound(shootSound, transform.position);

                proj.Launch(dir, projectileSpeed, damage);
            }
        }
    }
}