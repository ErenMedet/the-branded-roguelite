using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Loop
{
    [Serializable]
    public struct LightingPreset
    {
        [field: SerializeField, FormerlySerializedAs("sunColor")] public Color SunColor { get; private set; }
        [field: SerializeField, FormerlySerializedAs("sunIntensity")] public float SunIntensity { get; private set; }
        [field: SerializeField, FormerlySerializedAs("sunAngles")] public Vector3 SunAngles { get; private set; }
        [field: SerializeField, FormerlySerializedAs("ambient")] public Color Ambient { get; private set; }
        [field: SerializeField, FormerlySerializedAs("background")] public Color Background { get; private set; }
        [field: SerializeField, FormerlySerializedAs("torchIntensity")] public float TorchIntensity { get; private set; }

        public LightingPreset(Color sunColor, float sunIntensity, Vector3 sunAngles, Color ambient, Color background, float torchIntensity) : this()
        {
            SunColor = sunColor;
            SunIntensity = sunIntensity;
            SunAngles = sunAngles;
            Ambient = ambient;
            Background = background;
            TorchIntensity = torchIntensity;
        }
    }
}
