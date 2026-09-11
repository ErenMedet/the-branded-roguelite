using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Branded.Dialogue
{
    // Hades-style dialogue on the 2D UI canvas, independent of the 3D game. Lines type out; E / Space / click
    // first completes the line, then moves to the next one.
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] UnityEngine.UI.Image portrait;
        [SerializeField] TMP_Text speakerName;
        [SerializeField] TMP_Text body;
        [SerializeField] GameObject continueHint;
        [SerializeField] AudioSource voice;
        [SerializeField] float charactersPerSecond = 45f;

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
            panel.SetActive(false);
        }

        void OnDestroy()
        {
            _advance.performed -= OnAdvance;
            _advance.Dispose();
        }

        void OnAdvance(InputAction.CallbackContext _) => _advancePressed = true;

        public IEnumerator Play(DialogueData data)
        {
            if (!data || data.lines == null || data.lines.Length == 0) yield break;

            IsOpen = true;
            panel.SetActive(true);
            speakerName.text = data.speakerName;
            portrait.sprite = data.speakerPortrait;
            portrait.enabled = data.speakerPortrait;
            _advance.Enable();

            foreach (string line in data.lines)
            {
                yield return TypeLine(line, data.voiceMumble);
                continueHint.SetActive(true);
                while (!_advancePressed) yield return null;
                _advancePressed = false;
            }

            _advance.Disable();
            panel.SetActive(false);
            IsOpen = false;
        }

        IEnumerator TypeLine(string line, AudioClip mumble)
        {
            continueHint.SetActive(false);
            body.text = line;
            body.maxVisibleCharacters = 0;
            body.ForceMeshUpdate();
            int total = body.textInfo.characterCount;
            if (voice && mumble) voice.PlayOneShot(mumble);

            _advancePressed = false; // a press from before this line doesn't skip it
            float shown = 0f;
            while (shown < total && !_advancePressed)
            {
                shown += charactersPerSecond * Time.unscaledDeltaTime;
                body.maxVisibleCharacters = Mathf.FloorToInt(shown);
                yield return null;
            }
            _advancePressed = false;
            body.maxVisibleCharacters = total;
        }
    }
}
