using Branded.Player;
using UnityEngine;

namespace Branded.UI
{
    // Souls-style: no reticle while the sword is out, one appears while the crossbow is aimed.
    // A broken arm has nothing to aim, so it stays hidden until the repair bench fixes it.
    public class CrosshairUI : MonoBehaviour
    {
        [SerializeField] PlayerInputReader _inputComponent;  // empty = the Player-tagged object
        [SerializeField] PlayerCrossbow _crossbowComponent;
        [SerializeField] GameObject _reticle;

        bool _isVisible;

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

            // The reticle is authored visible in the scene, so it is hidden outright rather than through SetVisible.
            _isVisible = false;
            if (_reticle) _reticle.SetActive(false);
        }

        // The fire button is a held state with no event of its own, so it is read each frame.
        void Update()
        {
            if (!_inputComponent) return;
            SetVisible(_inputComponent.IsFireHeld && _crossbowComponent && !_crossbowComponent.IsBroken);
        }

        void SetVisible(bool visible)
        {
            if (_isVisible == visible || !_reticle) return;
            _isVisible = visible;
            _reticle.SetActive(visible);
        }
    }
}
