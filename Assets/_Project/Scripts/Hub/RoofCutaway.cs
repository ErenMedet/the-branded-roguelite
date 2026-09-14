using UnityEngine;
using UnityEngine.Rendering;

namespace Branded.Hub
{
    // Hides a roof while the player is behind or under it, so the isometric camera can see them.
    // Roofs switch to shadows-only instead of turning off, so the area underneath stays in shade.
    [RequireComponent(typeof(Collider))]
    public class RoofCutaway : MonoBehaviour
    {
        [SerializeField] Renderer[] _roofRendererComponents;

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            SetHidden(true);
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            SetHidden(false);
        }

        void SetHidden(bool hidden)
        {
            ShadowCastingMode mode = hidden ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
            foreach (Renderer roof in _roofRendererComponents)
                roof.shadowCastingMode = mode;
        }
    }
}
