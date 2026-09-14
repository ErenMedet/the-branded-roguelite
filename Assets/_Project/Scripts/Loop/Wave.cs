using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Loop
{
    [Serializable]
    public class Wave
    {
        [field: SerializeField, FormerlySerializedAs("delay")] public float Delay { get; private set; } = 15f;
        [field: SerializeField, FormerlySerializedAs("groups")] public SpawnGroup[] Groups { get; private set; }
    }
}
