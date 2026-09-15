using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Branded.Player
{
    // Sword swing timing with an input buffer: a press shortly before the player is free again still fires.
    // The 360° spin (Q) runs through the same phases; pressed during a dash it spins along the dash path.
    public class PlayerCombat : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, FormerlySerializedAs("input")] PlayerInputReader _inputComponent;
        [SerializeField, FormerlySerializedAs("motor")] PlayerMotor _motorComponent;
        [SerializeField, FormerlySerializedAs("aim")] PlayerAim _aimComponent;

        [Header("Swing timing (seconds)")]
        [SerializeField, FormerlySerializedAs("windup")] float _windup = 0.08f;
        [SerializeField, FormerlySerializedAs("active")] float _active = 0.12f;
        [SerializeField, FormerlySerializedAs("recovery")] float _recovery = 0.22f;
        [SerializeField, FormerlySerializedAs("inputBufferTime")] float _inputBufferTime = 0.2f;

        [Header("Spin timing (seconds)")]
        [SerializeField] float _spinWindup = 0.05f;
        [SerializeField] float _spinActive = 0.28f;
        [SerializeField] float _spinRecovery = 0.18f;
        [SerializeField] float _spinCooldown = 2f;

        [Header("Movement")]
        [SerializeField, Range(0f, 1f), FormerlySerializedAs("moveSpeedWhileSwinging")] float _moveSpeedWhileSwinging = 0.25f;
        [SerializeField, Range(0f, 1f)] float _moveSpeedWhileSpinning = 0.5f;

        public ESwingPhase Phase { get; private set; }
        public float PhaseProgress => _phaseDuration > 0f ? Mathf.Clamp01(_phaseTimer / _phaseDuration) : 0f;
        // True while the current swing is the 360° spin.
        public bool IsSpin { get; private set; }
        // 0 right after a spin starts, 1 once it can be used again.
        public float SpinCooldownProgress => _spinCooldown > 0f ? 1f - Mathf.Clamp01(_spinCooldownTimer / _spinCooldown) : 1f;

        public event UnityAction SwingStarted;
        public event UnityAction SwingActive; // hit frame: SwordHitbox scans here
        public event UnityAction SwingEnded;

        float _bufferTimer;
        float _spinBufferTimer;
        float _spinCooldownTimer;
        float _phaseTimer;
        float _phaseDuration;

        void Awake()
        {
            if (!_inputComponent) _inputComponent = GetComponent<PlayerInputReader>();
            if (!_motorComponent) _motorComponent = GetComponent<PlayerMotor>();
            if (!_aimComponent) _aimComponent = GetComponent<PlayerAim>();
        }

        void OnEnable()
        {
            _inputComponent.AttackPressed += OnAttackPressed;
            _inputComponent.SpinPressed += OnSpinPressed;
            _motorComponent.DashStarted += OnDashStarted;
        }

        void OnDisable()
        {
            _inputComponent.AttackPressed -= OnAttackPressed;
            _inputComponent.SpinPressed -= OnSpinPressed;
            _motorComponent.DashStarted -= OnDashStarted;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            _bufferTimer -= dt;
            _spinBufferTimer -= dt;
            _spinCooldownTimer -= dt;

            if (Phase != ESwingPhase.None) TickPhase(dt);
            if (Phase != ESwingPhase.None) return;

            // The spin is the one attack allowed mid-dash.
            if (_spinBufferTimer > 0f && _spinCooldownTimer <= 0f)
            {
                StartSpin();
                return;
            }

            if (_motorComponent.IsDashing || _bufferTimer <= 0f) return;
            StartSwing();
        }

        void OnAttackPressed() => _bufferTimer = _inputBufferTime;
        void OnSpinPressed() => _spinBufferTimer = _inputBufferTime;

        void StartSwing()
        {
            _bufferTimer = 0f;
            IsSpin = false;
            _aimComponent.SnapToAim();
            _aimComponent.RotationLocked = true;
            _motorComponent.SpeedMultiplier = _moveSpeedWhileSwinging;
            EnterPhase(ESwingPhase.Windup, _windup);
            SwingStarted?.Invoke();
        }

        void StartSpin()
        {
            _spinBufferTimer = 0f;
            _bufferTimer = 0f;
            _spinCooldownTimer = _spinCooldown;
            IsSpin = true;
            _aimComponent.RotationLocked = true;
            _motorComponent.SpeedMultiplier = _moveSpeedWhileSpinning;

            // A dash spin skips the windup, otherwise most of the short dash would pass before it hits.
            bool fromDash = _motorComponent.IsDashing;
            EnterPhase(fromDash ? ESwingPhase.Active : ESwingPhase.Windup, fromDash ? _spinActive : _spinWindup);
            SwingStarted?.Invoke();
            if (fromDash) SwingActive?.Invoke();
        }

        void TickPhase(float dt)
        {
            _phaseTimer += dt;
            if (_phaseTimer < _phaseDuration) return;

            switch (Phase)
            {
                case ESwingPhase.Windup:
                    EnterPhase(ESwingPhase.Active, IsSpin ? _spinActive : _active);
                    SwingActive?.Invoke();
                    break;
                case ESwingPhase.Active:
                    EnterPhase(ESwingPhase.Recovery, IsSpin ? _spinRecovery : _recovery);
                    break;
                case ESwingPhase.Recovery:
                    EndSwing();
                    break;
            }
        }

        void EnterPhase(ESwingPhase phase, float duration)
        {
            Phase = phase;
            _phaseTimer = 0f;
            _phaseDuration = duration;
        }

        // Dash cancels a swing but not a spin, which carries on along the dash.
        // An attack pressed during the dash stays buffered.
        void OnDashStarted()
        {
            if (Phase == ESwingPhase.None || IsSpin) return;
            EndSwing();
        }

        void EndSwing()
        {
            Phase = ESwingPhase.None;
            IsSpin = false;
            _phaseTimer = 0f;
            _phaseDuration = 0f;
            _aimComponent.RotationLocked = false;
            _motorComponent.SpeedMultiplier = 1f;
            SwingEnded?.Invoke();
        }
    }
}
