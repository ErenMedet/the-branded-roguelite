using System;
using System.Collections;
using UnityEngine;

namespace Branded.Loop
{
    [Serializable]
    public struct LightingPreset
    {
        public Color sunColor;
        public float sunIntensity;
        public Vector3 sunAngles;
        public Color ambient;
        public Color background;
        public float torchIntensity;
    }

    // Blends the scene between the night and morning looks: 0 = night, 1 = morning.
    public class DayNightLighting : MonoBehaviour
    {
        [SerializeField] Light sun;
        [SerializeField] Light torch;
        [SerializeField] Camera cam;
        [SerializeField] LightingPreset night = new LightingPreset
        {
            sunColor = new Color(0.55f, 0.65f, 0.9f),
            sunIntensity = 0.55f,
            sunAngles = new Vector3(50f, 330f, 0f),
            ambient = new Color(0.12f, 0.13f, 0.18f),
            background = new Color(0.03f, 0.035f, 0.05f),
            torchIntensity = 45f,
        };
        [SerializeField] LightingPreset morning = new LightingPreset
        {
            sunColor = new Color(1f, 0.8f, 0.58f),
            sunIntensity = 1.3f,
            sunAngles = new Vector3(28f, 300f, 0f),
            ambient = new Color(0.42f, 0.4f, 0.42f),
            background = new Color(0.62f, 0.55f, 0.5f),
            torchIntensity = 10f,
        };

        public float Blend { get; private set; }

        public IEnumerator BlendTo(float target, float duration)
        {
            float from = Blend;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                Apply(Mathf.Lerp(from, target, Mathf.SmoothStep(0f, 1f, t / duration)));
                yield return null;
            }
            Apply(target);
        }

        public void Apply(float blend)
        {
            Blend = blend;
            if (sun)
            {
                sun.color = Color.Lerp(night.sunColor, morning.sunColor, blend);
                sun.intensity = Mathf.Lerp(night.sunIntensity, morning.sunIntensity, blend);
                sun.transform.rotation = Quaternion.Slerp(Quaternion.Euler(night.sunAngles), Quaternion.Euler(morning.sunAngles), blend);
                // Only the torch casts shadows at night (GDD light rule); the morning sun takes over.
                sun.shadows = blend > 0.5f ? LightShadows.Soft : LightShadows.None;
            }
            if (torch) torch.intensity = Mathf.Lerp(night.torchIntensity, morning.torchIntensity, blend);
            if (cam) cam.backgroundColor = Color.Lerp(night.background, morning.background, blend);
            RenderSettings.ambientLight = Color.Lerp(night.ambient, morning.ambient, blend);
        }
    }
}
