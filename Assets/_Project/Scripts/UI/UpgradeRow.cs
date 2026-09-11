using System;
using Branded.Meta;
using TMPro;
using UnityEngine;

namespace Branded.UI
{
    // One upgrade in the hub panel: name, effect, level, price and the buy button.
    public class UpgradeRow : MonoBehaviour
    {
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text description;
        [SerializeField] TMP_Text level;
        [SerializeField] TMP_Text cost;
        [SerializeField] UnityEngine.UI.Button buyButton;

        UpgradeData _upgrade;
        Action<UpgradeData> _onBuy;

        void Awake() => buyButton.onClick.AddListener(() => _onBuy?.Invoke(_upgrade));

        public void Show(UpgradeData upgrade, Action<UpgradeData> onBuy)
        {
            _upgrade = upgrade;
            _onBuy = onBuy;
            title.text = upgrade.upgradeName;
            description.text = upgrade.description;
            Refresh();
        }

        public void Refresh()
        {
            int price = Progress.NextCost(_upgrade);
            level.text = $"Seviye {Progress.LevelOf(_upgrade)} / {_upgrade.MaxLevel}";
            cost.text = price < 0 ? "Tamamlandı" : $"{price} Kül";
            buyButton.interactable = price >= 0 && Progress.Ashes >= price;
        }
    }
}
