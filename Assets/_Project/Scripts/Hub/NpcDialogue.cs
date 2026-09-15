using Branded.Core;
using Branded.Dialogue;
using Branded.Interaction;
using UnityEngine;

namespace Branded.Hub
{
    // A hub character the player talks to. DialogueStarted locks input, which also clears
    // PlayerInteractor.Current, so no extra "talking" flag is needed here.
    public class NpcDialogue : Interactable
    {
        [SerializeField] DialogueData _dialogue;

        public override bool IsAvailable => _dialogue;

        public override void Interact(PlayerInteractor user)
        {
            if (!_dialogue) return;
            GameEvents.RaiseDialogueRequested(_dialogue);
        }
    }
}
