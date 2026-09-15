using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Loop
{
    // A pack in a night's pool: spawned together from one spawn point.
    [Serializable]
    public class SpawnGroup
    {
        [field: SerializeField, FormerlySerializedAs("prefab")] public GameObject Prefab { get; private set; }
        [field: SerializeField, FormerlySerializedAs("count")] public int Count { get; private set; } = 1;
        // Spawned right beside each enemy of this group; a cultist leader can bring these back.
        [field: SerializeField] public GameObject Escort { get; private set; }
        [field: SerializeField] public int EscortCount { get; private set; }
        // Pressure of the whole pack, escorts included; the spawner keeps the field's total near the night's target.
        [field: SerializeField] public float Threat { get; private set; } = 3f;
        [field: SerializeField] public float Weight { get; private set; } = 1f;
    }
}
