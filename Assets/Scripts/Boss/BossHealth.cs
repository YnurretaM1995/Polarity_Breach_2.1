using System;
using UnityEngine;
using System.Collections;

namespace PolarityBreach.Boss
{
    public class BossHealth : MonoBehaviour
    {
        [Header("Boss Health")] [SerializeField]
        private float maxHealth = 150f;

        [SerializeField] private float currentHealth;

        [Header("Boss WeakPoints")] [SerializeField]
        private BossWeakPoint[] weakPoints;
        
        [Header("Boss Shield")]
        public bool IsShielded {get; private set;}

        private bool isDead;

        public event Action OnDamaged;
        public event Action OnDied;
        public event Action<float> OnHealthPercentChanged;
        public event Action OnWeakPointDestroyed;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => isDead;


        [Header("Death")]
        [SerializeField] private float deathAnimationDuration = 2f;

        void Awake()
        {
            InitializeWeakPoints();
            RefreshHealth();
        }

        private void InitializeWeakPoints()
        {
            if (weakPoints == null || weakPoints.Length == 0) return;

            float healthPerWeakPoint = maxHealth / weakPoints.Length;

            for (int i = 0; i < weakPoints.Length; i++)
            {
                if (weakPoints[i] == null) continue;

                weakPoints[i].Initialize(this, healthPerWeakPoint);
            }
        }

        public void RefreshHealth()
        {
            currentHealth = 0f;
            
            for (int i = 0; i < weakPoints.Length; i++)
            {
                if (weakPoints[i] == null) continue;

                currentHealth += weakPoints[i].CurrentHealth;
            }
            
            float healthPercent = currentHealth / Mathf.Max(maxHealth, 1f);
            OnHealthPercentChanged?.Invoke(healthPercent);

            if (currentHealth <= 0f && !isDead)
            {
                isDead = true;
                OnDied?.Invoke();
                Debug.Log("Boss Defeated");
                //StartCoroutine(DisableAfterDeathAnimation());
                //gameObject.SetActive(false);
            }
        }

        private IEnumerator DisableAfterDeathAnimation()
        {
            yield return new WaitForSeconds(deathAnimationDuration);
            gameObject.SetActive(false);
        }

        public void WeakPointDestroyed()
        {
            RefreshHealth();

            if (!isDead)
            {
                OnWeakPointDestroyed?.Invoke();
            }
        }

        public void SetShielded(bool shielded)
        {
            IsShielded = shielded;
        }

        public void NotifyDamaged()
        {
            if (isDead) return;
            OnDamaged?.Invoke();
        }
    }
}
