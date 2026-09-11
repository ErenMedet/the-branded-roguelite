using Branded.Interaction;
using TMPro;
using UnityEngine;

namespace Branded.UI
{
    // "[E] ..." floating over whatever the player can use right now.
    public class InteractPromptUI : MonoBehaviour
    {
        [SerializeField] PlayerInteractor interactor; // empty = the Player-tagged object
        [SerializeField] RectTransform prompt;
        [SerializeField] TMP_Text label;

        Camera _camera;
        Interactable _shown;

        void Awake()
        {
            if (!interactor)
            {
                var player = GameObject.FindWithTag("Player");
                if (player) interactor = player.GetComponent<PlayerInteractor>();
            }
            prompt.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (!_camera) _camera = Camera.main;
            Interactable target = interactor && _camera ? interactor.Current : null;

            if (target != _shown)
            {
                _shown = target;
                prompt.gameObject.SetActive(target);
                if (target) label.text = $"[E] {target.Prompt}";
            }
            if (target) prompt.position = _camera.WorldToScreenPoint(target.PromptPosition);
        }
    }
}
