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
            if (IsDead || amount <= 0f) return;
            foreach (var source in _invulnerabilitySources)
                if (source.IsInvulnerable) return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            OnDamaged?.Invoke(amount, hitDirection);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            if (currentHealth <= 0f) OnDeath?.Invoke();
        }
    }
}
