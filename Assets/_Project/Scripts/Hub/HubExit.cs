using System.Collections;
using Branded.Interaction;
using Branded.Meta;
using Branded.UI;
using UnityEngine;

namespace Branded.Hub
{
    // The cave mouth: fade out and start a new run.
    public class HubExit : Interactable
    {
        [SerializeField] float fadeDuration = 0.8f;

        bool _leaving;

        public override bool IsAvailable => !_leaving;

        public override void Interact(PlayerInteractor user)
        {
            if (_leaving) return;
            _leaving = true;
            user.Input.enabled = false;
            StartCoroutine(Leave());
        }

        IEnumerator Leave()
        {
            var screen = FindAnyObjectByType<ScreenTransition>();
            if (screen) yield return screen.FadeOut(fadeDuration);
            SceneFlow.Load(SceneFlow.Run);
        }
    }
}
