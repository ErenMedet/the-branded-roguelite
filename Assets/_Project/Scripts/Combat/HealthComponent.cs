using System;
using UnityEngine;

namespace Branded.Combat
{
    // Event-driven health. UI, effects and death logic listen to the events; nothing polls this.
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        public float maxHealth = 100f;
        public float currentHealth;

        public bool IsDead => currentHealth <= 0f;

        public event Action<float, float> OnHealthChanged; // (current, max)
        public event Action<float, Vector3> OnDamaged;     // (amount, hitDirection)
        public event Action OnDeath;

        IInvulnerabilitySource[] _invulnerabilitySources;

        void Awake()
        {
            currentHealth = maxHealth;
            _invulnerabilitySources = GetComponents<IInvulnerabilitySource>();
        }

        public void TakeDamage(float amount, Vector3 hitDirection)
        {
            if (!CanBeHurt(amount)) return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            OnDamaged?.Invoke(amount, hitDirection);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            if (currentHealth <= 0f) OnDeath?.Invoke();
        }

        // Damage over time (e.g. burn): lowers health without OnDamaged, so it doesn't flash or stagger.
        public void TakeTickDamage(float amount)
        {
            if (!CanBeHurt(amount)) return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            if (currentHealth <= 0f) OnDeath?.Invoke();
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        bool CanBeHurt(float amount)
        {
            if (IsDead || amount <= 0f) return false;
            foreach (var source in _invulnerabilitySources)
                if (source.IsInvulnerable) return false;
            return true;
        }
    }
}
