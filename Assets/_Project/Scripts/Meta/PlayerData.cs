using System;
using System.Collections.Generic;

namespace Branded.Meta
{
    // Everything that survives death, as written to the save file.
    [Serializable]
    public class PlayerData
    {
        public int ashes;
        public List<UpgradeLevel> upgrades = new();
    }
}
