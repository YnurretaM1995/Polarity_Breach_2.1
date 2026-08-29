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

        private bool isDashing = false;
        private bool canDash = true;
        private Vector3 dashDirection;
        private PlayerStatsData _playerStats;
        
        [Header("Movement Feel")]
        [SerializeField] private float acceleration = 12f;   
        [SerializeField] private float deceleration = 16f;
        [SerializeField] private AbilityUIDisplay dashUI;

        [Header("SFX")]
        [SerializeField] private AudioSource dashSfxSource;
        [SerializeField] private AudioClip dashSound;

        private void Awake()
        {
            InitializeInputSystem();
            _playerStats = GetComponent<PlayerStatsData>();
        }

        private void OnEnable()
        {
            controls.Player.Enable();
            PauseMenu.OnPauseChanged += HandlePause;
            UIQueue.OnBlockingChanged += HandlePause;
        }

        private void OnDisable()
        {
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
                if (stickInput.magnitude > 0.1f)
                {
                    lookDirection = new Vector3(stickInput.x, 0f, stickInput.y).normalized;
                }
            }
            else
            {
                Vector3 playerScreenPos = Camera.main.WorldToScreenPoint(transform.position);

                float deltaX = mouseScreenPos.x - playerScreenPos.x;
                float deltaY = mouseScreenPos.y - playerScreenPos.y;

                lookDirection = new Vector3(deltaX, 0f, deltaY).normalized;
            }
        }


        private void RotatePlayerTowardAim()
        {
            if (lookDirection != Vector3.zero)
            {
                transform.forward = lookDirection;
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

            StartCoroutine(DashCoroutine());
            PlayDashSfx();
            dashUI.StartCooldownUI();
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
        }

    }
}
