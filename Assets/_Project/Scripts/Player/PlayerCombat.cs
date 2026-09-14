using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Branded.Player
{
    // Sword swing timing with an input buffer: a press shortly before the player is free again still fires.
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

        [Header("Movement")]
        [SerializeField, Range(0f, 1f), FormerlySerializedAs("moveSpeedWhileSwinging")] float _moveSpeedWhileSwinging = 0.25f;

        public ESwingPhase Phase { get; private set; }
        public float PhaseProgress => _phaseDuration > 0f ? Mathf.Clamp01(_phaseTimer / _phaseDuration) : 0f;

        public event UnityAction SwingStarted;
        public event UnityAction SwingActive; // hit frame: SwordHitbox scans here
        public event UnityAction SwingEnded;

        float _bufferTimer;
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
            _motorComponent.DashStarted += OnDashStarted;
        }

        void OnDisable()
        {
            _inputComponent.AttackPressed -= OnAttackPressed;
            _motorComponent.DashStarted -= OnDashStarted;
        }

        void Update()
        {
            _bufferTimer -= Time.deltaTime;

            if (Phase != ESwingPhase.None) TickPhase(Time.deltaTime);

            if (Phase != ESwingPhase.None || _motorComponent.IsDashing || _bufferTimer <= 0f) return;
            StartSwing();
        }

        void OnAttackPressed() => _bufferTimer = _inputBufferTime;

        void StartSwing()
        {
            _bufferTimer = 0f;
            _aimComponent.SnapToAim();
            _aimComponent.RotationLocked = true;
            _motorComponent.SpeedMultiplier = _moveSpeedWhileSwinging;
            EnterPhase(ESwingPhase.Windup, _windup);
            SwingStarted?.Invoke();
        }

        void TickPhase(float dt)
        {
            _phaseTimer += dt;
            if (_phaseTimer < _phaseDuration) return;

            switch (Phase)
            {
                case ESwingPhase.Windup:
                    EnterPhase(ESwingPhase.Active, _active);
                    SwingActive?.Invoke();
                    break;
                case ESwingPhase.Active:
                    EnterPhase(ESwingPhase.Recovery, _recovery);
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

        // Dash cancels the swing; an attack pressed during the dash stays buffered.
        void OnDashStarted()
        {
            if (Phase == ESwingPhase.None) return;
            EndSwing();
        }

        void EndSwing()
        {
            Phase = ESwingPhase.None;
            _phaseTimer = 0f;
            _phaseDuration = 0f;
            _aimComponent.RotationLocked = false;
            _motorComponent.SpeedMultiplier = 1f;
            SwingEnded?.Invoke();
        }
    }
}
