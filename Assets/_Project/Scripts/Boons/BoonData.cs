using UnityEngine;

namespace Branded.Boons
{
    public enum ElementType { Physical, Fire, Holy, Bleed }

    // A temporary run boon (sword oil, charm, bandage...). The effect fields are what PlayerBoons applies.
    [CreateAssetMenu(fileName = "NewBoon", menuName = "Roguelite/Boon")]
    public class BoonData : ScriptableObject
    {
        public string boonName;
        public Sprite icon;
        [TextArea] public string description;
        public ElementType elementType;
        public float damageMultiplier = 1f;

        [Header("Effects")]
        public float burnDamagePerSecond;
        public float burnDuration;
        public float dashCooldownMultiplier = 1f;
        public float healAmount;
        [Tooltip("Can be offered again after being taken (e.g. one-off heals).")]
        public bool repeatable;
    }
}
