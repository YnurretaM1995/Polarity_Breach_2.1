using PolarityBreach.Audio;
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

        [Header("SFX")]
        [SerializeField] private AudioClip[] bossScreamSounds;
        [SerializeField] private AudioClip bossScreamSound;
        [SerializeField, Range(0f, 1f)] private float bossScreamSoundVolume = 1f;
        [SerializeField] private AudioClip bossDefeatedSound;
        [SerializeField, Range(0f, 1f)] private float bossDefeatedSoundVolume = 1f;
        [SerializeField] private bool playBossSoundsAs2D;

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
                PlayBossSfx(bossDefeatedSound, bossDefeatedSoundVolume);
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
                PlayBossSfx(GetRandomBossScreamSound(), bossScreamSoundVolume);
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

        private void PlayBossSfx(AudioClip clip, float volume)
        {
            if (playBossSoundsAs2D)
            {
                AudioHandler.Play2DSound(clip, volume);
            }
            else
            {
                AudioHandler.Play3DSound(clip, transform.position, volume);
            }
        }

        private AudioClip GetRandomBossScreamSound()
        {
            if (bossScreamSounds != null && bossScreamSounds.Length > 0)
            {
                return bossScreamSounds[UnityEngine.Random.Range(0, bossScreamSounds.Length)];
            }

            return bossScreamSound;
        }
    }
}
