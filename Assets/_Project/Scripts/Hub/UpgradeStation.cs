using Branded.Core;
using Branded.Interaction;
using Branded.Meta;
using UnityEngine;
using UnityEngine.Serialization;

namespace Branded.Hub
{
    // Godot's forge or Puck: opens the upgrade panel with this station's upgrades.
    // UpgradePanelOpened shows the panel and locks the player's input until UpgradePanelClosed.
    public class UpgradeStation : Interactable
    {
        [SerializeField, FormerlySerializedAs("stationName")] string _stationName;
        [SerializeField, FormerlySerializedAs("upgrades")] UpgradeData[] _upgrades;

        bool _open;

        public override bool IsAvailable => !_open;

        protected override void OnEnable()
        {
            base.OnEnable();
            GameEvents.UpgradePanelClosed += OnUpgradePanelClosed;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            GameEvents.UpgradePanelClosed -= OnUpgradePanelClosed;
        }

        public override void Interact(PlayerInteractor user)
        {
            if (_open) return;
            _open = true;
            GameEvents.RaiseUpgradePanelOpened(_stationName, _upgrades);
        }

        void OnUpgradePanelClosed() => _open = false;
    }
}
