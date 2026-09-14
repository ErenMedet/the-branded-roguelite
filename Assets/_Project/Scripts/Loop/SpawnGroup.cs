using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Loop
{
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
