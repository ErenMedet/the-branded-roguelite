using UnityEngine;
using UnityEngine.Events;

namespace Branded.Player
{
    // Owns which weapon the hands are on. The Dragonslayer takes both hands, so aiming the crossbow
    // means letting go with the left one first; the two transitions are real states because the arms
    // have a clip to play through. Nothing here gates the sword: a swing may start while the left arm
    // is still coming home, which is what the stow animation is for.
    public class PlayerGrip : MonoBehaviour
    {
        [SerializeField] PlayerInputReader _inputComponent;
        [SerializeField] PlayerCrossbow _crossbowComponent;
        [Tooltip("Shown from the first frame of the draw until the stow finishes.")]
        [SerializeField] GameObject _crossbowVisual;

        [Header("Timing (seconds)")]
        [Tooltip("Set to 0 to make the switch instant and leave the animation purely cosmetic.")]
        [SerializeField] float _drawDuration = 0.2f;
        [SerializeField] float _stowDuration = 0.15f;

        public EGripState State { get; private set; } = EGripState.Sword;
        // 0 to 1 through the current transition; 1 while resting in Sword or Crossbow.
        public float SwitchProgress => _duration > 0f ? Mathf.Clamp01(_timer / _duration) : 1f;
        public bool IsCrossbowReady => State == EGripState.Crossbow;

        public event UnityAction<EGripState> GripChanged;

        float _timer;
        float _duration;
        // A swing sends the left hand back even with aim still held, until the player re-presses aim.
        bool _attackOverride;

        void Awake()
        {
            if (!_inputComponent) _inputComponent = GetComponent<PlayerInputReader>();
            if (!_crossbowComponent) _crossbowComponent = GetComponent<PlayerCrossbow>();
            ApplyVisual();
        }

        void OnEnable() => _inputComponent.AttackPressed += OnAttackPressed;
        void OnDisable() => _inputComponent.AttackPressed -= OnAttackPressed;

        void Update()
        {
            bool wantsCrossbow = WantsCrossbow();
            _timer += Time.deltaTime;

            switch (State)
            {
                case EGripState.Sword:
                    if (wantsCrossbow) Enter(EGripState.Drawing, _drawDuration, 0f);
                    break;
                case EGripState.Drawing:
                    if (!wantsCrossbow) Reverse(EGripState.Stowing, _stowDuration);
                    else if (_timer >= _duration) Enter(EGripState.Crossbow, 0f, 0f);
                    break;
                case EGripState.Crossbow:
                    if (!wantsCrossbow) Enter(EGripState.Stowing, _stowDuration, 0f);
                    break;
                case EGripState.Stowing:
                    if (wantsCrossbow) Reverse(EGripState.Drawing, _drawDuration);
                    else if (_timer >= _duration) Enter(EGripState.Sword, 0f, 0f);
                    break;
            }
        }

        // Losing input to a dialogue or a repair bench puts the sword back in both hands.
        bool WantsCrossbow()
        {
            if (!_inputComponent.IsFireHeld) _attackOverride = false;
            if (_inputComponent.IsLocked || _attackOverride) return false;
            return _inputComponent.IsFireHeld && _crossbowComponent && !_crossbowComponent.IsBroken;
        }

        void OnAttackPressed()
        {
            _attackOverride = true;
            if (State == EGripState.Crossbow) Enter(EGripState.Stowing, _stowDuration, 0f);
            else if (State == EGripState.Drawing) Reverse(EGripState.Stowing, _stowDuration);
        }

        // Turning back mid-transition picks up where the arm actually is instead of replaying the
        // whole clip, so tapping aim on and off doesn't make the hand jump.
        void Reverse(EGripState state, float duration) => Enter(state, duration, duration * (1f - SwitchProgress));

        void Enter(EGripState state, float duration, float elapsed)
        {
            State = state;
            _duration = duration;
            _timer = elapsed;
            ApplyVisual();
            GripChanged?.Invoke(state);
        }

        void ApplyVisual()
        {
            if (!_crossbowVisual) return;
            bool broken = _crossbowComponent && _crossbowComponent.IsBroken;
            _crossbowVisual.SetActive(State != EGripState.Sword && !broken);
        }
    }
}
