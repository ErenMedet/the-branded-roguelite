using System;
using Branded.Combat;
using UnityEngine;

namespace Branded.Player
{
    // Moves the player with a CharacterController: camera-relative WASD, gravity and dash (with i-frames).
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour, IInvulnerabilitySource
    {
        [Header("References")]
        [SerializeField] PlayerInputReader input;
        [SerializeField] PlayerAim aim;
        [Tooltip("Movement is relative to this camera. Empty = Camera.main.")]
        [SerializeField] Transform cameraTransform;

        [Header("Movement")]
        [SerializeField] float moveSpeed = 7f;
        [SerializeField] float acceleration = 70f;
        [SerializeField] float gravity = -25f;

        [Header("Dash")]
        [SerializeField] float dashDistance = 5f;
        [SerializeField] float dashDuration = 0.2f;
        [SerializeField] float dashCooldown = 0.35f;
        [SerializeField] float invulnerabilityDuration = 0.2f;

        public bool IsDashing => _dashTimer > 0f;
        public bool IsInvulnerable => _iFrameTimer > 0f;
        public Vector3 DashDirection => _dashDirection;
        public Vector3 PlanarVelocity => _planarVelocity;

        // Other systems (e.g. combat) scale movement through this, never by touching moveSpeed.
        public float SpeedMultiplier { get; set; } = 1f;

        public event Action DashStarted;
        public event Action DashEnded;

        CharacterController _controller;
        Vector3 _planarVelocity;
        float _verticalVelocity;
        Vector3 _dashDirection = Vector3.forward;
        float _dashTimer;
        float _cooldownTimer;
        float _iFrameTimer;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (!input) input = GetComponent<PlayerInputReader>();
            if (!aim) aim = GetComponent<PlayerAim>();
        }

        void OnEnable() => input.DashPressed += TryDash;
        void OnDisable() => input.DashPressed -= TryDash;

        void Update()
        {
            float dt = Time.deltaTime;
            _cooldownTimer -= dt;
            _iFrameTimer -= dt;

            Vector3 motion;
            if (IsDashing)
            {
                _dashTimer -= dt;
                motion = _dashDirection * (dashDistance / dashDuration);
                if (_dashTimer <= 0f)
                {
                    _planarVelocity = _dashDirection * moveSpeed * SpeedMultiplier;
                    DashEnded?.Invoke();
                }
            }
            else
            {
                Vector3 target = CameraRelative(input.Move) * (moveSpeed * SpeedMultiplier);
                _planarVelocity = Vector3.MoveTowards(_planarVelocity, target, acceleration * dt);
                motion = _planarVelocity;
            }

            if (_controller.isGrounded && _verticalVelocity < 0f) _verticalVelocity = -2f;
            _verticalVelocity += gravity * dt;
            motion.y = _verticalVelocity;

            _controller.Move(motion * dt);
        }

        void TryDash()
        {
            if (IsDashing || _cooldownTimer > 0f) return;

            Vector3 direction = CameraRelative(input.Move);
            if (direction.sqrMagnitude < 0.01f) direction = aim ? aim.AimDirection : transform.forward;

            _dashDirection = direction.normalized;
            _dashTimer = dashDuration;
            _cooldownTimer = dashDuration + dashCooldown;
            _iFrameTimer = invulnerabilityDuration;
            DashStarted?.Invoke();
        }

        Vector3 CameraRelative(Vector2 move)
        {
            if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
            if (!cameraTransform) return new Vector3(move.x, 0f, move.y);

            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            return forward * move.y + right * move.x;
        }
    }
}
