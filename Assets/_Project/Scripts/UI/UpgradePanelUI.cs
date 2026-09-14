using System.Collections;
using Branded.Core;
using Branded.Meta;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Branded.UI
{
    // Hub upgrade screen for Godot's forge or Puck: one row per upgrade. Esc / E / the close button leaves.
    // Opens on UpgradePanelOpened and raises UpgradePanelClosed when it closes.
    public class UpgradePanelUI : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("panel")] GameObject _panel;
        [SerializeField, FormerlySerializedAs("title")] TMP_Text _titleComponent;
        [SerializeField, FormerlySerializedAs("ashes")] TMP_Text _ashesComponent;
        [SerializeField, FormerlySerializedAs("rows")] UpgradeRow[] _rowComponents;
        [SerializeField, FormerlySerializedAs("closeButton")] UnityEngine.UI.Button _closeButtonComponent;

        InputAction _close;
        bool _closePressed;

        // Awake/OnDestroy rather than OnEnable/OnDisable: the panel may hide this GameObject.
        void Awake()
        {
            _close = new InputAction("Close", InputActionType.Button);
            _close.AddBinding("<Keyboard>/escape");
            _close.AddBinding("<Keyboard>/e");
            _close.performed += OnClose;
            _closeButtonComponent.onClick.AddListener(OnCloseButtonClicked);
            GameEvents.UpgradePanelOpened += OnUpgradePanelOpened;
            _panel.SetActive(false);
        }

        void OnDestroy()
        {
            GameEvents.UpgradePanelOpened -= OnUpgradePanelOpened;
            _close.performed -= OnClose;
            _close.Dispose();
        }

        void OnClose(InputAction.CallbackContext _) => _closePressed = true;
        void OnCloseButtonClicked() => _closePressed = true;

        void OnUpgradePanelOpened(string stationName, UpgradeData[] upgrades)
        {
            // Activate first: a coroutine can't start on an inactive GameObject.
            _panel.SetActive(true);
            StartCoroutine(Show(stationName, upgrades));
        }

        IEnumerator Show(string stationName, UpgradeData[] upgrades)
        {
            _titleComponent.text = stationName;
            for (int i = 0; i < _rowComponents.Length; i++)
            {
                bool used = upgrades != null && i < upgrades.Length && upgrades[i];
                _rowComponents[i].gameObject.SetActive(used);
                if (used) _rowComponents[i].Show(upgrades[i], OnBuyClicked);
            }
            ShowAshes();

            yield return null; // the E press that opened the panel must not close it
            _closePressed = false;
            _close.Enable();
            while (!_closePressed) yield return null;
            _close.Disable();
            _panel.SetActive(false);
            GameEvents.RaiseUpgradePanelClosed();
        }

        void OnBuyClicked(UpgradeData upgrade)
        {
            // A clicked button stays selected, and Space (UI submit) would buy again.
            if (UnityEngine.EventSystems.EventSystem.current) UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            if (!Progress.TryBuy(upgrade)) return;
            ShowAshes();
            foreach (var row in _rowComponents)
                if (row.gameObject.activeSelf) row.Refresh();
        }

        void ShowAshes() => _ashesComponent.text = $"İblis Külü  {Progress.Ashes}";
    }
}
