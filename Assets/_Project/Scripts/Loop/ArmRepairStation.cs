using Branded.Core;
using Branded.Interaction;

namespace Branded.Loop
{
    // Small workbench beside the morning campfire: fixes the crossbow and reloads the arm cannon, once per camp.
    // It is a child of the campfire prefab, so it appears and disappears with the campfire.
    public class ArmRepairStation : Interactable
    {
        bool _used;

        public override bool IsAvailable => !_used;

        public override void Interact(PlayerInteractor user)
        {
            if (_used) return;
            _used = true;
            GameEvents.RaiseArmRepaired();
        }
    }
}
