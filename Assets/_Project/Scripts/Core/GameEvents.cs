using Branded.Meta;
using UnityEngine;
using UnityEngine.Events;

namespace Branded.Core
{
    // Observer hub: whoever makes something happen raises it here, and whoever cares subscribes.
    // Senders never look up their listeners, so removing a listener can't break a sender.
    public static class GameEvents
    {
        public static event UnityAction<int> AshesChanged;
        public static event UnityAction UpgradesChanged;

        public static event UnityAction CampStarted;
        public static event UnityAction CampEnded;
        public static event UnityAction CampfireRested;
        public static event UnityAction ArmRepaired;

        public static event UnityAction<string, UpgradeData[]> UpgradePanelOpened;
        public static event UnityAction UpgradePanelClosed;

        public static event UnityAction<float> HubExited; // (fade duration)
        public static event UnityAction ScreenFadedOut;

        public static event UnityAction PlayerDied;
        public static event UnityAction DeathScreenFinished;

        public static void RaiseAshesChanged(int ashes) => AshesChanged?.Invoke(ashes);
        public static void RaiseUpgradesChanged() => UpgradesChanged?.Invoke();

        public static void RaiseCampStarted() => CampStarted?.Invoke();
        public static void RaiseCampEnded() => CampEnded?.Invoke();
        public static void RaiseCampfireRested() => CampfireRested?.Invoke();
        public static void RaiseArmRepaired() => ArmRepaired?.Invoke();

        public static void RaiseUpgradePanelOpened(string stationName, UpgradeData[] upgrades) => UpgradePanelOpened?.Invoke(stationName, upgrades);
        public static void RaiseUpgradePanelClosed() => UpgradePanelClosed?.Invoke();

        public static void RaiseHubExited(float fadeDuration) => HubExited?.Invoke(fadeDuration);
        public static void RaiseScreenFadedOut() => ScreenFadedOut?.Invoke();

        public static void RaisePlayerDied() => PlayerDied?.Invoke();
        public static void RaiseDeathScreenFinished() => DeathScreenFinished?.Invoke();

        // Domain reload is off, so listeners from the last play session would otherwise still be attached.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetEvents()
        {
            AshesChanged = null;
            UpgradesChanged = null;
            CampStarted = null;
            CampEnded = null;
            CampfireRested = null;
            ArmRepaired = null;
            UpgradePanelOpened = null;
            UpgradePanelClosed = null;
            HubExited = null;
            ScreenFadedOut = null;
            PlayerDied = null;
            DeathScreenFinished = null;
        }
    }
}
