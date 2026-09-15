using Branded.Combat;
using Branded.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Branded.Player
{
    // Moves the player with a CharacterController: camera-relative WASD, gravity and dash (with i-frames).
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour, IInvulnerabilitySource
    {
        [Header("References")]
        [SerializeField, FormerlySerializedAs("input")] PlayerInputReader _inputComponent;
        [SerializeField, FormerlySerializedAs("aim")] PlayerAim _aimComponent;
        [Tooltip("Movement is relative to this camera. Empty = Camera.main.")]
        [SerializeField, FormerlySerializedAs("cameraTransform")] Transform _cameraTransformComponent;

        [Header("Movement")]
        [SerializeField, FormerlySerializedAs("moveSpeed")] float _moveSpeed = 7f;
        [SerializeField, FormerlySerializedAs("acceleration")] float _acceleration = 70f;
        [SerializeField, FormerlySerializedAs("gravity")] float _gravity = -25f;

        [Header("Dash")]
        [SerializeField, FormerlySerializedAs("dashDistance")] float _dashDistance = 5f;
        [SerializeField, FormerlySerializedAs("dashDuration")] float _dashDuration = 0.2f;
        [SerializeField, FormerlySerializedAs("dashCooldown")] float _dashCooldown = 0.35f;
        [SerializeField, FormerlySerializedAs("invulnerabilityDuration")] float _invulnerabilityDuration = 0.2f;

        public bool IsDashing => _dashTimer > 0f;
        public bool IsInvulnerable => _iFrameTimer > 0f;
        public Vector3 DashDirection { get; private set; } = Vector3.forward;
        public Vector3 PlanarVelocity { get; private set; }

        // Other systems (e.g. combat) scale movement through this, never by touching _moveSpeed.
        public float SpeedMultiplier { get; set; } = 1f;
        // Latched spirits slow walking through this; kept apart from SpeedMultiplier so combat can't overwrite it.
        public float SlowMultiplier { get; set; } = 1f;
        // Boons shorten the dash cooldown through this.
        public float DashCooldownMultiplier { get; set; } = 1f;

        public event UnityAction DashStarted;
        public event UnityAction DashEnded;

        CharacterController _characterControllerComponent;
        float _verticalVelocity;
        float _dashTimer;
        float _cooldownTimer;
        float _iFrameTimer;
        Vector3 _pushVelocity;
        float _pushTimer;

        void Awake()
        {
            _characterControllerComponent = GetComponent<CharacterController>();
            if (!_inputComponent) _inputComponent = GetComponent<PlayerInputReader>();
            if (!_aimComponent) _aimComponent = GetComponent<PlayerAim>();
        }

        void OnEnable() => _inputComponent.DashPressed += OnDashPressed;
        void OnDisable() => _inputComponent.DashPressed -= OnDashPressed;

        void Update()
        {
            float dt = Time.deltaTime;
            _cooldownTimer -= dt;
            _iFrameTimer -= dt;

            Vector3 motion = IsDashing ? DashMotion(dt) : _pushTimer > 0f ? PushMotion(dt) : WalkMotion(dt);

            if (_characterControllerComponent.isGrounded && _verticalVelocity < 0f) _verticalVelocity = -2f;
            _verticalVelocity += _gravity * dt;
            motion.y = _verticalVelocity;

            _characterControllerComponent.Move(motion * dt);
        }

        Vector3 DashMotion(float dt)
        {
            _dashTimer -= dt;
            Vector3 motion = DashDirection * (_dashDistance / _dashDuration);
            if (_dashTimer > 0f) return motion;

            PlanarVelocity = DashDirection * _moveSpeed * SpeedMultiplier;
            DashEnded?.Invoke();
            return motion;
        }

        Vector3 PushMotion(float dt)
        {
            _pushTimer -= dt;
            PlanarVelocity = Vector3.zero;
            return _pushVelocity;
        }

        // Forced motion from outside (cannon recoil): overrides walking like a dash, but grants no i-frames.
        public void Push(Vector3 direction, float distance, float duration)
        {
            direction = FlatMath.Flat(direction);
            if (direction.sqrMagnitude < 0.0001f || duration <= 0f) return;
            _pushVelocity = direction.normalized * (distance / duration);
            _pushTimer = duration;
        }

        Vector3 WalkMotion(float dt)
        {
            Vector3 target = CameraRelative(_inputComponent.Move) * (_moveSpeed * SpeedMultiplier * SlowMultiplier);
            PlanarVelocity = Vector3.MoveTowards(PlanarVelocity, target, _acceleration * dt);
            return PlanarVelocity;
        }

        void OnDashPressed()
        {
            if (IsDashing || _cooldownTimer > 0f) return;

            Vector3 direction = CameraRelative(_inputComponent.Move);
            if (direction.sqrMagnitude < 0.01f) direction = _aimComponent ? _aimComponent.AimDirection : transform.forward;

            DashDirection = direction.normalized;
            _dashTimer = _dashDuration;
            _pushTimer = 0f;
            _cooldownTimer = _dashDuration + _dashCooldown * DashCooldownMultiplier;
            _iFrameTimer = _invulnerabilityDuration;
            DashStarted?.Invoke();
        }

        Vector3 CameraRelative(Vector2 move)
        {
            if (!_cameraTransformComponent && Camera.main) _cameraTransformComponent = Camera.main.transform;
            if (!_cameraTransformComponent) return new Vector3(move.x, 0f, move.y);

            Vector3 forward = Vector3.ProjectOnPlane(_cameraTransformComponent.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(_cameraTransformComponent.right, Vector3.up).normalized;
            return forward * move.y + right * move.x;
        }
    }
}
