using Branded.Boons;
using Branded.Dialogue;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Loop
{
    // One night's length and enemy waves, plus the morning camp that follows it.
    // Each wave comes `Delay` seconds after the previous one finished spawning.
    [CreateAssetMenu(fileName = "NewNight", menuName = "Roguelite/Night")]
    public class NightData : ScriptableObject
    {
        [field: SerializeField, FormerlySerializedAs("duration")] public float Duration { get; private set; } = 90f;
        [field: SerializeField, FormerlySerializedAs("waves")] public Wave[] Waves { get; private set; }

        [field: Header("Morning")]
        [field: SerializeField, FormerlySerializedAs("morningDialogue")] public DialogueData MorningDialogue { get; private set; }
        [field: SerializeField, FormerlySerializedAs("boonPool")] public BoonData[] BoonPool { get; private set; }
    }
}
