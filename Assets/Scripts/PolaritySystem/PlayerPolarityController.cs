using PolarityBreach.Audio;
using PolarityBreach.Player;
using PolarityBreach.UI;
using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

namespace PolarityBreach.PolaritySystem
{
    [RequireComponent(typeof(PolarityComponent))]
    public class PlayerPolarityController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference _switchActionRef;

        private PolarityComponent _polarity;
        private InputAction _switchAction;
        private bool _ownsAction;
        private float _lastSwitchTime = float.NegativeInfinity;
        private PlayerStatsData _playerStats;

        [Header("SFX")]
        [SerializeField] private AudioClip[] colorSounds;
        [SerializeField] private AudioClip colorSound;
        [SerializeField, Range(0f, 1f)] private float colorSoundVolume = 1f;
        [SerializeField] private bool playColorSoundAs2D = true;
        [SerializeField] private AudioMixerGroup sfxMixerGroup;

        [SerializeField] private AbilityUIDisplay polarityUI;
        [SerializeField] private PolarityPostProcessPulse postProcessPulse;
        public event Action OnPolaritySwitched;
        public static event Action<Transform> OnAnyPlayerPolaritySwitched;
        
        public float SwitchCooldown
        {
            get => _playerStats.polaritySwitchCooldown;
            set => _playerStats.polaritySwitchCooldown = Mathf.Max(0f, value);
        }
        
        public float CooldownRemaining => Mathf.Max(0f, (_lastSwitchTime + _playerStats.polaritySwitchCooldown) - Time.time);
        public bool CanSwitch => Time.time >= _lastSwitchTime + _playerStats.polaritySwitchCooldown;

        private void Awake()
        {
            GetComponents();
            RefToSwitchAction();
        }

        private void RefToSwitchAction()
        {
            if (_switchActionRef != null)
            {
                _switchAction = _switchActionRef.action;
            }
            else
            {
                _switchAction = new InputAction("SwitchPolarity", InputActionType.Button);
                _switchAction.AddBinding("<Mouse>/rightButton");
                _switchAction.AddBinding("<Gamepad>/leftTrigger");
                _ownsAction = true;
            }
        }

        private void GetComponents()
        {
            _polarity = GetComponent<PolarityComponent>();
            _playerStats = GetComponent<PlayerStatsData>();

            if (postProcessPulse == null)
            {
                postProcessPulse = GetComponentInChildren<PolarityPostProcessPulse>();
            }

            if (postProcessPulse == null)
            {
                postProcessPulse = FindFirstObjectByType<PolarityPostProcessPulse>();
            }
        }

        private void OnEnable()
        {
            _switchAction.performed += OnSwitchPerformed;
            PauseMenu.OnPauseChanged += HandlePause;
            _switchAction.Enable();
            UIQueue.OnBlockingChanged += HandlePause;
        }

        private void OnDisable()
        {
            _switchAction.performed -= OnSwitchPerformed;
            PauseMenu.OnPauseChanged -= HandlePause;
            if (_ownsAction) _switchAction.Disable();
            UIQueue.OnBlockingChanged -= HandlePause;
        }

        private void OnDestroy()
        {
            if (_ownsAction) _switchAction?.Dispose();
        }

        private void OnSwitchPerformed(InputAction.CallbackContext context) => TrySwitch();

        public bool TrySwitch()
        {
            if (!CanSwitch) return false;
            _polarity.Toggle();
            postProcessPulse?.Play(_polarity.CurrentPolarity);
            _lastSwitchTime = Time.time;
            PlaySwitchSfx();
            polarityUI.StartCooldownUI();
            OnPolaritySwitched?.Invoke();
            OnAnyPlayerPolaritySwitched?.Invoke(transform);
            return true;
        }

        private void PlaySwitchSfx()
        {
            AudioClip clip = GetRandomSwitchSound();

            if (playColorSoundAs2D)
            {
                AudioHandler.Play2DSound(clip, colorSoundVolume, sfxMixerGroup);
                return;
            }

            AudioHandler.Play3DSound(clip, transform.position, colorSoundVolume, sfxMixerGroup);
        }

        private AudioClip GetRandomSwitchSound()
        {
            if (colorSounds != null && colorSounds.Length > 0)
            {
                return colorSounds[UnityEngine.Random.Range(0, colorSounds.Length)];
            }

            return colorSound;
        }
        
        private void HandlePause(bool paused)
        {
            if (paused) _switchAction.Disable();   
            else _switchAction.Enable();
        }
    }
} 
