using System;
using UnityEngine;

namespace Branded.Player
{
    // Sword swing timing with an input buffer: a press shortly before the player is free again still fires.
    public class PlayerCombat : MonoBehaviour
    {
        public enum SwingPhase { None, Windup, Active, Recovery }

        [Header("References")]
        [SerializeField] PlayerInputReader input;
        [SerializeField] PlayerMotor motor;
        [SerializeField] PlayerAim aim;

        [Header("Swing timing (seconds)")]
        [SerializeField] float windup = 0.08f;
        [SerializeField] float active = 0.12f;
        [SerializeField] float recovery = 0.22f;
        [SerializeField] float inputBufferTime = 0.2f;

        [Header("Movement")]
        [SerializeField, Range(0f, 1f)] float moveSpeedWhileSwinging = 0.25f;

        public SwingPhase Phase { get; private set; }
        public float PhaseProgress => _phaseDuration > 0f ? Mathf.Clamp01(_phaseTimer / _phaseDuration) : 0f;

        public event Action SwingStarted;
        public event Action SwingActive;   // hit frame: Aşama 2 hitbox scan hooks in here
        public event Action SwingEnded;

        float _bufferTimer;
        float _phaseTimer;
        float _phaseDuration;

        void Awake()
        {
            if (!input) input = GetComponent<PlayerInputReader>();
            if (!motor) motor = GetComponent<PlayerMotor>();
            if (!aim) aim = GetComponent<PlayerAim>();
        }

        void OnEnable()
        {
            input.AttackPressed += BufferAttack;
            motor.DashStarted += CancelSwing;
        }

        void OnDisable()
        {
            input.AttackPressed -= BufferAttack;
            motor.DashStarted -= CancelSwing;
        }

        void Update()
        {
            _bufferTimer -= Time.deltaTime;

            if (Phase != SwingPhase.None) TickPhase(Time.deltaTime);

            if (Phase == SwingPhase.None && !motor.IsDashing && _bufferTimer > 0f)
                StartSwing();
        }

        void BufferAttack() => _bufferTimer = inputBufferTime;

        void StartSwing()
        {
            _bufferTimer = 0f;
            aim.SnapToAim();
            aim.RotationLocked = true;
            motor.SpeedMultiplier = moveSpeedWhileSwinging;
            EnterPhase(SwingPhase.Windup, windup);
            SwingStarted?.Invoke();
        }

        void TickPhase(float dt)
        {
            _phaseTimer += dt;
            if (_phaseTimer < _phaseDuration) return;

            switch (Phase)
            {
                case SwingPhase.Windup:
                    EnterPhase(SwingPhase.Active, active);
                    SwingActive?.Invoke();
                    break;
                case SwingPhase.Active:
                    EnterPhase(SwingPhase.Recovery, recovery);
                    break;
                case SwingPhase.Recovery:
                    EndSwing();
                    break;
            }
        }

        void EnterPhase(SwingPhase phase, float duration)
        {
            Phase = phase;
            _phaseTimer = 0f;
            _phaseDuration = duration;
        }

        // Dash cancels the swing; an attack pressed during the dash stays buffered.
        void CancelSwing()
        {
            if (Phase != SwingPhase.None) EndSwing();
        }

        void EndSwing()
        {
            Phase = SwingPhase.None;
            _phaseTimer = 0f;
            _phaseDuration = 0f;
            aim.RotationLocked = false;
            motor.SpeedMultiplier = 1f;
            SwingEnded?.Invoke();
        }
    }
}
