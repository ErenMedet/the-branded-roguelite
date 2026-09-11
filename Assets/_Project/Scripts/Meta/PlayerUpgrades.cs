using Branded.Combat;
using Branded.Player;
using UnityEngine;

namespace Branded.Meta
{
    // Applies the permanent upgrades to the player on spawn, and again right after a purchase in the hub.
    public class PlayerUpgrades : MonoBehaviour
    {
        [SerializeField] UpgradeData[] upgrades;
        [SerializeField] SwordHitbox sword;
        [SerializeField] HealthComponent health;

        float _baseMaxHealth;

        public int DefianceCharges { get; private set; }

        void Awake()
        {
            if (!sword) sword = GetComponent<SwordHitbox>();
            if (!health) health = GetComponent<HealthComponent>();
            _baseMaxHealth = health.maxHealth;
        }

        void OnEnable() => Progress.UpgradesChanged += Apply;
        void OnDisable() => Progress.UpgradesChanged -= Apply;
        void Start() => Apply();

        public bool TryUseDefiance()
        {
            if (DefianceCharges <= 0) return false;
            DefianceCharges--;
            return true;
        }

        void Apply()
        {
            float damage = 0f, maxHealth = 0f, defiance = 0f;
            foreach (var upgrade in upgrades)
            {
                if (!upgrade) continue;
                float bonus = upgrade.valuePerLevel * Progress.LevelOf(upgrade);
                switch (upgrade.stat)
                {
                    case UpgradeStat.SwordDamage: damage += bonus; break;
                    case UpgradeStat.MaxHealth: maxHealth += bonus; break;
                    case UpgradeStat.DeathDefiance: defiance += bonus; break;
                }
            }
            sword.BaseDamageMultiplier = 1f + damage;
            health.SetMaxHealth(_baseMaxHealth + maxHealth);
            DefianceCharges = Mathf.RoundToInt(defiance);
        }
    }
}
