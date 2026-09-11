using Branded.Player;
using UnityEngine;

namespace Branded.Interaction
{
    // Tracks the nearest usable Interactable in range and uses it on the interact key.
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] PlayerInputReader input;

        public Interactable Current { get; private set; }
        public PlayerInputReader Input => input;

        void Awake()
        {
            if (!input) input = GetComponent<PlayerInputReader>();
        }

        void OnEnable() => input.InteractPressed += Use;

        void OnDisable()
        {
            input.InteractPressed -= Use;
            Current = null;
        }

        // No target while control is locked (dialogue, menus), so the prompt hides too.
        void Update() => Current = input.enabled ? Nearest() : null;

        void Use()
        {
            if (Current && Current.IsAvailable) Current.Interact(this);
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
