using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PolarityBreach.Player
{
    public class GamepadRumble : MonoBehaviour
    {
        public static GamepadRumble Instance { get; private set; }

        [Header("Charge Shot")]
        [SerializeField] private float chargeLowFrequency = 0.3f;
        [SerializeField] private float chargeHighFrequency = 0.5f;

        [Header("Low Health")]
        [SerializeField] private float lowHealthThreshold = 0.3f;
        [SerializeField] private float lowHealthPulseStrength = 0.5f;
        [SerializeField] private float lowHealthPulseDuration = 0.06f;

        private bool chargeActive;
        private Coroutine pulseRoutine;

        public float LowHealthThreshold => lowHealthThreshold;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            if (!chargeActive) return;
            if (Gamepad.current == null) return;

            Gamepad.current.SetMotorSpeeds(chargeLowFrequency, chargeHighFrequency);
        }

        private void OnDisable()
        {
            StopAll();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnApplicationQuit()
        {
            StopAll();
        }

        public void SetChargeRumble(bool active)
        {
            chargeActive = active;
            Debug.Log($"[Rumble] SetChargeRumble: {active}");

            if (active && pulseRoutine != null)
            {
                StopCoroutine(pulseRoutine);
                pulseRoutine = null;
            }

            if (Gamepad.current == null) return;

            if (active)
            {
                Gamepad.current.SetMotorSpeeds(chargeLowFrequency, chargeHighFrequency);
            }
            else
            {
                Gamepad.current.SetMotorSpeeds(0f, 0f);
            }
        }

        public void PulseLowHealth()
        {
            if (chargeActive) return;
            if (Gamepad.current == null) return;

            if (pulseRoutine != null) StopCoroutine(pulseRoutine);
            pulseRoutine = StartCoroutine(PulseRoutine());
        }

        private IEnumerator PulseRoutine()
        {
            Gamepad.current.SetMotorSpeeds(lowHealthPulseStrength, lowHealthPulseStrength);

            yield return new WaitForSecondsRealtime(lowHealthPulseDuration);

            if (!chargeActive && Gamepad.current != null)
                Gamepad.current.SetMotorSpeeds(0f, 0f);

            pulseRoutine = null;
        }

        public void StopAll()
        {
            chargeActive = false;

            if (pulseRoutine != null)
            {
                StopCoroutine(pulseRoutine);
                pulseRoutine = null;
            }

            if (Gamepad.current != null)
                Gamepad.current.SetMotorSpeeds(0f, 0f);
        }
    }
}