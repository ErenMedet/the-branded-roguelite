using UnityEngine;

namespace Branded.Combat
{
    // Fire damage over time. Re-applying refreshes the duration and keeps the stronger burn.
    public class BurnStatus : MonoBehaviour
    {
        const float TickInterval = 0.5f;

        HealthComponent _healthComponent;
        float _damagePerSecond;
        float _remaining;
        float _tickTimer;

        public static void Apply(HealthComponent target, float damagePerSecond, float duration)
        {
            if (!target || target.IsDead) return;
            if (!target.TryGetComponent(out BurnStatus burn))
            {
                burn = target.gameObject.AddComponent<BurnStatus>();
                burn._healthComponent = target;
            }
            if (!burn.enabled || burn._remaining <= 0f) burn._tickTimer = TickInterval;
            burn._damagePerSecond = Mathf.Max(burn._damagePerSecond, damagePerSecond);
            burn._remaining = Mathf.Max(burn._remaining, duration);
            burn.enabled = true;
        }

        void Update()
        {
            _remaining -= Time.deltaTime;
            _tickTimer -= Time.deltaTime;
            if (_tickTimer <= 0f)
            {
                _tickTimer = TickInterval;
                _healthComponent.TakeTickDamage(_damagePerSecond * TickInterval);
            }
            if (_remaining > 0f && !_healthComponent.IsDead) return;
            _damagePerSecond = 0f;
            enabled = false;
        }
    }
}
