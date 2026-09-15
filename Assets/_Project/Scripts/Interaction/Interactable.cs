using System.Collections.Generic;
using Branded.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Interaction
{
    // Anything used with the interact key when the player is close. PlayerInteractor picks the nearest one.
    public abstract class Interactable : MonoBehaviour
    {
        static readonly List<Interactable> Instances = new();
        public static IReadOnlyList<Interactable> All => Instances;

        [SerializeField, FormerlySerializedAs("interactRadius")] float _interactRadius = 2.5f;
        [SerializeField, FormerlySerializedAs("prompt")] string _prompt = "Kullan";
        [SerializeField, FormerlySerializedAs("promptHeight")] float _promptHeight = 1.6f;

        public string Prompt => _prompt;
        public Vector3 PromptPosition => transform.position + Vector3.up * _promptHeight;
        public virtual bool IsAvailable => true;

        protected virtual void OnEnable() => Instances.Add(this);
        protected virtual void OnDisable() => Instances.Remove(this);

        // Squared flat distance, or -1 when out of range.
        public float RangeTo(Vector3 position)
        {
            float sqr = FlatMath.Flat(position - transform.position).sqrMagnitude;
            return sqr <= _interactRadius * _interactRadius ? sqr : -1f;
        }

        public abstract void Interact(PlayerInteractor user);

        // Domain reload is off, so entries left over from the last play session would otherwise stay in the list.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Instances.Clear();
    }
}
