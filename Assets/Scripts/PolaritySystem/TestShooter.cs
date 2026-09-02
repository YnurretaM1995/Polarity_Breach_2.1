using PolarityBreach.Audio;
using PolarityBreach.Player;
using PolarityBreach.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PolarityBreach.PolaritySystem
{
    [RequireComponent(typeof(PolarityComponent))]
    [RequireComponent(typeof(PlayerStatsData))]
    public class TestShooter : MonoBehaviour
    {
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private GameObject _chargedProjectilePrefab;
        [SerializeField] private ProjectilePool _normalProjectilePool;
        [SerializeField] private ProjectilePool _chargedProjectilePool;
        
        [SerializeField] private AudioClip[] shootSounds;
        [SerializeField] private AudioClip shootSound;
        [SerializeField, Range(0f, 1f)] private float shootSoundVolume = 1f;
        [SerializeField] private bool playShootSoundAs2D;

        [Header("Charge Shot SFX")]
        [SerializeField] private AudioSource chargeLoopSource;
        [SerializeField] private AudioClip chargeSound;
        [SerializeField, Range(0f, 1f)] private float chargeSoundVolume = 1f;
        [SerializeField] private AudioSource heldChargeLoopSource;
        [SerializeField] private AudioClip heldChargeSound;
        [SerializeField, Range(0f, 1f)] private float heldChargeSoundVolume = 1f;
        [SerializeField] private AudioClip chargeCancelSound;
        [SerializeField, Range(0f, 1f)] private float chargeCancelSoundVolume = 1f;
        [SerializeField] private AudioClip chargedShotSound;
        [SerializeField, Range(0f, 1f)] private float chargedShotSoundVolume = 1f;
        [SerializeField] private bool playChargeShotSoundsAs2D = true;
        
        private bool _isCharging;
        private bool _chargeReady;
        private float _chargeStartTime;
        [SerializeField] private Transform _muzzle;
        private PolarityComponent _polarity;
        private InputAction _fireAction;
        private float _lastShotTime = float.NegativeInfinity;
        private Camera _cam;
        private PlayerStatsData _playerStats;

        public bool IsCharging => _isCharging;
        public bool ChargeReady => _chargeReady;
        public float ChargeProgress
        {
            get
            {
                if (!_isCharging) return 0f;
                if (_playerStats.chargeTime <= 0f) return 1f;
                return Mathf.Clamp01((Time.time - _chargeStartTime) / _playerStats.chargeTime);
            }
        }

        private void Awake()
        {
            _polarity = GetComponent<PolarityComponent>();
            _playerStats = GetComponent<PlayerStatsData>();
            _cam = Camera.main;
            if (_muzzle == null) 
                _muzzle = transform;
            if (chargeLoopSource == null)
                chargeLoopSource = CreateLoopSource("Charge Shot Loop Audio");
            if (heldChargeLoopSource == null)
                heldChargeLoopSource = CreateLoopSource("Held Charge Shot Loop Audio");

            _fireAction = new InputAction("Fire", InputActionType.Button);
            _fireAction.AddBinding("<Mouse>/leftButton");
            _fireAction.AddBinding("<Gamepad>/rightTrigger");
        }

        private void OnEnable()
        { 
            _fireAction.Enable();
            PauseMenu.OnPauseChanged += HandlePause;
            UIQueue.OnBlockingChanged += HandlePause;
        }
        private void OnDisable()
        {
            _fireAction.Disable();
            PauseMenu.OnPauseChanged -= HandlePause;
            UIQueue.OnBlockingChanged -= HandlePause;
            StopChargeLoop();
            StopHeldChargeLoop();
        }
        private void OnDestroy() => _fireAction.Dispose();

        private void Update()
        {
            if (UIQueue.IsBlocking || PauseMenu.IsPaused) return;
            if (_playerStats.chargeShotUnlocked)
            {
                if (_fireAction.WasPressedThisFrame())
                {
                    StartCharging();
                    return;
                }

                if (_fireAction.WasReleasedThisFrame())
                {
                    ReleaseCharge();
                    return;
                }

                if (_fireAction.IsPressed())
                {
                    UpdateCharging();
                    return;
                }
            }

            AutoFire();
        }
        
        private void HandlePause(bool paused)
        {
            if (paused)
            {
                _fireAction.Disable();
                CancelCharge(false);
            }
            else _fireAction.Enable();
        }
        
        private void StartCharging()
        {
            _isCharging = true;
            _chargeReady = false;
            _chargeStartTime = Time.time;
            StopHeldChargeLoop();
            StartChargeLoop();
        }

        private void UpdateCharging()
        {
            float holdTime = Time.time - _chargeStartTime;

            if (holdTime >= _playerStats.chargeTime && !_chargeReady)
            {
                _chargeReady = true;
                StartHeldChargeLoop();
                StopChargeLoop();
                Debug.Log("Charge Shot READY!");
            }
        }

        private void ReleaseCharge()
        {
            if (_isCharging && _chargeReady)
            {
                StopChargeLoop();
                StopHeldChargeLoop();
                ChargeShot();
            }
            else
            {
                CancelCharge(true);
            }
            
            _isCharging = false;
            _chargeReady = false;
        }
        
        private void Shoot()
        {
            ShootFromPool(_normalProjectilePool, 
                _playerStats.attackSpeed,
                _playerStats.attackDamage,
                _playerStats.knockBackPower);
            PlayShootSfx();
        }
        
        private void ChargeShot()
        {
            ShootFromPool(_chargedProjectilePool,
                _playerStats.chargeShotSpeed,
                _playerStats.chargeShotDamage,
                _playerStats.chargeShotKnockBackPower);
            PlayChargeShotSfx();
        }

        private void ShootFromPool(ProjectilePool pool, float speed, float damage, float knockbackForce)
        {
            if (pool == null) return;

            Vector3 dir = transform.forward;
            dir.y = 0f;
            dir.Normalize();

            ShootProjectile projectile = pool.GetProjectile(_muzzle.position, Quaternion.LookRotation(dir));

            if (projectile == null) return;
            projectile.SetStats(speed, damage, knockbackForce);
            
            var bulletPolarity = projectile.GetComponent<PolarityComponent>();
            if (bulletPolarity != null) bulletPolarity.SetPolarity(_polarity.CurrentPolarity);

            _lastShotTime = Time.time;
        }

        
        
        private void AutoFire()
        {
            if (Time.time >= _lastShotTime + _playerStats.attackSpeedDelay)
            {
                Shoot();
            }
        }

        private AudioClip GetRandomShootSound()
        {
            if (shootSounds != null && shootSounds.Length > 0)
            {
                return shootSounds[Random.Range(0, shootSounds.Length)];
            }

            return shootSound;
        }

        private void PlayShootSfx()
        {
            AudioClip clip = GetRandomShootSound();

            if (playShootSoundAs2D)
            {
                AudioHandler.Play2DSound(clip, shootSoundVolume);
                return;
            }

            AudioHandler.Play3DSound(clip, transform.position, shootSoundVolume);
        }

        private void StartChargeLoop()
        {
            if (chargeSound == null) return;

            if (chargeLoopSource == null)
                chargeLoopSource = CreateLoopSource("Charge Shot Loop Audio");

            chargeLoopSource.clip = chargeSound;
            chargeLoopSource.volume = chargeSoundVolume;
            chargeLoopSource.loop = true;
            chargeLoopSource.spatialBlend = playChargeShotSoundsAs2D ? 0f : 1f;
            chargeLoopSource.outputAudioMixerGroup = AudioHandler.DefaultSfxMixerGroup;
            chargeLoopSource.Play();
        }

        private void StopChargeLoop()
        {
            if (chargeLoopSource == null) return;

            if (chargeLoopSource.isPlaying && chargeLoopSource.clip == chargeSound)
                chargeLoopSource.Stop();

            chargeLoopSource.loop = false;
        }

        private void StartHeldChargeLoop()
        {
            if (heldChargeSound == null) return;

            if (heldChargeLoopSource == null)
                heldChargeLoopSource = CreateLoopSource("Held Charge Shot Loop Audio");

            heldChargeLoopSource.clip = heldChargeSound;
            heldChargeLoopSource.volume = heldChargeSoundVolume;
            heldChargeLoopSource.loop = true;
            heldChargeLoopSource.spatialBlend = playChargeShotSoundsAs2D ? 0f : 1f;
            heldChargeLoopSource.outputAudioMixerGroup = AudioHandler.DefaultSfxMixerGroup;
            heldChargeLoopSource.Play();
        }

        private void StopHeldChargeLoop()
        {
            if (heldChargeLoopSource == null) return;

            if (heldChargeLoopSource.isPlaying && heldChargeLoopSource.clip == heldChargeSound)
                heldChargeLoopSource.Stop();

            heldChargeLoopSource.loop = false;
        }

        private void CancelCharge(bool playCancelSound)
        {
            if (!_isCharging) return;

            StopChargeLoop();
            StopHeldChargeLoop();
            _lastShotTime = Time.time;

            if (playCancelSound)
            {
                Debug.Log("Charge Shot CANCELLED!");
                PlayChargeCancelSfx();
            }

            _isCharging = false;
            _chargeReady = false;
        }

        private void PlayChargeCancelSfx()
        {
            if (playChargeShotSoundsAs2D)
            {
                AudioHandler.Play2DSound(chargeCancelSound, chargeCancelSoundVolume);
                return;
            }

            AudioHandler.Play3DSound(chargeCancelSound, transform.position, chargeCancelSoundVolume);
        }

        private void PlayChargeShotSfx()
        {
            if (playChargeShotSoundsAs2D)
            {
                AudioHandler.Play2DSound(chargedShotSound, chargedShotSoundVolume);
                return;
            }

            AudioHandler.Play3DSound(chargedShotSound, transform.position, chargedShotSoundVolume);
        }

        private AudioSource CreateLoopSource(string sourceName)
        {
            GameObject sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform);
            sourceObject.transform.localPosition = Vector3.zero;

            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }
    }
}
