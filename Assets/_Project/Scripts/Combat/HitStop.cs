using Branded.Core;
using UnityEngine;

namespace Branded.Combat
{
    // Freezes the game for a few frames on impact, then ramps time back up instead of snapping to 1:
    // a hard 0 to 1 step reads as a dropped frame, while a short ramp reads as the blow landing.
    // Overlapping calls extend the freeze instead of stacking.
    public class HitStop : MonoBehaviour
    {
        const float DefaultRamp = 0.06f;

        static HitStop _instanceComponent;

        float _resumeAt;
        float _rampDuration;
        float _rampTimer;
        bool _frozen;

        // Domain reload is off, so the reference to last session's object has to be dropped by hand.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => _instanceComponent = null;

        public static void Trigger(float duration, float ramp = DefaultRamp)
        {
            if (duration <= 0f) return;
            if (!_instanceComponent)
            {
                var go = new GameObject("[HitStop]");
                DontDestroyOnLoad(go);
                _instanceComponent = go.AddComponent<HitStop>();
            }
            _instanceComponent.Freeze(duration, ramp);
        }

        void Freeze(float duration, float ramp)
        {
            // A hit landing mid-ramp drops straight back to a full freeze, so the second blow reads too.
            _resumeAt = Mathf.Max(_resumeAt, Time.unscaledTime + duration);
            _rampDuration = Mathf.Max(0f, ramp);
            _rampTimer = 0f;
            _frozen = true;
            Time.timeScale = 0f;
        }

        void Update()
        {
            if (!_frozen) return;
            if (Time.unscaledTime < _resumeAt) return;

            if (_rampDuration <= 0f)
            {
                Resume();
                return;
            }

            _rampTimer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(_rampTimer / _rampDuration);
            Time.timeScale = Easing.OutCubic(t);
            if (t >= 1f) Resume();
        }

        void Resume()
        {
            _frozen = false;
            _rampDuration = 0f;
            _rampTimer = 0f;
            Time.timeScale = 1f;
        }

        void OnDestroy()
        {
            if (_frozen) Time.timeScale = 1f;
        }
    }
}
