using UnityEngine;

namespace Branded.Meta
{
    public enum UpgradeStat { SwordDamage, MaxHealth, DeathDefiance }

    // A permanent upgrade bought with demon ash at Godo's forge or from Puck. One cost per level.
    [CreateAssetMenu(fileName = "NewUpgrade", menuName = "Roguelite/Upgrade")]
    public class UpgradeData : ScriptableObject
    {
        [Tooltip("Save key. Changing it loses the levels already bought.")]
        public string id;
        public string upgradeName;
        [TextArea] public string description;
        public UpgradeStat stat;
        [Tooltip("Added per level: damage fraction, max health, or defiance charges.")]
        public float valuePerLevel;
        public int[] costs;

        public int MaxLevel => costs != null ? costs.Length : 0;
    }
}
