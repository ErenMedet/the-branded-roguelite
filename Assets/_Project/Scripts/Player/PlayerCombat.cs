using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Branded.Player
{
    // Sword timing with an input buffer: a press shortly before the player is free again still fires.
    // Presses chain through _comboSwings; a press held past _chargeMinTime charges a heavy strike instead,
    // a press right after a dash becomes the dash strike, and the finisher links into the 360° spin (Q).
    public class PlayerCombat : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, FormerlySerializedAs("input")] PlayerInputReader _inputComponent;
        [SerializeField, FormerlySerializedAs("motor")] PlayerMotor _motorComponent;
        [SerializeField, FormerlySerializedAs("aim")] PlayerAim _aimComponent;

        [Header("Combo chain")]
        [Tooltip("Played in order, the last one being the finisher.")]
        [SerializeField] SwingData[] _comboSwings;
        [SerializeField, FormerlySerializedAs("inputBufferTime")] float _inputBufferTime = 0.28f;
        [Tooltip("How long after a swing ends the chain still continues.")]
        [SerializeField] float _comboResetTime = 0.4f;

        [Header("Dash strike")]
        [SerializeField] SwingData _dashStrike;
        [SerializeField] float _dashStrikeWindow = 0.25f;

        [Header("Charged strike")]
        [SerializeField] SwingData _chargedStrike;
        [Tooltip("Held shorter than this, an attack press is just the next chain swing.")]
        [SerializeField] float _chargeMinTime = 0.25f;
        [SerializeField] float _chargeFullTime = 0.9f;
        [Tooltip("Share of the charged strike's damage at the shortest charge.")]
        [SerializeField, Range(0f, 1f)] float _chargeMinDamageFraction = 0.5f;
        [SerializeField, Range(0f, 1f)] float _moveSpeedWhileCharging = 0.3f;

        [Header("Spin timing (seconds)")]
        [SerializeField] float _spinWindup = 0.05f;
        [SerializeField] float _spinActive = 0.28f;
        [SerializeField] float _spinRecovery = 0.18f;
        [SerializeField] float _spinCooldown = 2f;
        [Tooltip("How long after the finisher the spin still links off it.")]
        [SerializeField] float _spinLinkWindow = 0.3f;
        [SerializeField] float _linkedSpinDamageMultiplier = 1.5f;

        [Header("Movement")]
        [SerializeField, Range(0f, 1f)] float _moveSpeedWhileSpinning = 0.5f;

        public ESwingPhase Phase { get; private set; }
        public float PhaseProgress => _phaseDuration > 0f ? Mathf.Clamp01(_phaseTimer / _phaseDuration) : 0f;
        // True while the current swing is the 360° spin.
        public bool IsSpin { get; private set; }
        // The attack being played; null for the spin, which has its own timing fields.
        public SwingData CurrentSwing { get; private set; }
        // Scales the sword's damage for this attack: chain step, charge level or linked spin.
        public float AttackDamageMultiplier { get; private set; } = 1f;
        // 0 at the start of a charge, 1 once it is full.
        public float ChargeProgress => Phase == ESwingPhase.Charging && _chargeFullTime > 0f ? Mathf.Clamp01(_phaseTimer / _chargeFullTime) : 0f;
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
        float _comboResetTimer;
        float _dashStrikeTimer;
        float _spinLinkTimer;
        int _comboIndex;
        bool _inChain;
        bool _isFinisher;

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
            _motorComponent.DashEnded += OnDashEnded;
        }

        void OnDisable()
        {
            _inputComponent.AttackPressed -= OnAttackPressed;
            _inputComponent.SpinPressed -= OnSpinPressed;
            _motorComponent.DashStarted -= OnDashStarted;
            _motorComponent.DashEnded -= OnDashEnded;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            _bufferTimer -= dt;
            _spinBufferTimer -= dt;
            _spinCooldownTimer -= dt;
            _dashStrikeTimer -= dt;
            _spinLinkTimer -= dt;

            if (Phase == ESwingPhase.Charging)
            {
                TickCharge(dt);
                return;
            }

            // The finisher's recovery is cancelled straight into the spin.
            if (Phase != ESwingPhase.None && CanLinkSpin())
            {
                StartSpin(true);
                return;
            }

            // The tail of a recovery is cancelled into the next attack, so a chain never waits out an
            // animation whose hit is already over. This is what makes the three swings read as one flow.
            if (CanCancelRecovery())
            {
                EndSwing();
                ConsumeAttackBuffer();
                return;
            }

            if (Phase != ESwingPhase.None) TickPhase(dt);
            if (Phase != ESwingPhase.None) return;

            TickComboReset(dt);

            if (_spinBufferTimer > 0f && _spinCooldownTimer <= 0f)
            {
                StartSpin(_spinLinkTimer > 0f);
                return;
            }

            if (_motorComponent.IsDashing || _bufferTimer <= 0f) return;

            // A press right out of a dash lunges instead of charging: the window is too short to hold anything.
            if (_dashStrikeTimer > 0f)
            {
                StartDashStrike();
                return;
            }

            ConsumeAttackBuffer();
        }

        void OnAttackPressed() => _bufferTimer = _inputBufferTime;
        void OnSpinPressed() => _spinBufferTimer = _inputBufferTime;
        void OnDashEnded() => _dashStrikeTimer = _dashStrikeWindow;

        // A tap continues the chain; a press still held starts a charge instead.
        void ConsumeAttackBuffer()
        {
            if (_inputComponent.IsAttackHeld) EnterCharge();
            else StartComboSwing(0f);
        }

        void TickComboReset(float dt)
        {
            if (_comboResetTimer <= 0f) return;
            _comboResetTimer -= dt;
            if (_comboResetTimer <= 0f) _comboIndex = 0;
        }

        void TickCharge(float dt)
        {
            _phaseTimer += dt;

            // Losing input (dialogue, camp, death) drops the charge instead of swinging on its own.
            if (_inputComponent.IsLocked)
            {
                CancelCharge();
                return;
            }

            if (_spinBufferTimer > 0f && _spinCooldownTimer <= 0f)
            {
                CancelCharge();
                StartSpin(false);
                return;
            }

            if (_inputComponent.IsAttackHeld) return;

            // A quick tap is just the next chain swing; the time already held counts toward its windup.
            if (_phaseTimer < _chargeMinTime)
            {
                StartComboSwing(_phaseTimer);
                return;
            }

            StartChargedStrike();
        }

        // True once the recovery has run past its cancel point with an attack still buffered.
        bool CanCancelRecovery()
        {
            if (Phase != ESwingPhase.Recovery || IsSpin || CurrentSwing == null) return false;
            if (_bufferTimer <= 0f || _motorComponent.IsDashing) return false;
            return _phaseTimer >= CurrentSwing.RecoveryCancelTime;
        }

        bool CanLinkSpin()
        {
            if (Phase != ESwingPhase.Recovery || IsSpin || !_isFinisher) return false;
            return _spinBufferTimer > 0f && _spinCooldownTimer <= 0f;
        }

        void EnterCharge()
        {
            _bufferTimer = 0f;
            IsSpin = false;
            CurrentSwing = null;
            AttackDamageMultiplier = 1f;
            _motorComponent.SpeedMultiplier = _moveSpeedWhileCharging;
            EnterPhase(ESwingPhase.Charging, 0f);
        }

        void CancelCharge()
        {
            Phase = ESwingPhase.None;
            _phaseTimer = 0f;
            _phaseDuration = 0f;
            _comboIndex = 0;
            _inChain = false;
            _motorComponent.SpeedMultiplier = 1f;
        }

        void StartComboSwing(float creditedWindup)
        {
            _bufferTimer = 0f;
            if (_comboSwings == null || _comboSwings.Length == 0) return;

            int index = Mathf.Clamp(_comboIndex, 0, _comboSwings.Length - 1);
            _isFinisher = index == _comboSwings.Length - 1;
            _comboIndex = _isFinisher ? 0 : index + 1;
            _inChain = true;
            StartSwing(_comboSwings[index], creditedWindup);
        }

        void StartDashStrike()
        {
            _dashStrikeTimer = 0f;
            _comboIndex = 0;
            _inChain = false;
            _isFinisher = false;
            StartSwing(_dashStrike, 0f);
        }

        void StartChargedStrike()
        {
            float charge = _chargeFullTime > _chargeMinTime ? Mathf.InverseLerp(_chargeMinTime, _chargeFullTime, _phaseTimer) : 1f;
            _comboIndex = 0;
            _inChain = false;
            _isFinisher = false;
            StartSwing(_chargedStrike, 0f);
            AttackDamageMultiplier *= Mathf.Lerp(_chargeMinDamageFraction, 1f, charge);
        }

        void StartSwing(SwingData swing, float creditedWindup)
        {
            if (swing == null) return;

            _bufferTimer = 0f;
            IsSpin = false;
            CurrentSwing = swing;
            AttackDamageMultiplier = swing.DamageMultiplier;
            // Turning is spread across the windup rather than snapped, so the body leads the blade.
            _aimComponent.TurnToAim(Mathf.Max(0f, swing.Windup - creditedWindup));
            _aimComponent.RotationLocked = true;
            _motorComponent.SpeedMultiplier = swing.MoveSpeedMultiplier;
            EnterPhase(ESwingPhase.Windup, Mathf.Max(0f, swing.Windup - creditedWindup));
            SwingStarted?.Invoke();
        }

        void StartSpin(bool linked)
        {
            _spinBufferTimer = 0f;
            _bufferTimer = 0f;
            _spinCooldownTimer = _spinCooldown;
            _spinLinkTimer = 0f;
            _comboIndex = 0;
            _inChain = false;
            _isFinisher = false;
            IsSpin = true;
            CurrentSwing = null;
            AttackDamageMultiplier = linked ? _linkedSpinDamageMultiplier : 1f;
            _aimComponent.RotationLocked = true;
            _motorComponent.SpeedMultiplier = _moveSpeedWhileSpinning;

            // Linked off the finisher or thrown mid-dash, the spin skips the windup: the opening is already there.
            bool skipWindup = linked || _motorComponent.IsDashing;
            EnterPhase(skipWindup ? ESwingPhase.Active : ESwingPhase.Windup, skipWindup ? _spinActive : _spinWindup);
            SwingStarted?.Invoke();
            if (skipWindup) SwingActive?.Invoke();
        }

        void TickPhase(float dt)
        {
            _phaseTimer += dt;
            if (_phaseTimer < _phaseDuration) return;

            switch (Phase)
            {
                case ESwingPhase.Windup:
                    EnterPhase(ESwingPhase.Active, IsSpin ? _spinActive : CurrentSwing.Active);
                    Lunge();
                    SwingActive?.Invoke();
                    break;
                case ESwingPhase.Active:
                    EnterPhase(ESwingPhase.Recovery, IsSpin ? _spinRecovery : CurrentSwing.Recovery);
                    if (_isFinisher) _spinLinkTimer = _phaseDuration + _spinLinkWindow;
                    break;
                case ESwingPhase.Recovery:
                    EndSwing();
                    break;
            }
        }

        // Carries the player into the swing. A dash already moves them, so it is left alone.
        void Lunge()
        {
            if (IsSpin || CurrentSwing == null || CurrentSwing.LungeDistance <= 0f) return;
            if (_motorComponent.IsDashing) return;
            _motorComponent.Push(_aimComponent.AimDirection, CurrentSwing.LungeDistance, CurrentSwing.Active, CurrentSwing.LungeDecay);
        }

        void EnterPhase(ESwingPhase phase, float duration)
        {
            Phase = phase;
            _phaseTimer = 0f;
            _phaseDuration = duration;
        }

        // Dash cancels a swing or a charge but not a spin, which carries on along the dash.
        // An attack pressed during the dash stays buffered.
        void OnDashStarted()
        {
            if (Phase == ESwingPhase.Charging)
            {
                CancelCharge();
                return;
            }

            if (Phase == ESwingPhase.None || IsSpin) return;
            _comboIndex = 0;
            EndSwing();
        }

        void EndSwing()
        {
            _comboResetTimer = _inChain ? _comboResetTime : 0f;
            if (!_inChain) _comboIndex = 0;

            Phase = ESwingPhase.None;
            IsSpin = false;
            CurrentSwing = null;
            AttackDamageMultiplier = 1f;
            _phaseTimer = 0f;
            _phaseDuration = 0f;
            _inChain = false;
            _isFinisher = false;
            _aimComponent.RotationLocked = false;
            _motorComponent.SpeedMultiplier = 1f;
            SwingEnded?.Invoke();
        }
    }
}
