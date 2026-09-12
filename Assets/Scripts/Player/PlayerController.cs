using PolarityBreach.Audio;
using PolarityBreach.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PolarityBreach.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        private Rigidbody rb;
        private Vector3 moveInput;
        private Vector3 lookDirection;
        private PlayerInputActions controls;
        private bool isUsingGamepad = false;
        private float aimStrength = 1f;
        private float currentAimRange;
        private float aimRangeVelocity;

        private bool isDashing = false;
        private bool canDash = true;
        private Vector3 dashDirection;
        private Coroutine dashCoroutine;
        private PlayerStatsData _playerStats;
        
        [Header("Movement Feel")]
        [SerializeField] private float acceleration = 12f;   
        [SerializeField] private float deceleration = 16f;
        [SerializeField] private AbilityUIDisplay dashUI;

        [Header("Aim Range")]
        [SerializeField] private Transform aimReticle;
        [SerializeField] private float minAimRange = 1f;
        [SerializeField] private float maxAimRange = 9.59f;
        [SerializeField] private bool useReticleStartDistanceAsMax = true;
        [SerializeField] private float mousePixelsForMaxRange = 350f;
        [SerializeField] private float gamepadAimDeadZone = 0.1f;
        [SerializeField] private float reticleSmoothTime = 0.05f;

        [Header("SFX")]
        [SerializeField] private AudioSource dashSfxSource;
        [SerializeField] private AudioClip dashSound;

        public Vector3 AimDirection => lookDirection;
        public float AimStrength => aimStrength;
        public float CurrentAimRange => currentAimRange;

        private void Awake()
        {
            InitializeInputSystem();
            _playerStats = GetComponent<PlayerStatsData>();
            FindAimReticleIfMissing();
            InitializeAimRange();
        }

        private void InitializeAimRange()
        {
            if (aimReticle != null && useReticleStartDistanceAsMax)
            {
                maxAimRange = Mathf.Abs(aimReticle.localPosition.z);
            }

            maxAimRange = Mathf.Max(maxAimRange, minAimRange);
            currentAimRange = maxAimRange;
        }

        private void OnEnable()
        {
            ResetDashState();
            controls.Player.Enable();
            PauseMenu.OnPauseChanged += HandlePause;
            UIQueue.OnBlockingChanged += HandlePause;
        }

        private void OnDisable()
        {
            ResetDashState();
            controls.Player.Disable();
            PauseMenu.OnPauseChanged -= HandlePause;
            UIQueue.OnBlockingChanged -= HandlePause;
        }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            RotatePlayerTowardAim();
            if(isDashing) return;
            ReadMovementInput();
            CalculateAimDirection();
            RotatePlayerTowardAim();
            UpdateAimReticle();
        }

        private void FixedUpdate()
        {
            if (isDashing)
            {
                ApplyDashPhysics();
            }
            else
            {
                ApplyMovementPhysics();
            }
        }
    
        private void HandlePause(bool paused)
        {
            if (paused) controls.Player.Disable();
            else controls.Player.Enable();
        }

        private void InitializeInputSystem()
        {
            controls = new PlayerInputActions();
            controls.Player.Dash.performed += ctx => TryTriggerDash();
        }

        private void ReadMovementInput()
        {
            Vector2 inputVector = controls.Player.Movement.ReadValue<Vector2>();
            moveInput = new Vector3(inputVector.x, 0f, inputVector.y).normalized;
        }

        private void CalculateAimDirection()
        {
            Vector2 mouseScreenPos = controls.Player.MouseAim.ReadValue<Vector2>();
            Vector2 stickInput = controls.Player.GamepadAim.ReadValue<Vector2>();

            if (stickInput.magnitude > 0.1f)
            {
                isUsingGamepad = true;
            }
            else if (Mouse.current != null && Mouse.current.delta.ReadValue().magnitude > 0.1f)
            {
                isUsingGamepad = false;
            }

            if (isUsingGamepad)
            {
                float stickMagnitude = stickInput.magnitude;
                aimStrength = Mathf.InverseLerp(gamepadAimDeadZone, 1f, stickMagnitude);

                if (stickMagnitude > gamepadAimDeadZone)
                {
                    lookDirection = new Vector3(stickInput.x, 0f, stickInput.y).normalized;
                }
            }
            else
            {
                Vector3 playerScreenPos = Camera.main.WorldToScreenPoint(transform.position);

                float deltaX = mouseScreenPos.x - playerScreenPos.x;
                float deltaY = mouseScreenPos.y - playerScreenPos.y;
                float mouseDistance = new Vector2(deltaX, deltaY).magnitude;

                aimStrength = Mathf.Clamp01(mouseDistance / Mathf.Max(mousePixelsForMaxRange, 1f));

                if (mouseDistance > 0.01f)
                {
                    lookDirection = new Vector3(deltaX, 0f, deltaY).normalized;
                }
            }
        }


        private void RotatePlayerTowardAim()
        {
            if (lookDirection != Vector3.zero)
            {
                transform.forward = lookDirection;
            }
        }

        private void UpdateAimReticle()
        {
            if (aimReticle == null) return;

            float targetRange = Mathf.Lerp(minAimRange, maxAimRange, aimStrength);
            currentAimRange = Mathf.SmoothDamp(currentAimRange, targetRange, ref aimRangeVelocity, reticleSmoothTime);

            Vector3 localPosition = aimReticle.localPosition;
            localPosition.z = currentAimRange;
            aimReticle.localPosition = localPosition;
        }

        private void FindAimReticleIfMissing()
        {
            if (aimReticle != null) return;

            Transform[] children = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name == "SimpleAimReticle")
                {
                    aimReticle = children[i];
                    return;
                }
            }
        }

        private void ApplyMovementPhysics()
        {
            // rb.linearVelocity = new Vector3(moveInput.x * _playerStats.movementSpeed, rb.linearVelocity.y, moveInput.z * _playerStats.movementSpeed);
           
            Vector3 targetVelocity = new Vector3(moveInput.x * _playerStats.CurrentMovementSpeed, 0f, moveInput.z * _playerStats.CurrentMovementSpeed);

            Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            float rate = moveInput.sqrMagnitude > 0.01f ? acceleration : deceleration;
            Vector3 newVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, rate * Time.fixedDeltaTime);

            rb.linearVelocity = new Vector3(newVelocity.x, rb.linearVelocity.y, newVelocity.z);

        }

        private void ApplyDashPhysics()
        {
            rb.linearVelocity = new Vector3(dashDirection.x * _playerStats.dashSpeed, rb.linearVelocity.y, dashDirection.z * _playerStats.dashSpeed);
        }

        private void TryTriggerDash()
        {
            if (!_playerStats.dashUnlocked) return;
            
            if (!canDash || isDashing) return;
            
            if (moveInput != Vector3.zero)
            {
                dashDirection = moveInput;
            }
            else
            {
                dashDirection = transform.forward;
            }

            dashCoroutine = StartCoroutine(DashCoroutine());
            PlayDashSfx();
            dashUI?.StartCooldownUI();
        }

        private void PlayDashSfx()
        {
            if (dashSound == null) return;

            if (dashSfxSource != null)
            {
                dashSfxSource.PlayOneShot(dashSound);
                return;
            }

            AudioHandler.Play3DSound(dashSound, transform.position);
        }

        private IEnumerator DashCoroutine()
        {
            canDash = false;
            isDashing = true;
            yield return new WaitForSeconds(_playerStats.dashDuration);
        
            isDashing = false;
        
            yield return new WaitForSeconds(_playerStats.dashCooldown);
        
            canDash = true;
            dashCoroutine = null;
        }

        private void ResetDashState()
        {
            if (dashCoroutine != null)
            {
                StopCoroutine(dashCoroutine);
                dashCoroutine = null;
            }

            isDashing = false;
            canDash = true;
        }

    }
}
