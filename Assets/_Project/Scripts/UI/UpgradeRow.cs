using Branded.Meta;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Branded.UI
{
    // One upgrade in the hub panel: name, effect, level, price and the buy button.
    public class UpgradeRow : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("title")] TMP_Text _titleComponent;
        [SerializeField, FormerlySerializedAs("description")] TMP_Text _descriptionComponent;
        [SerializeField, FormerlySerializedAs("level")] TMP_Text _levelComponent;
        [SerializeField, FormerlySerializedAs("cost")] TMP_Text _costComponent;
        [SerializeField, FormerlySerializedAs("buyButton")] UnityEngine.UI.Button _buyButtonComponent;

        UpgradeData _upgrade;
        UnityAction<UpgradeData> _buyClicked;

        void Awake() => _buyButtonComponent.onClick.AddListener(OnBuyButtonClicked);

        void OnBuyButtonClicked() => _buyClicked?.Invoke(_upgrade);

        public void Show(UpgradeData upgrade, UnityAction<UpgradeData> buyClicked)
        {
            _upgrade = upgrade;
            _buyClicked = buyClicked;
            _titleComponent.text = upgrade.UpgradeName;
            _descriptionComponent.text = upgrade.Description;
            Refresh();
        }

        public void Refresh()
        {
            int price = Progress.NextCost(_upgrade);
            _levelComponent.text = $"Seviye {Progress.LevelOf(_upgrade)} / {_upgrade.MaxLevel}";
            _costComponent.text = price < 0 ? "Tamamlandı" : $"{price} Kül";
            _buyButtonComponent.interactable = price >= 0 && Progress.Ashes >= price;
        }
    }
}
