using Branded.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Branded.UI
{
    // Souls-style: no reticle while the sword is out, one appears while the crossbow is aimed.
    // A broken arm has nothing to aim, so it stays hidden until the repair bench fixes it.
    // It never leaves screen centre: PlayerAim takes its aim point off the centre ray at the muzzle's own
    // height, so the bolt flies through that exact spot. Chasing the impact point instead would be just as
    // true but would jump every time the shot crossed an obstacle, which reads as a twitch. What is in the
    // way is reported by tinting the marks rather than by moving them.
    public class CrosshairUI : MonoBehaviour
    {
        static readonly Color ClearColour = new Color(1f, 1f, 1f, 0.5f);
        static readonly Color BlockedColour = new Color(1f, 1f, 1f, 0.22f);
        static readonly Color TargetColour = new Color(0.9f, 0.2f, 0.2f, 0.95f);

        [SerializeField] PlayerInputReader _inputComponent;  // empty = the Player-tagged object
        [SerializeField] PlayerCrossbow _crossbowComponent;
        [SerializeField] GameObject _reticle;

        bool _isVisible;
        Graphic[] _markComponents;
        EAimState _state;
        bool _stateKnown;

        void Awake()
        {
            if (!_inputComponent || !_crossbowComponent)
            {
                var player = GameObject.FindWithTag("Player");
                if (player)
                {
                    if (!_inputComponent) _inputComponent = player.GetComponent<PlayerInputReader>();
                    if (!_crossbowComponent) _crossbowComponent = player.GetComponent<PlayerCrossbow>();
                }
            }

            if (_reticle) _markComponents = _reticle.GetComponentsInChildren<Graphic>(true);

            // The reticle is authored visible in the scene, so it is hidden outright rather than through SetVisible.
            _isVisible = false;
            if (_reticle) _reticle.SetActive(false);
        }

        // The fire button is a held state with no event of its own, so it is read each frame. This runs in
        // LateUpdate so the aim it reports is the one PlayerAim settled on this frame, not last frame's.
        void LateUpdate()
        {
            if (!_inputComponent) return;
            SetVisible(_inputComponent.IsFireHeld && _crossbowComponent && !_crossbowComponent.IsBroken);
            if (_isVisible) ApplyState(_crossbowComponent.PredictAim());
        }

        // Only a change is written: tinting a Graphic every frame would rebuild the canvas for nothing.
        void ApplyState(EAimState state)
        {
            if (_stateKnown && _state == state) return;
            _state = state;
            _stateKnown = true;
            if (_markComponents == null) return;

            Color colour = state == EAimState.Target ? TargetColour
                : state == EAimState.Blocked ? BlockedColour
                : ClearColour;
            foreach (var mark in _markComponents) mark.color = colour;
        }

        void SetVisible(bool visible)
        {
            if (_isVisible == visible || !_reticle) return;
            _isVisible = visible;
            _reticle.SetActive(visible);
            // A reticle coming back has no tint of its own, so the next state is always written out.
            if (!visible) _stateKnown = false;
        }
    }
}
