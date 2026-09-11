using System;
using System.IO;
using UnityEngine;

namespace Branded.Meta
{
    // Permanent progress: demon ash and bought upgrade levels, kept in PlayerData as JSON on disk.
    public static class Progress
    {
        static PlayerData _data;

        public static event Action<int> AshesChanged;
        public static event Action UpgradesChanged;

        static string FilePath => Path.Combine(Application.persistentDataPath, "save.json");
        static PlayerData Data => _data ??= Load();

        public static int Ashes => Data.ashes;

        public static void AddAshes(int amount)
        {
            if (amount <= 0) return;
            Data.ashes += amount;
            AshesChanged?.Invoke(Data.ashes);
        }

        public static int LevelOf(UpgradeData upgrade)
        {
            foreach (var entry in Data.upgrades)
                if (entry.id == upgrade.id) return entry.level;
            return 0;
        }

        // Price of the next level, or -1 once the upgrade is maxed.
        public static int NextCost(UpgradeData upgrade)
        {
            int level = LevelOf(upgrade);
            return level < upgrade.MaxLevel ? upgrade.costs[level] : -1;
        }

        public static bool TryBuy(UpgradeData upgrade)
        {
            int cost = NextCost(upgrade);
            if (cost < 0 || Data.ashes < cost) return false;

            Data.ashes -= cost;
            SetLevel(upgrade.id, LevelOf(upgrade) + 1);
            Save();
            AshesChanged?.Invoke(Data.ashes);
            UpgradesChanged?.Invoke();
            return true;
        }

        public static void Save()
        {
            try { File.WriteAllText(FilePath, JsonUtility.ToJson(Data, true)); }
            catch (Exception e) { Debug.LogError($"Save failed: {e.Message}"); }
        }

        static PlayerData Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    return JsonUtility.FromJson<PlayerData>(File.ReadAllText(FilePath)) ?? new PlayerData();
            }
            catch (Exception e) { Debug.LogError($"Save file unreadable, starting fresh: {e.Message}"); }
            return new PlayerData();
        }

        static void SetLevel(string id, int level)
        {
            foreach (var entry in Data.upgrades)
                if (entry.id == id)
                {
                    entry.level = level;
                    return;
                }
            Data.upgrades.Add(new UpgradeLevel { id = id, level = level });
        }

        // Domain reload is off, so statics outlive a play session: start every session from the file.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _data = null;
            AshesChanged = null;
            UpgradesChanged = null;
            Application.quitting -= Save;
            Application.quitting += Save;
        }
    }
}
