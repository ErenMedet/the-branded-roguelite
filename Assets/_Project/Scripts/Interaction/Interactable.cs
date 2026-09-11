using System.Collections.Generic;
using UnityEngine;

namespace Branded.Interaction
{
    // Anything used with the interact key when the player is close. PlayerInteractor picks the nearest one.
    public abstract class Interactable : MonoBehaviour
    {
        static readonly List<Interactable> _all = new();
        public static IReadOnlyList<Interactable> All => _all;

        [SerializeField] float interactRadius = 2.5f;
        [SerializeField] string prompt = "Kullan";
        [SerializeField] float promptHeight = 1.6f;

        public string Prompt => prompt;
        public Vector3 PromptPosition => transform.position + Vector3.up * promptHeight;
        public virtual bool IsAvailable => true;

        protected virtual void OnEnable() => _all.Add(this);
        protected virtual void OnDisable() => _all.Remove(this);

        // Squared flat distance, or -1 when out of range.
        public float RangeTo(Vector3 position)
        {
            Vector3 offset = position - transform.position;
            offset.y = 0f;
            float sqr = offset.sqrMagnitude;
            return sqr <= interactRadius * interactRadius ? sqr : -1f;
        }

        public abstract void Interact(PlayerInteractor user);
    }
}
