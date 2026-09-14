using Branded.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Interaction
{
    // Tracks the nearest usable Interactable in range and uses it on the interact key.
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("input")] PlayerInputReader _inputComponent;

        public Interactable Current { get; private set; }

        void Awake()
        {
            if (!_inputComponent) _inputComponent = GetComponent<PlayerInputReader>();
        }

        void OnEnable() => _inputComponent.InteractPressed += OnInteractPressed;

        void OnDisable()
        {
            _inputComponent.InteractPressed -= OnInteractPressed;
            Current = null;
        }

        // No target while input is locked (camp, menus, leaving), so the prompt hides too.
        void Update() => Current = _inputComponent.isActiveAndEnabled && !_inputComponent.IsLocked ? Nearest() : null;

        void OnInteractPressed()
        {
            if (!Current || !Current.IsAvailable) return;
            Current.Interact(this);
        }

        Interactable Nearest()
        {
            Interactable best = null;
            float bestRange = float.MaxValue;
            foreach (var candidate in Interactable.All)
            {
                if (!candidate.IsAvailable) continue;
                float range = candidate.RangeTo(transform.position);
                if (range < 0f || range >= bestRange) continue;
                best = candidate;
                bestRange = range;
            }
            return best;
        }
    }
}
