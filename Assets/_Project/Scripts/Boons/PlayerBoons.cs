using System;
using System.Collections.Generic;
using Branded.Combat;
using Branded.Player;
using UnityEngine;

namespace Branded.Boons
{
    // The run's temporary boons. Their stats are pushed into the player's components; all of it is lost on
    // death (for now the scene reload, later Godo's forge).
    public class PlayerBoons : MonoBehaviour
    {
        [SerializeField] SwordHitbox sword;
        [SerializeField] PlayerMotor motor;
        [SerializeField] HealthComponent health;

        readonly List<BoonData> _active = new();
        float _burnDamagePerSecond;
        float _burnDuration;

        public IReadOnlyList<BoonData> Active => _active;
        public event Action<BoonData> BoonAdded;

        void Awake()
        {
            if (!sword) sword = GetComponent<SwordHitbox>();
            if (!motor) motor = GetComponent<PlayerMotor>();
            if (!health) health = GetComponent<HealthComponent>();
        }

        void OnEnable() => sword.TargetHit += OnTargetHit;
        void OnDisable() => sword.TargetHit -= OnTargetHit;

        public void Add(BoonData boon)
        {
            _active.Add(boon);
            Recalculate();
            if (boon.healAmount > 0f) health.Heal(boon.healAmount);
            BoonAdded?.Invoke(boon);
        }

        // Up to `count` random boons from the pool, skipping ones already taken unless they are repeatable.
        public List<BoonData> PickOffers(BoonData[] pool, int count)
        {
            var offers = new List<BoonData>();
            if (pool != null)
                foreach (var boon in pool)
                    if (boon && (boon.repeatable || !_active.Contains(boon))) offers.Add(boon);

            for (int i = offers.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (offers[i], offers[j]) = (offers[j], offers[i]);
            }
            if (offers.Count > count) offers.RemoveRange(count, offers.Count - count);
            return offers;
        }

        void Recalculate()
        {
            float damage = 1f;
            float dashCooldown = 1f;
            _burnDamagePerSecond = 0f;
            _burnDuration = 0f;
            foreach (var boon in _active)
            {
                damage *= boon.damageMultiplier;
                dashCooldown *= boon.dashCooldownMultiplier;
                _burnDamagePerSecond += boon.burnDamagePerSecond;
                _burnDuration = Mathf.Max(_burnDuration, boon.burnDuration);
            }
            sword.DamageMultiplier = damage;
            motor.DashCooldownMultiplier = dashCooldown;
        }

        void OnTargetHit(IDamageable target)
        {
            if (_burnDamagePerSecond > 0f && target is HealthComponent victim)
                BurnStatus.Apply(victim, _burnDamagePerSecond, _burnDuration);
        }
    }
}
