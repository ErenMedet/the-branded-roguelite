using UnityEngine;

namespace Branded.Dialogue
{
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Roguelite/Dialogue")]
    public class DialogueData : ScriptableObject
    {
        public string speakerName;         // e.g. "Godo", "Kafatası Şövalyesi"
        public Sprite speakerPortrait;
        public AudioClip voiceMumble;
        [TextArea(3, 5)]
        public string[] lines;
    }
}
