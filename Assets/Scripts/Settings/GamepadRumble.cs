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

        [Header("Damage Taken")]
        [SerializeField] private float damageLowFrequency = 0.8f;
        [SerializeField] private float damageHighFrequency = 0.6f;
        [SerializeField] private float damagePulseDuration = 0.18f;

        private bool chargeActive;
        private Coroutine pulseRoutine;

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
            if (pulseRoutine != null) return;

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

            if (Gamepad.current == null) return;

            if (active)
            {
                if (pulseRoutine == null)
                    Gamepad.current.SetMotorSpeeds(chargeLowFrequency, chargeHighFrequency);
            }
            else if (pulseRoutine == null)
            {
                Gamepad.current.SetMotorSpeeds(0f, 0f);
            }
        }

        public void PulseDamage()
        {
            if (Gamepad.current == null) return;

            if (pulseRoutine != null) StopCoroutine(pulseRoutine);
            pulseRoutine = StartCoroutine(DamagePulseRoutine());
        }

        private IEnumerator DamagePulseRoutine()
        {
            Gamepad.current.SetMotorSpeeds(damageLowFrequency, damageHighFrequency);

            yield return new WaitForSecondsRealtime(damagePulseDuration);

            pulseRoutine = null;

            if (Gamepad.current == null) yield break;

            if (chargeActive)
                Gamepad.current.SetMotorSpeeds(chargeLowFrequency, chargeHighFrequency);
            else
                Gamepad.current.SetMotorSpeeds(0f, 0f);
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