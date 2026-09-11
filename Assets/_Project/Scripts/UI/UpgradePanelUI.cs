using System.Collections;
using Branded.Meta;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Branded.UI
{
    // Hub upgrade screen for Godo's forge or Puck: one row per upgrade. Esc / E / the close button leaves.
    public class UpgradePanelUI : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text ashes;
        [SerializeField] UpgradeRow[] rows;
        [SerializeField] UnityEngine.UI.Button closeButton;

        InputAction _close;
        bool _closePressed;

        void Awake()
        {
            _close = new InputAction("Close", InputActionType.Button);
            _close.AddBinding("<Keyboard>/escape");
            _close.AddBinding("<Keyboard>/e");
            _close.performed += OnClose;
            closeButton.onClick.AddListener(() => _closePressed = true);
            panel.SetActive(false);
        }

        void OnDestroy()
        {
            _close.performed -= OnClose;
            _close.Dispose();
        }

        void OnClose(InputAction.CallbackContext _) => _closePressed = true;

        public IEnumerator Show(string stationName, UpgradeData[] upgrades)
        {
            panel.SetActive(true);
            title.text = stationName;
            for (int i = 0; i < rows.Length; i++)
            {
                bool used = upgrades != null && i < upgrades.Length && upgrades[i];
                rows[i].gameObject.SetActive(used);
                if (used) rows[i].Show(upgrades[i], Buy);
            }
            ShowAshes();

            yield return null; // the E press that opened the panel must not close it
            _closePressed = false;
            _close.Enable();
            while (!_closePressed) yield return null;
            _close.Disable();
            panel.SetActive(false);
        }

        void Buy(UpgradeData upgrade)
        {
            // A clicked button stays selected, and Space (UI submit) would buy again.
            if (UnityEngine.EventSystems.EventSystem.current) UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            if (!Progress.TryBuy(upgrade)) return;
            ShowAshes();
            foreach (var row in rows)
                if (row.gameObject.activeSelf) row.Refresh();
        }

        void ShowAshes() => ashes.text = $"İblis Külü  {Progress.Ashes}";
    }
}
