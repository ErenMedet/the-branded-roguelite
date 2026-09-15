using Branded.Core;
using UnityEngine;

namespace Branded.UI
{
    // Hides the HUD during NPC dialogue: the health bar and ash counter sit under the dialogue portrait.
    public class HudVisibility : MonoBehaviour
    {
        void Awake()
        {
            GameEvents.DialogueStarted += OnDialogueStarted;
            GameEvents.DialogueEnded += OnDialogueEnded;
        }

        void OnDestroy()
        {
            GameEvents.DialogueStarted -= OnDialogueStarted;
            GameEvents.DialogueEnded -= OnDialogueEnded;
        }

        void OnDialogueStarted() => gameObject.SetActive(false);
        void OnDialogueEnded() => gameObject.SetActive(true);
    }
}
