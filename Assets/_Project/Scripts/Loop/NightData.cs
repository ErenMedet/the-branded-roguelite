using System;
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

    [Serializable]
    public class Wave
    {
        [field: SerializeField, FormerlySerializedAs("delay")] public float Delay { get; private set; } = 15f;
        [field: SerializeField, FormerlySerializedAs("groups")] public SpawnGroup[] Groups { get; private set; }
    }

    [Serializable]
    public class SpawnGroup
    {
        [field: SerializeField, FormerlySerializedAs("prefab")] public GameObject Prefab { get; private set; }
        [field: SerializeField, FormerlySerializedAs("count")] public int Count { get; private set; } = 1;
        // Spawned right beside each enemy of this group; a cultist leader can bring these back.
        [field: SerializeField] public GameObject Escort { get; private set; }
        [field: SerializeField] public int EscortCount { get; private set; }
    }
}
