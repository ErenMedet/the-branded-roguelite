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
        [Tooltip("Lower than acceleration, so stopping carries a little weight instead of cutting out.")]
        [SerializeField] float _deceleration = 45f;
        [Tooltip("Rate used when reversing or cutting a corner, so a direction change stays crisp.")]
        [SerializeField] float _turnAcceleration = 110f;
        [SerializeField, FormerlySerializedAs("gravity")] float _gravity = -25f;

        [Header("Dash")]
        [SerializeField, FormerlySerializedAs("dashDistance")] float _dashDistance = 5f;
        [SerializeField, FormerlySerializedAs("dashDuration")] float _dashDuration = 0.2f;
        [Tooltip("How sharply the dash bleeds off: 0 is a flat slide, higher bursts out and settles.")]
        [SerializeField, Range(0f, 3f)] float _dashDecay = 0.7f;
        [SerializeField, FormerlySerializedAs("dashCooldown")] float _dashCooldown = 0.35f;
        [SerializeField, FormerlySerializedAs("invulnerabilityDuration")] float _invulnerabilityDuration = 0.2f;

        [Header("Speed blending")]
        [Tooltip("How fast an attack clamps the walk speed down. High, because committing has to be felt at once.")]
        [SerializeField] float _speedDropSharpness = 35f;
        [Tooltip("How fast full speed returns once an attack ends. Lower, so the recovery eases out.")]
        [SerializeField] float _speedRecoverSharpness = 12f;

        public bool IsDashing => _dashTimer > 0f;
        public bool IsInvulnerable => _iFrameTimer > 0f;
        public Vector3 DashDirection { get; private set; } = Vector3.forward;
        public Vector3 PlanarVelocity { get; private set; }

        // Other systems (e.g. combat) scale movement through this, never by touching _moveSpeed.
        // It is a target: the motor eases onto it, so starting a swing never steps the walk speed.
        public float SpeedMultiplier { get; set; } = 1f;
        // Latched spirits slow walking through this; kept apart from SpeedMultiplier so combat cannot overwrite it.
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
        float _easedSpeedMultiplier = 1f;
        Vector3 _pushDirection;
        float _pushSpeed;
        float _pushDecay;
        float _pushTimer;
        float _pushDuration;

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
            TickSpeedMultiplier(dt);

            Vector3 motion = IsDashing ? DashMotion(dt) : _pushTimer > 0f ? PushMotion(dt) : WalkMotion(dt);

            if (_characterControllerComponent.isGrounded && _verticalVelocity < 0f) _verticalVelocity = -2f;
            _verticalVelocity += _gravity * dt;
            motion.y = _verticalVelocity;

            _characterControllerComponent.Move(motion * dt);
        }

        // Attacks and charges set SpeedMultiplier as a target; easing onto it keeps the walk from stepping.
        void TickSpeedMultiplier(float dt)
        {
            float sharpness = SpeedMultiplier < _easedSpeedMultiplier ? _speedDropSharpness : _speedRecoverSharpness;
            _easedSpeedMultiplier = Mathf.Lerp(_easedSpeedMultiplier, SpeedMultiplier, 1f - Mathf.Exp(-sharpness * dt));
        }

        // The dash leaves at full speed and bleeds off, so it lands instead of stopping dead.
        Vector3 DashMotion(float dt)
        {
            float elapsed = Mathf.Clamp01(1f - _dashTimer / _dashDuration);
            Vector3 motion = DashDirection * (_dashDistance / _dashDuration * Easing.DecaySpeed(elapsed, _dashDecay));

            _dashTimer -= dt;
            if (_dashTimer > 0f) return motion;

            // Walking picks up from where the dash left off rather than from a standstill.
            PlanarVelocity = DashDirection * (_moveSpeed * _easedSpeedMultiplier);
            DashEnded?.Invoke();
            return motion;
        }

        // Same decaying profile as the dash: a lunge bites at the start and settles into the swing.
        Vector3 PushMotion(float dt)
        {
            float elapsed = Mathf.Clamp01(1f - _pushTimer / _pushDuration);
            Vector3 motion = _pushDirection * (_pushSpeed * Easing.DecaySpeed(elapsed, _pushDecay));

            _pushTimer -= dt;
            PlanarVelocity = Vector3.zero;
            return motion;
        }

        // Forced motion from outside (a swing lunge, cannon recoil): overrides walking like a dash,
        // but grants no i-frames. decay shapes how the burst bleeds off; 0 keeps a flat slide.
        public void Push(Vector3 direction, float distance, float duration, float decay = 1.4f)
        {
            direction = FlatMath.Flat(direction);
            if (direction.sqrMagnitude < 0.0001f || duration <= 0f) return;

            _pushDirection = direction.normalized;
            _pushSpeed = distance / duration;
            _pushDecay = Mathf.Max(0f, decay);
            _pushDuration = duration;
            _pushTimer = duration;
        }

        // Starting, turning and stopping each get their own rate, so the walk reads as a body and not a cursor.
        Vector3 WalkMotion(float dt)
        {
            Vector3 target = CameraRelative(_inputComponent.Move) * (_moveSpeed * _easedSpeedMultiplier * SlowMultiplier);
            PlanarVelocity = Vector3.MoveTowards(PlanarVelocity, target, WalkRate(target) * dt);
            return PlanarVelocity;
        }

        float WalkRate(Vector3 target)
        {
            if (target.sqrMagnitude < 0.0001f) return _deceleration;
            if (PlanarVelocity.sqrMagnitude < 0.0001f) return _acceleration;
            return Vector3.Dot(PlanarVelocity.normalized, target.normalized) < 0.7f ? _turnAcceleration : _acceleration;
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
