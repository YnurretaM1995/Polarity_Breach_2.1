using PolarityBreach.PolaritySystem;
using UnityEngine;
using PolarityBreach.PolaritySystem.Interfaces;

namespace PolarityBreach.Boss
{
    [RequireComponent(typeof(PolarityComponent))]
    public class BossWeakPoint : MonoBehaviour, IDamageable
    {
        [SerializeField] private Collider weakPointCollider;

        [SerializeField] private float maxHealth;
        [SerializeField] private float currentHealth;
        

        private BossHealth bossHealth;
        private bool isDestroyed;
        
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDestroyed => isDestroyed;
        public bool CanTakeDamage => !isDestroyed && bossHealth != null && !bossHealth.IsShielded;

        [Header("Destroyed Indicator")]
        [SerializeField] private Renderer orbRenderer;
        [SerializeField] private Material deadMaterial;

        void Awake()
        {
            if (weakPointCollider == null)
            {
                weakPointCollider = GetComponent<Collider>();
            }
        }
        
        public void Initialize(BossHealth owner, float healthAmount)
        {
            bossHealth = owner;
            maxHealth = healthAmount;
            currentHealth = maxHealth;
            isDestroyed = false;

            if (weakPointCollider != null)
            {
                weakPointCollider.enabled = true;
            }
        }

        public void TakeDamage(float amount)
        {
            if (isDestroyed) return;
            if (bossHealth.IsShielded) return;

            currentHealth -= amount;
            currentHealth = Mathf.Max(currentHealth, 0f);

            bossHealth.NotifyDamaged();
            bossHealth.RefreshHealth();

            if (currentHealth <= 0f)
            {
                DestroyWeakPoint();
            }
        }
        
        private void DestroyWeakPoint()
        {
            isDestroyed = true;

            if (weakPointCollider != null)
            {
                weakPointCollider.enabled = false;
            }

            SetOrbDead();
            bossHealth.WeakPointDestroyed();
            
            Debug.Log(gameObject.name + " weak point destroyed.");
        }

        private void SetOrbDead()
        {
            if (orbRenderer == null) return;

            PolarityVisual visual = orbRenderer.GetComponentInParent<PolarityVisual>();
            if (visual != null) visual.enabled = false;

            if (deadMaterial != null)
            {
                Material[] mats = new Material[orbRenderer.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++) mats[i] = deadMaterial;
                orbRenderer.materials = mats;
            }
            else
            {
                orbRenderer.material.color = Color.red;
            }
        }
    }
}
