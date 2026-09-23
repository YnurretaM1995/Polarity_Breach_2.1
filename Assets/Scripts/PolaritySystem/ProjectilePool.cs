using System.Collections.Generic;
using UnityEngine;

namespace PolarityBreach.PolaritySystem
{
    public class ProjectilePool : MonoBehaviour
    {
        [SerializeField] private ShootProjectile projectilePrefab;
        [SerializeField] private int poolSize = 30;

        private List<ShootProjectile> projectiles = new List<ShootProjectile>();

        private void Awake()
        {
            for (int i = 0; i < poolSize; i++)
            {
                ShootProjectile projectile = Instantiate(projectilePrefab, transform);
                projectile.gameObject.SetActive(false);
                projectiles.Add(projectile);
            }
        }

        public ShootProjectile GetProjectile(Vector3 position, Quaternion rotation)
        {
            ShootProjectile projectile = GetInactiveProjectile(position, rotation);

            if (projectile == null) return null;

            projectile.gameObject.SetActive(true);
            return projectile;
        }

        public ShootProjectile GetInactiveProjectile(Vector3 position, Quaternion rotation)
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                if (!projectiles[i].gameObject.activeInHierarchy)
                {
                    projectiles[i].transform.SetPositionAndRotation(position, rotation);
                    return projectiles[i];
                }
            }

            Debug.LogWarning("No inactive projectile available.");
            return null;
        }
    }
}