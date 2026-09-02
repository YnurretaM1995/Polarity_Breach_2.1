using System.Collections;
using PolarityBreach.Audio;
using UnityEngine;

namespace PolarityBreach.Boss
{
    public class BossPhaseTwoAttack : MonoBehaviour
    {
        [SerializeField] private Transform beamPivot;
        [SerializeField] private GameObject[] beams;
        [SerializeField] private GameObject[] warningBeams;
        [SerializeField] private float warningDuration = 1f;
        [SerializeField] private float beamDuration = 4f;
        [SerializeField] private float timeBetweenBeamAttacks = 2f;
        [SerializeField] private float rotationSpeed = 20f;

        [Header("SFX")]
        [SerializeField] private AudioSource beamSfxSource;
        [SerializeField] private AudioClip beamAttackSound;
        [SerializeField, Range(0f, 1f)] private float beamAttackSoundVolume = 1f;
        [SerializeField] private bool playBeamAttackSoundAs2D;
        
        private bool isActive;
        private Coroutine phaseRoutine;
        private float rotationDirection = 1f;
        private bool useCustomRotationSpeed;
        private float customRotationSpeed;

        private void Awake()
        {
            if (beamSfxSource == null)
                beamSfxSource = CreateBeamSfxSource();

            SetWarningBeamsActive(false);
            SetBeamsActive(false);
        }

        public void StartPhase()
        {
            if (isActive || phaseRoutine != null) return;

            useCustomRotationSpeed = false;
            rotationDirection = 1f;
            phaseRoutine = StartCoroutine(PhaseTwoLoop());
        }

        public void StartPhase(float customRotationSpeed)
        {
            if (isActive || phaseRoutine != null) return;

            useCustomRotationSpeed = true;
            this.customRotationSpeed = customRotationSpeed;
            rotationDirection = 1f;
            phaseRoutine = StartCoroutine(PhaseTwoLoop());
        }

        public void SetCustomRotationSpeed(float newRotationSpeed)
        {
            customRotationSpeed = newRotationSpeed;
        }

        public void StopPhase()
        {
            if (phaseRoutine != null)
            {
                StopCoroutine(phaseRoutine);
                phaseRoutine = null;
            }

            isActive = false;
            SetWarningBeamsActive(false);
            SetBeamsActive(false);
        }

        private IEnumerator PhaseTwoLoop()
        {
            while (true)
            {
                isActive = false;
                SetBeamsActive(false);
                SetWarningBeamsActive(true);

                yield return new WaitForSeconds(warningDuration);

                SetWarningBeamsActive(false);
                SetBeamsActive(true);
                isActive = true;

                yield return new WaitForSeconds(beamDuration);

                isActive = false;
                SetBeamsActive(false);
                rotationDirection *= -1f;

                yield return new WaitForSeconds(timeBetweenBeamAttacks);
            }
        }

        private void Update()
        {
            if (!isActive || beamPivot == null) return;
            
            float currentRotationSpeed = useCustomRotationSpeed ? customRotationSpeed : rotationSpeed;

            beamPivot.Rotate(Vector3.up, currentRotationSpeed * rotationDirection * Time.deltaTime);
        }
        
        private void SetBeamsActive(bool active)
        {
            for (int i = 0; i < beams.Length; i++)
            {
                if (beams[i] != null)
                {
                    beams[i].SetActive(active);
                }
            }

            if (active)
                PlayBeamSfx();
        }

        private void SetWarningBeamsActive(bool active)
        {
            for (int i = 0; i < warningBeams.Length; i++)
            {
                if (warningBeams[i] != null)
                {
                    warningBeams[i].SetActive(active);
                }
            }
        }

        private void PlayBeamSfx()
        {
            if (beamAttackSound == null || beamSfxSource == null) return;

            beamSfxSource.volume = beamAttackSoundVolume;
            beamSfxSource.loop = false;
            beamSfxSource.spatialBlend = playBeamAttackSoundAs2D ? 0f : 1f;
            beamSfxSource.outputAudioMixerGroup = AudioHandler.DefaultSfxMixerGroup;
            beamSfxSource.PlayOneShot(beamAttackSound, beamAttackSoundVolume);
        }

        private AudioSource CreateBeamSfxSource()
        {
            GameObject sourceObject = new GameObject("Boss Beam Attack Audio");
            sourceObject.transform.SetParent(transform);
            sourceObject.transform.localPosition = Vector3.zero;

            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }

    }
}
