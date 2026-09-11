using System;
using UnityEngine;

namespace Branded.Loop
{
    // One night's length and enemy waves. Each wave comes `delay` seconds after the previous one finished spawning.
    [CreateAssetMenu(fileName = "NewNight", menuName = "Roguelite/Night")]
    public class NightData : ScriptableObject
    {
        public float duration = 90f;
        public Wave[] waves;
    }

    [Serializable]
    public class Wave
    {
        public float delay = 15f;
        public SpawnGroup[] groups;
    }

    [Serializable]
    public class SpawnGroup
    {
        public GameObject prefab;
        public int count = 1;
    }
}
