using Branded.Player;
using TMPro;
using UnityEngine;

namespace Branded.UI
{
    // HUD crossbow bar, opposite the health bar: bolts left, a refill sweep while reloading, and the broken state after the cannon.
    public class PlayerAmmoBar : MonoBehaviour
    {
        [SerializeField] PlayerCrossbow _crossbowComponent; // empty = the Player-tagged object
        [SerializeField] RectTransform _fillComponent;      // stretched; its right anchor follows the value
        [SerializeField] TMP_Text _labelComponent;
        [SerializeField] string _reloadingText = "Dolduruluyor...";
        [SerializeField] string _brokenText = "Kırık";

        bool _isReloading;

        void Awake()
        {
            if (_crossbowComponent) return;
            var player = GameObject.FindWithTag("Player");
            if (player) _crossbowComponent = player.GetComponent<PlayerCrossbow>();
        }

        void OnEnable()
        {
            if (!_crossbowComponent) return;
            _crossbowComponent.BoltsChanged += OnBoltsChanged;
            _crossbowComponent.ReloadStarted += OnReloadStarted;
            _crossbowComponent.Broke += OnBroke;
            Refresh(); // catch up on anything missed while hidden
        }

        void OnDisable()
        {
            if (!_crossbowComponent) return;
            _crossbowComponent.BoltsChanged -= OnBoltsChanged;
            _crossbowComponent.ReloadStarted -= OnReloadStarted;
            _crossbowComponent.Broke -= OnBroke;
        }

        // Again in Start: OnEnable can run before the crossbow's Awake fills the magazine.
        void Start()
        {
            if (!_crossbowComponent) return;
            Refresh();
        }

        // Follows the crossbow's own timer, so the sweep ends exactly when the bolts come back.
        void Update()
        {
            if (!_isReloading) return;
            SetBar(_crossbowComponent.ReloadProgress);
        }

        void Refresh()
        {
            if (_crossbowComponent.IsBroken) OnBroke();
            else if (_crossbowComponent.IsReloading) OnReloadStarted();
            else OnBoltsChanged(_crossbowComponent.BoltsLeft, _crossbowComponent.MagazineSize);
        }

        void OnBoltsChanged(int left, int magazine)
        {
            _isReloading = false;
            SetBar(magazine > 0 ? (float)left / magazine : 0f);
            SetLabel($"{left} / {magazine}");
        }

        void OnReloadStarted()
        {
            _isReloading = true;
            SetBar(_crossbowComponent.ReloadProgress);
            SetLabel(_reloadingText);
        }

        void OnBroke()
        {
            _isReloading = false;
            SetBar(0f);
            SetLabel(_brokenText);
        }

        void SetBar(float value) => _fillComponent.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);

        void SetLabel(string text)
        {
            if (!_labelComponent) return;
            _labelComponent.text = text;
        }
    }
}
