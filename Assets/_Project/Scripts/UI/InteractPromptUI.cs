using Branded.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.UI
{
    // "[E] ..." floating over whatever the player can use right now.
    public class InteractPromptUI : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("interactor")] PlayerInteractor _interactorComponent; // empty = the Player-tagged object
        [SerializeField, FormerlySerializedAs("prompt")] RectTransform _promptComponent;
        [SerializeField, FormerlySerializedAs("label")] TMP_Text _labelComponent;

        Camera _cameraComponent;
        Interactable _shownComponent;

        void Awake()
        {
            _promptComponent.gameObject.SetActive(false);
            if (_interactorComponent) return;
            var player = GameObject.FindWithTag("Player");
            if (player) _interactorComponent = player.GetComponent<PlayerInteractor>();
        }

        void LateUpdate()
        {
            if (!_cameraComponent) _cameraComponent = Camera.main;
            Interactable target = _interactorComponent && _cameraComponent ? _interactorComponent.Current : null;

            if (target != _shownComponent)
            {
                _shownComponent = target;
                _promptComponent.gameObject.SetActive(target);
                if (target) _labelComponent.text = $"[E] {target.Prompt}";
            }
            if (!target) return;
            _promptComponent.position = _cameraComponent.WorldToScreenPoint(target.PromptPosition);
        }
    }
}
