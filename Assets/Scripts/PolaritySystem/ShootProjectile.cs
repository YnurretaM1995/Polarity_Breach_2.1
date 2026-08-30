using PolarityBreach.Audio;
using PolarityBreach.Boss;
using PolarityBreach.Enemy;
using PolarityBreach.Feedback;
using UnityEngine;

namespace PolarityBreach.PolaritySystem
{
    
    [RequireComponent(typeof(PolarityComponent))]
    public class ShootProjectile : MonoBehaviour
    {
        [SerializeField] private float _lifeTime = 3f;
        [SerializeField] private bool _disapearOnHit = true;
        [SerializeField] private GameObject _impactEffect;
        [SerializeField] private AudioClip[] _impactSounds;
        [SerializeField] private AudioClip _impactSound;
        [SerializeField, Range(0f, 1f)] private float _impactSoundVolume = 1f;
        [SerializeField] private bool _playImpactSoundAs2D;
        
        private float _speed;
        private float _damage;
        private float _knockbackForce;
        
        private PolarityComponent _polarity;
        private float _spawnTime;

        private void Awake() => _polarity = GetComponent<PolarityComponent>();
        private void OnEnable() => _spawnTime = Time.time;

        private void Update()
        {
            transform.position += transform.forward * (_speed * Time.deltaTime);
            if (Time.time - _spawnTime >= _lifeTime) gameObject.SetActive(false);
        }

        public void SetStats(float speed, float damage, float knockbackForce)
        {
            _speed = speed;
            _damage = damage;
            _knockbackForce = knockbackForce;
        }

        private void OnTriggerEnter(Collider other)
        {
            BossShield bossShield = other.GetComponentInParent<BossShield>();
            if (bossShield != null && bossShield.IsInvulnerable)
            {
                if (_disapearOnHit)
                {
                    gameObject.SetActive(false);
                }

                return;
            }

            BossWeakPoint bossWeakPoint = other.GetComponentInParent<BossWeakPoint>();
            if (bossWeakPoint != null && !bossWeakPoint.CanTakeDamage)
            {
                if (_disapearOnHit)
                {
                    gameObject.SetActive(false);
                }

                return;
            }

            EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
            bool hit = DamageSystem.TryApplyDamage(_polarity, other.gameObject, _damage);
            if (hit)
            {
                Rigidbody rb = other.GetComponent<Rigidbody>();
                Vector3 impactDirection = transform.forward;
                FeedbackHandler.SpawnParticles(_impactEffect, transform.position,impactDirection);

                if (enemyHealth == null || !enemyHealth.IsDead)
                {
                    PlayImpactSfx();
                }

                if (rb != null)
                {
                    Vector3 knockbackDirection = transform.forward;
                    knockbackDirection.y = 0;
                    knockbackDirection.Normalize();
                    
                    rb.AddForce(knockbackDirection * _knockbackForce, ForceMode.Impulse);
                }
                
                other.GetComponentInParent<PolarityVisual>()?.FlashHit();

                if (_disapearOnHit)
                {
                    gameObject.SetActive(false);
                }
            }
        }

        private AudioClip GetRandomImpactSound()
        {
            if (_impactSounds != null && _impactSounds.Length > 0)
            {
                return _impactSounds[Random.Range(0, _impactSounds.Length)];
            }

            return _impactSound;
        }

        private void PlayImpactSfx()
        {
            AudioClip clip = GetRandomImpactSound();

            if (_playImpactSoundAs2D)
            {
                AudioHandler.Play2DSound(clip, _impactSoundVolume);
                return;
            }

            AudioHandler.Play3DSound(clip, transform.position, _impactSoundVolume);
        }
    }
}
