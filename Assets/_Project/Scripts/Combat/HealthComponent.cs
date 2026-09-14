using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Branded.Combat
{
    // Event-driven health. UI, effects and death logic listen to the events; nothing polls this.
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [field: SerializeField, FormerlySerializedAs("maxHealth")] public float MaxHealth { get; private set; } = 100f;
        public float CurrentHealth { get; private set; }

        public bool IsDead => CurrentHealth <= 0f;

        public event UnityAction<float, float> HealthChanged; // (current, max)
        public event UnityAction<float, Vector3> Damaged;     // (amount, hitDirection)
        public event UnityAction Died;

        IInvulnerabilitySource[] _invulnerabilitySources;
        IDamageBlocker[] _damageBlockers;

        void Awake()
        {
            CurrentHealth = MaxHealth;
            _invulnerabilitySources = GetComponents<IInvulnerabilitySource>();
            _damageBlockers = GetComponents<IDamageBlocker>();
        }

        public void TakeDamage(float amount, Vector3 hitDirection)
        {
            if (!CanBeHurt(amount) || IsBlocked(amount, hitDirection)) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            Damaged?.Invoke(amount, hitDirection);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
            if (IsDead) Died?.Invoke();
        }

        // Damage over time (e.g. burn): lowers health without Damaged, so it doesn't flash or stagger.
        public void TakeTickDamage(float amount)
        {
            if (!CanBeHurt(amount)) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
            if (IsDead) Died?.Invoke();
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        // Permanent upgrades change the cap; the bar starts full at the new size.
        public void SetMaxHealth(float max)
        {
            MaxHealth = Mathf.Max(1f, max);
            CurrentHealth = MaxHealth;
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        // Death Defiance: back from zero on the spot.
        public void Revive(float amount)
        {
            CurrentHealth = Mathf.Clamp(amount, 1f, MaxHealth);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        bool CanBeHurt(float amount)
        {
            if (IsDead || amount <= 0f) return false;
            foreach (var source in _invulnerabilitySources)
                if (source.IsInvulnerable) return false;
            return true;
        }

        // Only direct hits are checked: damage over time has no direction to block.
        bool IsBlocked(float amount, Vector3 hitDirection)
        {
            foreach (var blocker in _damageBlockers)
                if (blocker.Blocks(amount, hitDirection)) return true;
            return false;
        }
    }
}
