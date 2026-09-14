using UnityEngine;

namespace Branded.Combat
{
    // Freezes the game for a few frames on impact. Overlapping calls extend the freeze instead of stacking.
    public class HitStop : MonoBehaviour
    {
        static HitStop _instanceComponent;

        float _resumeAt;
        bool _frozen;

        public static void Trigger(float duration)
        {
            if (duration <= 0f) return;
            if (!_instanceComponent)
            {
                var go = new GameObject("[HitStop]");
                DontDestroyOnLoad(go);
                _instanceComponent = go.AddComponent<HitStop>();
            }
            _instanceComponent.Freeze(duration);
        }

        void Freeze(float duration)
        {
            _resumeAt = Mathf.Max(_resumeAt, Time.unscaledTime + duration);
            if (_frozen) return;
            _frozen = true;
            Time.timeScale = 0f;
        }

        void Update()
        {
            if (!_frozen || Time.unscaledTime < _resumeAt) return;
            Resume();
        }

        void Resume()
        {
            _frozen = false;
            Time.timeScale = 1f;
        }

        void OnDestroy()
        {
            if (_frozen) Time.timeScale = 1f;
        }
    }
}
