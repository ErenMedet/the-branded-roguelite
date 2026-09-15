using UnityEngine;

namespace Branded.UI
{
    // A health bar's pale trail: after a hit it holds the lost chunk for a moment, then slides down to the value.
    // Healing moves it up at once. A plain class: the bar sets the target and draws what Tick returns.
    public class BarTrail
    {
        readonly float _delay;
        readonly float _speed; // bar fraction per second
        float _target = 1f;
        float _holdTimer;

        public float Value { get; private set; } = 1f;

        public BarTrail(float delay, float speed)
        {
            _delay = delay;
            _speed = speed;
        }

        public void SetTarget(float target)
        {
            _target = target;
            _holdTimer = _delay;
        }

        // Jumps straight to the target, e.g. when the bar first appears.
        public void Snap() => Value = _target;

        // The caller picks the clock: scaled for world bars, unscaled for the HUD.
        public float Tick(float deltaTime)
        {
            if (Value <= _target) Value = _target;
            else if (_holdTimer > 0f) _holdTimer -= deltaTime;
            else Value = Mathf.MoveTowards(Value, _target, _speed * deltaTime);
            return Value;
        }
    }
}
