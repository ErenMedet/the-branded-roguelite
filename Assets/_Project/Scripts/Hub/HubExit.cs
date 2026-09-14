using Branded.Core;
using Branded.Interaction;
using Branded.Meta;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Hub
{
    // The mountain path down from Godot's mine: fade out and start a new run.
    // HubExited locks the player's input and makes ScreenTransition fade out; ScreenFadedOut loads the run.
    public class HubExit : Interactable
    {
        [SerializeField, FormerlySerializedAs("fadeDuration")] float _fadeDuration = 0.8f;

        bool _leaving;

        public override bool IsAvailable => !_leaving;

        protected override void OnEnable()
        {
            base.OnEnable();
            GameEvents.ScreenFadedOut += OnScreenFadedOut;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            GameEvents.ScreenFadedOut -= OnScreenFadedOut;
        }

        public override void Interact(PlayerInteractor user)
        {
            if (_leaving) return;
            _leaving = true;
            GameEvents.RaiseHubExited(_fadeDuration);
        }

        void OnScreenFadedOut()
        {
            if (!_leaving) return;
            SceneFlow.Load(SceneFlow.Run);
        }
    }
}
