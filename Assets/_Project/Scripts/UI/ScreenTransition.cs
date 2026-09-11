using System.Collections;
using TMPro;
using UnityEngine;

namespace Branded.UI
{
    // Black overlay for scene changes. Every scene fades in from it; on death the brand's lines appear over it.
    public class ScreenTransition : MonoBehaviour
    {
        [SerializeField] CanvasGroup overlay;
        [SerializeField] TMP_Text[] deathLines; // shown one after the other
        [SerializeField] float fadeInOnStart = 1f;
        [SerializeField] float deathFade = 1.2f;
        [SerializeField] float lineFade = 0.8f;
        [SerializeField] float lineHold = 1.8f;

        int _fadeId; // a newer fade takes over from an older one

        void Start()
        {
            foreach (var line in deathLines) line.alpha = 0f;
            overlay.alpha = 1f;
            StartCoroutine(Fade(0f, fadeInOnStart));
        }

        public IEnumerator FadeOut(float duration) => Fade(1f, duration);

        public IEnumerator PlayDeath()
        {
            yield return Fade(1f, deathFade);
            foreach (var line in deathLines)
            {
                for (float t = 0f; t < lineFade; t += Time.unscaledDeltaTime)
                {
                    line.alpha = t / lineFade;
                    yield return null;
                }
                line.alpha = 1f;
                yield return new WaitForSecondsRealtime(lineHold);
            }
        }

        IEnumerator Fade(float target, float duration)
        {
            int id = ++_fadeId;
            overlay.blocksRaycasts = true;
            float start = overlay.alpha;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                if (id != _fadeId) yield break;
                overlay.alpha = Mathf.Lerp(start, target, t / duration);
                yield return null;
            }
            if (id != _fadeId) yield break;
            overlay.alpha = target;
            overlay.blocksRaycasts = target > 0f;
        }
    }
}
