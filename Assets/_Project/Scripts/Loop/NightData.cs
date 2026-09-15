using Branded.Boons;
using Branded.Dialogue;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Loop
{
    // One night's length and the packs that can come during it, plus the morning camp that follows it.
    // How much comes at once is the spawner's job, since it grows with the night number rather than per asset.
    [CreateAssetMenu(fileName = "NewNight", menuName = "Roguelite/Night")]
    public class NightData : ScriptableObject
    {
        [field: SerializeField, FormerlySerializedAs("duration")] public float Duration { get; private set; } = 90f;
        [field: SerializeField] public SpawnGroup[] Pool { get; private set; }

        [field: Header("Morning")]
        [field: SerializeField, FormerlySerializedAs("morningDialogue")] public DialogueData MorningDialogue { get; private set; }
        [field: SerializeField, FormerlySerializedAs("boonPool")] public BoonData[] BoonPool { get; private set; }
    }
}
