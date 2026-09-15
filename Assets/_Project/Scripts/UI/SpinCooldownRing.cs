using Branded.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Branded.UI
{
    // World-space ring above the player: empties when the spin starts and fills back as its cooldown runs out.
    public class SpinCooldownRing : MonoBehaviour
    {
        [SerializeField] PlayerCombat _combatComponent; // empty = the parent's
        [SerializeField] Image _fillComponent;          // Filled, Radial 360

        Transform _cameraTransformComponent;

        void Awake()
        {
            if (!_combatComponent) _combatComponent = GetComponentInParent<PlayerCombat>();
        }

        // Reads the timer after PlayerCombat's Update, and faces the camera after it has moved this frame.
        void LateUpdate()
        {
            if (!_combatComponent || !_fillComponent) return;
            _fillComponent.fillAmount = _combatComponent.SpinCooldownProgress;

            if (!_cameraTransformComponent && Camera.main) _cameraTransformComponent = Camera.main.transform;
            if (!_cameraTransformComponent) return;
            transform.rotation = _cameraTransformComponent.rotation;
        }
    }
}
