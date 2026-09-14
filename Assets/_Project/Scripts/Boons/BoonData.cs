using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Boons
{
    // A temporary run boon (sword oil, charm, bandage...). The effect values are what PlayerBoons applies.
    [CreateAssetMenu(fileName = "NewBoon", menuName = "Roguelite/Boon")]
    public class BoonData : ScriptableObject
    {
        [field: SerializeField, FormerlySerializedAs("boonName")] public string BoonName { get; private set; }
        [field: SerializeField, FormerlySerializedAs("icon")] public Sprite Icon { get; private set; }
        [field: SerializeField, TextArea, FormerlySerializedAs("description")] public string Description { get; private set; }
        [field: SerializeField, FormerlySerializedAs("elementType")] public EElementType ElementType { get; private set; }
        [field: SerializeField, FormerlySerializedAs("damageMultiplier")] public float DamageMultiplier { get; private set; } = 1f;

        [field: Header("Effects")]
        [field: SerializeField, FormerlySerializedAs("burnDamagePerSecond")] public float BurnDamagePerSecond { get; private set; }
        [field: SerializeField, FormerlySerializedAs("burnDuration")] public float BurnDuration { get; private set; }
        [field: SerializeField, FormerlySerializedAs("dashCooldownMultiplier")] public float DashCooldownMultiplier { get; private set; } = 1f;
        [field: SerializeField, FormerlySerializedAs("healAmount")] public float HealAmount { get; private set; }
        [field: Tooltip("Can be offered again after being taken (e.g. one-off heals).")]
        [field: SerializeField, FormerlySerializedAs("repeatable")] public bool Repeatable { get; private set; }
    }
}
