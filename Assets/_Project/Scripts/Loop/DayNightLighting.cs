using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Loop
{
    // Blends the scene between the night and morning looks: 0 = night, 1 = morning.
    public class DayNightLighting : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("sun")] Light _sunComponent;
        [SerializeField, FormerlySerializedAs("torch")] Light _torchComponent;
        [SerializeField, FormerlySerializedAs("cam")] Camera _cameraComponent;
        [SerializeField, FormerlySerializedAs("night")] LightingPreset _night = new(
            sunColor: new Color(0.55f, 0.65f, 0.9f),
            sunIntensity: 0.55f,
            sunAngles: new Vector3(50f, 330f, 0f),
            ambient: new Color(0.12f, 0.13f, 0.18f),
            background: new Color(0.03f, 0.035f, 0.05f),
            torchIntensity: 45f);
        [SerializeField, FormerlySerializedAs("morning")] LightingPreset _morning = new(
            sunColor: new Color(1f, 0.8f, 0.58f),
            sunIntensity: 1.3f,
            sunAngles: new Vector3(28f, 300f, 0f),
            ambient: new Color(0.42f, 0.4f, 0.42f),
            background: new Color(0.62f, 0.55f, 0.5f),
            torchIntensity: 10f);

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
            if (_sunComponent)
            {
                _sunComponent.color = Color.Lerp(_night.SunColor, _morning.SunColor, blend);
                _sunComponent.intensity = Mathf.Lerp(_night.SunIntensity, _morning.SunIntensity, blend);
                _sunComponent.transform.rotation = Quaternion.Slerp(Quaternion.Euler(_night.SunAngles), Quaternion.Euler(_morning.SunAngles), blend);
                // Only the torch casts shadows at night (GDD light rule); the morning sun takes over.
                _sunComponent.shadows = blend > 0.5f ? LightShadows.Soft : LightShadows.None;
            }
            if (_torchComponent) _torchComponent.intensity = Mathf.Lerp(_night.TorchIntensity, _morning.TorchIntensity, blend);
            if (_cameraComponent) _cameraComponent.backgroundColor = Color.Lerp(_night.Background, _morning.Background, blend);
            RenderSettings.ambientLight = Color.Lerp(_night.Ambient, _morning.Ambient, blend);
        }
    }
}
