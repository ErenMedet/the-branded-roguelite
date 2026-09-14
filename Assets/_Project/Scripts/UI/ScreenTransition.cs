using System.Collections;
using Branded.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.UI
{
    // Black overlay for scene changes. Every scene fades in from it; on death the brand's lines appear over it.
    // Answers HubExited with ScreenFadedOut and PlayerDied with DeathScreenFinished.
    public class ScreenTransition : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("overlay")] CanvasGroup _overlayComponent;
        [SerializeField, FormerlySerializedAs("deathLines")] TMP_Text[] _deathLineComponents; // shown one after the other
        [SerializeField, FormerlySerializedAs("fadeInOnStart")] float _fadeInOnStart = 1f;
        [SerializeField, FormerlySerializedAs("deathFade")] float _deathFade = 1.2f;
        [SerializeField, FormerlySerializedAs("lineFade")] float _lineFade = 0.8f;
        [SerializeField, FormerlySerializedAs("lineHold")] float _lineHold = 1.8f;

        int _fadeId; // a newer fade takes over from an older one

        void OnEnable()
        {
            GameEvents.HubExited += OnHubExited;
            GameEvents.PlayerDied += OnPlayerDied;
        }

        void OnDisable()
        {
            GameEvents.HubExited -= OnHubExited;
            GameEvents.PlayerDied -= OnPlayerDied;
        }

        void Start()
        {
            foreach (var line in _deathLineComponents) line.alpha = 0f;
            _overlayComponent.alpha = 1f;
            StartCoroutine(Fade(0f, _fadeInOnStart));
        }

        void OnHubExited(float fadeDuration) => StartCoroutine(FadeOut(fadeDuration));
        void OnPlayerDied() => StartCoroutine(PlayDeath());

        IEnumerator FadeOut(float duration)
        {
            yield return Fade(1f, duration);
            GameEvents.RaiseScreenFadedOut();
        }

        IEnumerator PlayDeath()
        {
            yield return Fade(1f, _deathFade);
            foreach (var line in _deathLineComponents)
            {
                for (float t = 0f; t < _lineFade; t += Time.unscaledDeltaTime)
                {
                    line.alpha = t / _lineFade;
                    yield return null;
                }
                line.alpha = 1f;
                yield return new WaitForSecondsRealtime(_lineHold);
            }
            GameEvents.RaiseDeathScreenFinished();
        }

        IEnumerator Fade(float target, float duration)
        {
            int id = ++_fadeId;
            _overlayComponent.blocksRaycasts = true;
            float start = _overlayComponent.alpha;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                if (id != _fadeId) yield break;
                _overlayComponent.alpha = Mathf.Lerp(start, target, t / duration);
                yield return null;
            }
            if (id != _fadeId) yield break;
            _overlayComponent.alpha = target;
            _overlayComponent.blocksRaycasts = target > 0f;
        }
    }
}
