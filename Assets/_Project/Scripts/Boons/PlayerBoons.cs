using System.Collections.Generic;
using Branded.Combat;
using Branded.Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Branded.Boons
{
    // The run's temporary boons. Their stats are pushed into the player's components; all of it is lost on
    // death, when the run scene unloads.
    public class PlayerBoons : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("sword")] SwordHitbox _swordComponent;
        [SerializeField, FormerlySerializedAs("motor")] PlayerMotor _motorComponent;
        [SerializeField, FormerlySerializedAs("health")] HealthComponent _healthComponent;

        readonly List<BoonData> _active = new();
        float _burnDamagePerSecond;
        float _burnDuration;

        public IReadOnlyList<BoonData> Active => _active;
        public event UnityAction<BoonData> BoonAdded;

        void Awake()
        {
            if (!_swordComponent) _swordComponent = GetComponent<SwordHitbox>();
            if (!_motorComponent) _motorComponent = GetComponent<PlayerMotor>();
            if (!_healthComponent) _healthComponent = GetComponent<HealthComponent>();
        }

        void OnEnable() => _swordComponent.TargetHit += OnTargetHit;
        void OnDisable() => _swordComponent.TargetHit -= OnTargetHit;

        public void Add(BoonData boon)
        {
            _active.Add(boon);
            Recalculate();
            if (boon.HealAmount > 0f) _healthComponent.Heal(boon.HealAmount);
            BoonAdded?.Invoke(boon);
        }

        // Up to `count` random boons from the pool, skipping ones already taken unless they are repeatable.
        public List<BoonData> PickOffers(BoonData[] pool, int count)
        {
            var offers = new List<BoonData>();
            if (pool != null)
                foreach (var boon in pool)
                    if (boon && (boon.Repeatable || !_active.Contains(boon))) offers.Add(boon);

            for (int i = offers.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
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
                damage *= boon.DamageMultiplier;
                dashCooldown *= boon.DashCooldownMultiplier;
                _burnDamagePerSecond += boon.BurnDamagePerSecond;
                _burnDuration = Mathf.Max(_burnDuration, boon.BurnDuration);
            }
            _swordComponent.DamageMultiplier = damage;
            _motorComponent.DashCooldownMultiplier = dashCooldown;
        }

        void OnTargetHit(IDamageable target)
        {
            if (_burnDamagePerSecond <= 0f || target is not HealthComponent victim) return;
            BurnStatus.Apply(victim, _burnDamagePerSecond, _burnDuration);
        }
    }
}
