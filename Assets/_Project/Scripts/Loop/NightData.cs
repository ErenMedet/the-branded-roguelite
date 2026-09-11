using System;
using Branded.Boons;
using Branded.Dialogue;
using UnityEngine;

namespace Branded.Loop
{
    // One night's length and enemy waves, plus the morning camp that follows it.
    // Each wave comes `delay` seconds after the previous one finished spawning.
    [CreateAssetMenu(fileName = "NewNight", menuName = "Roguelite/Night")]
    public class NightData : ScriptableObject
    {
        public float duration = 90f;
        public Wave[] waves;

        [Header("Morning")]
        public DialogueData morningDialogue;
        public BoonData[] boonPool;
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
