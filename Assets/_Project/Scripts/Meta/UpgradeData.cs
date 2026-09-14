using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Meta
{
    public enum EUpgradeStat { SwordDamage, MaxHealth, DeathDefiance }

    // A permanent upgrade bought with demon ash at Godo's forge or from Puck. One cost per level.
    [CreateAssetMenu(fileName = "NewUpgrade", menuName = "Roguelite/Upgrade")]
    public class UpgradeData : ScriptableObject
    {
        [field: Tooltip("Save key. Changing it loses the levels already bought.")]
        [field: SerializeField, FormerlySerializedAs("id")] public string Id { get; private set; }
        [field: SerializeField, FormerlySerializedAs("upgradeName")] public string UpgradeName { get; private set; }
        [field: SerializeField, TextArea, FormerlySerializedAs("description")] public string Description { get; private set; }
        [field: SerializeField, FormerlySerializedAs("stat")] public EUpgradeStat Stat { get; private set; }
        [field: Tooltip("Added per level: damage fraction, max health, or defiance charges.")]
        [field: SerializeField, FormerlySerializedAs("valuePerLevel")] public float ValuePerLevel { get; private set; }
        [field: SerializeField, FormerlySerializedAs("costs")] public int[] Costs { get; private set; }

        public int MaxLevel => Costs != null ? Costs.Length : 0;
    }
}
