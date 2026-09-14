using Branded.Combat;
using Branded.Core;
using Branded.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Meta
{
    // Applies the permanent upgrades to the player on spawn, and again right after a purchase in the hub.
    public class PlayerUpgrades : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("upgrades")] UpgradeData[] _upgrades;
        [SerializeField, FormerlySerializedAs("sword")] SwordHitbox _swordComponent;
        [SerializeField, FormerlySerializedAs("health")] HealthComponent _healthComponent;

        float _baseMaxHealth;

        public int DefianceCharges { get; private set; }

        void Awake()
        {
            if (!_swordComponent) _swordComponent = GetComponent<SwordHitbox>();
            if (!_healthComponent) _healthComponent = GetComponent<HealthComponent>();
            _baseMaxHealth = _healthComponent.MaxHealth;
        }

        void OnEnable() => GameEvents.UpgradesChanged += OnUpgradesChanged;
        void OnDisable() => GameEvents.UpgradesChanged -= OnUpgradesChanged;
        void Start() => Apply();

        public bool TryUseDefiance()
        {
            if (DefianceCharges <= 0) return false;
            DefianceCharges--;
            return true;
        }

        void OnUpgradesChanged() => Apply();

        void Apply()
        {
            float damage = 0f, maxHealth = 0f, defiance = 0f;
            foreach (var upgrade in _upgrades)
            {
                if (!upgrade) continue;
                float bonus = upgrade.ValuePerLevel * Progress.LevelOf(upgrade);
                switch (upgrade.Stat)
                {
                    case EUpgradeStat.SwordDamage: damage += bonus; break;
                    case EUpgradeStat.MaxHealth: maxHealth += bonus; break;
                    case EUpgradeStat.DeathDefiance: defiance += bonus; break;
                }
            }
            _swordComponent.BaseDamageMultiplier = 1f + damage;
            _healthComponent.SetMaxHealth(_baseMaxHealth + maxHealth);
            DefianceCharges = Mathf.RoundToInt(defiance);
        }
    }
}
