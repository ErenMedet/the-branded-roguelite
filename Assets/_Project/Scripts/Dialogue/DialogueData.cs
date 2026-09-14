using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Dialogue
{
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Roguelite/Dialogue")]
    public class DialogueData : ScriptableObject
    {
        [field: SerializeField, FormerlySerializedAs("speakerName")] public string SpeakerName { get; private set; } // e.g. "Godo", "Kafatası Şövalyesi"
        [field: SerializeField, FormerlySerializedAs("speakerPortrait")] public Sprite SpeakerPortrait { get; private set; }
        [field: SerializeField, FormerlySerializedAs("voiceMumble")] public AudioClip VoiceMumble { get; private set; }
        [field: SerializeField, TextArea(3, 5), FormerlySerializedAs("lines")] public string[] Lines { get; private set; }
    }
}
