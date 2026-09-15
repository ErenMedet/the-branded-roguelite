using System.Collections;
using Branded.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Branded.Dialogue
{
    // Hades-style dialogue on the 2D UI canvas, independent of the 3D game. Lines type out; E / Space / click
    // first completes the line, then moves to the next one.
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("panel")] GameObject _panel;
        [SerializeField, FormerlySerializedAs("portrait")] UnityEngine.UI.Image _portraitComponent;
        [SerializeField, FormerlySerializedAs("speakerName")] TMP_Text _speakerNameComponent;
        [SerializeField, FormerlySerializedAs("body")] TMP_Text _bodyComponent;
        [SerializeField, FormerlySerializedAs("continueHint")] GameObject _continueHint;
        [SerializeField, FormerlySerializedAs("voice")] AudioSource _voiceComponent;
        [SerializeField, FormerlySerializedAs("charactersPerSecond")] float _charactersPerSecond = 45f;

        public bool IsOpen { get; private set; }

        InputAction _advance;
        bool _advancePressed;

        void Awake()
        {
            _advance = new InputAction("Advance", InputActionType.Button);
            _advance.AddBinding("<Keyboard>/e");
            _advance.AddBinding("<Keyboard>/space");
            _advance.AddBinding("<Mouse>/leftButton");
            _advance.performed += OnAdvance;
            _panel.SetActive(false);
        }

        void OnDestroy()
        {
            _advance.performed -= OnAdvance;
            _advance.Dispose();
        }

        void OnEnable() => GameEvents.DialogueRequested += OnDialogueRequested;
        void OnDisable() => GameEvents.DialogueRequested -= OnDialogueRequested;

        void OnAdvance(InputAction.CallbackContext _) => _advancePressed = true;

        // NPC talk: the camp locks input itself, so only requested dialogues raise Started / Ended.
        void OnDialogueRequested(DialogueData data)
        {
            if (IsOpen) return;
            StartCoroutine(PlayRequested(data));
        }

        IEnumerator PlayRequested(DialogueData data)
        {
            GameEvents.RaiseDialogueStarted();
            yield return Play(data);
            GameEvents.RaiseDialogueEnded();
        }

        public IEnumerator Play(DialogueData data)
        {
            if (!data || data.Lines == null || data.Lines.Length == 0) yield break;

            IsOpen = true;
            _panel.SetActive(true);
            _speakerNameComponent.text = data.SpeakerName;
            _portraitComponent.sprite = data.SpeakerPortrait;
            _portraitComponent.enabled = data.SpeakerPortrait;
            _bodyComponent.text = string.Empty;
            yield return null; // the interact press that opened the dialogue must not also skip its first line
            _advance.Enable();

            foreach (string line in data.Lines)
            {
                yield return TypeLine(line, data.VoiceMumble);
                _continueHint.SetActive(true);
                while (!_advancePressed) yield return null;
                _advancePressed = false;
            }

            _advance.Disable();
            _panel.SetActive(false);
            IsOpen = false;
        }

        IEnumerator TypeLine(string line, AudioClip mumble)
        {
            _continueHint.SetActive(false);
            _bodyComponent.text = line;
            _bodyComponent.maxVisibleCharacters = 0;
            _bodyComponent.ForceMeshUpdate();
            int total = _bodyComponent.textInfo.characterCount;
            if (_voiceComponent && mumble) _voiceComponent.PlayOneShot(mumble);

            _advancePressed = false; // a press from before this line doesn't skip it
            float shown = 0f;
            while (shown < total && !_advancePressed)
            {
                shown += _charactersPerSecond * Time.unscaledDeltaTime;
                _bodyComponent.maxVisibleCharacters = Mathf.FloorToInt(shown);
                yield return null;
            }
            _advancePressed = false;
            _bodyComponent.maxVisibleCharacters = total;
        }
    }
}
