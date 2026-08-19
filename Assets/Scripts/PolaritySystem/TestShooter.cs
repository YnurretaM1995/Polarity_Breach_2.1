using PolarityBreach.Audio;
using PolarityBreach.Player;
using PolarityBreach.UI;
using UnityEngine;
using UnityEngine.Audio;
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
        
        [SerializeField] private AudioClip shootSound;
        [SerializeField] private AudioClip chargeSound;
        
        private bool _isCharging;
        private bool _chargeReady;
        private float _chargeStartTime;
        [SerializeField] private Transform _muzzle;
        private PolarityComponent _polarity;
        private InputAction _fireAction;
        private float _lastShotTime = float.NegativeInfinity;
        private Camera _cam;
        private PlayerStatsData _playerStats;

        private void Awake()
        {
            _polarity = GetComponent<PolarityComponent>();
            _playerStats = GetComponent<PlayerStatsData>();
            _cam = Camera.main;
            if (_muzzle == null) 
                _muzzle = transform;

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
            if (paused) _fireAction.Disable();
            else _fireAction.Enable();
        }
        
        private void StartCharging()
        {
            _isCharging = true;
            _chargeReady = false;
            _chargeStartTime = Time.time;
        }

        private void UpdateCharging()
        {
            float holdTime = Time.time - _chargeStartTime;

            if (holdTime >= _playerStats.chargeTime && !_chargeReady)
            {
                _chargeReady = true;
                Debug.Log("Charge Shot READY!");
            }
        }

        private void ReleaseCharge()
        {
            if (_isCharging && _chargeReady)
            {
                ChargeShot();
            }
            else
            {
                Debug.Log("Charge Shot CANCELLED!");
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
            AudioHandler.Play3DSound(shootSound, transform.position);
        }
        
        private void ChargeShot()
        {
            ShootFromPool(_chargedProjectilePool,
                _playerStats.chargeShotSpeed,
                _playerStats.chargeShotDamage,
                _playerStats.chargeShotKnockBackPower);
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
    }
}
