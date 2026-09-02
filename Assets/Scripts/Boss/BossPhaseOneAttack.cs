using System.Collections;
using PolarityBreach.Audio;
using PolarityBreach.Enemy;
using PolarityBreach.PolaritySystem;
using UnityEngine;

namespace PolarityBreach.Boss
{
    public class BossPhaseOneAttack : MonoBehaviour
    {
        [SerializeField] private EnemyProjectilePool projectilePool;
        [SerializeField] private Transform firePoint;

        [SerializeField] private BossPhaseData phaseData;

        [Header("SFX")]
        [SerializeField] private AudioClip[] normalShotSounds;
        [SerializeField, Range(0f, 1f)] private float normalShotSoundVolume = 1f;
        [SerializeField] private bool playNormalShotSoundAs2D;
        
        private float attackStartAngle;
        private bool useInstantCircle = true;
        private Coroutine _phaseRoutine;

        public void StartPhase()
        {
            if (_phaseRoutine != null) return;
            
            if (phaseData == null)
            {
                return;
            }

            if (firePoint == null)
            {
                firePoint = transform;
            }

            _phaseRoutine = StartCoroutine(PhaseOneLoop());
        }

        public void StopPhase()
        {
            if (_phaseRoutine != null)
            {
                StopCoroutine(_phaseRoutine);
                _phaseRoutine = null;
            }
        }

        private Polarity GetRandomPolarity()
        {
            return Random.value < 0.5d ? Polarity.White : Polarity.Black;
        }

        private IEnumerator PhaseOneLoop()
        {
            while (true)
            {
                Polarity randomPolarity = GetRandomPolarity();

                if (useInstantCircle)
                {
                    FireCircleInstant(randomPolarity);
                }
                else
                {
                    yield return StartCoroutine(FireCircleSequence(randomPolarity));
                }

                useInstantCircle = !useInstantCircle;

                yield return new WaitForSeconds(phaseData.timeBetweenAttacks);
            }
        }

        private void FireCircleInstant(Polarity polarity)
        {
            bool firedAnyProjectile = false;

            for (int i = 0; i < phaseData.instantBulletCount; i++)
            {
                float angle = attackStartAngle + 360f / phaseData.instantBulletCount * i;
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

                firedAnyProjectile |= FireProjectile(direction, polarity, phaseData.instantProjectileSpeed, phaseData.instantProjectileDamage);
            }

            if (firedAnyProjectile)
            {
                PlayNormalShotSfx();
            }

            attackStartAngle += phaseData.attackStartAngleOffset;
        }

        private IEnumerator FireCircleSequence(Polarity polarity)
        {
            for (int i = 0; i < phaseData.sequenceBulletCount; i++)
            {
                float angle = attackStartAngle + ((360f / phaseData.sequenceBulletCount) * i);
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

                if (FireProjectile(direction, polarity, phaseData.sequenceProjectileSpeed, phaseData.sequenceProjectileDamage))
                {
                    PlayNormalShotSfx();
                }

                yield return new WaitForSeconds(phaseData.sequenceBulletDelay);
            }

            attackStartAngle += phaseData.attackStartAngleOffset;
        }
        
        private bool FireProjectile(Vector3 direction, Polarity polarity, float projectileSpeed, float projectileDamage)
        {
            if (projectilePool == null)
            {
                return false;
            }

            Projectile projectile = projectilePool.GetProjectile(firePoint.position, Quaternion.LookRotation(direction));

            if (projectile == null) return false;

            PolarityComponent projectilePolarity = projectile.GetComponent<PolarityComponent>();

            if (projectilePolarity != null)
            {
                projectilePolarity.SetPolarity(polarity);
            }

            projectile.Launch(direction, projectileSpeed, projectileDamage);
            return true;
        }

        private AudioClip GetRandomNormalShotSound()
        {
            if (normalShotSounds != null && normalShotSounds.Length > 0)
            {
                return normalShotSounds[Random.Range(0, normalShotSounds.Length)];
            }

            return null;
        }

        private void PlayNormalShotSfx()
        {
            AudioClip clip = GetRandomNormalShotSound();
            if (clip == null) return;

            if (playNormalShotSoundAs2D)
            {
                AudioHandler.Play2DSound(clip, normalShotSoundVolume);
                return;
            }

            Vector3 position = firePoint != null ? firePoint.position : transform.position;
            AudioHandler.Play3DSound(clip, position, normalShotSoundVolume);
        }
    }
}
