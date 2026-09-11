using System.Collections;
using Branded.Interaction;
using Branded.Meta;
using Branded.UI;
using UnityEngine;

namespace Branded.Hub
{
    // Godo's forge or Puck: opens the upgrade panel with this station's upgrades. The player can't move meanwhile.
    public class UpgradeStation : Interactable
    {
        [SerializeField] string stationName;
        [SerializeField] UpgradeData[] upgrades;

        bool _open;

        public override bool IsAvailable => !_open;

        public override void Interact(PlayerInteractor user)
        {
            var panel = FindAnyObjectByType<UpgradePanelUI>();
            if (panel && !_open) StartCoroutine(Open(panel, user));
        }

        IEnumerator Open(UpgradePanelUI panel, PlayerInteractor user)
        {
            _open = true;
            user.Input.enabled = false;
            yield return panel.Show(stationName, upgrades);
            user.Input.enabled = true;
            _open = false;
        }
    }
}
