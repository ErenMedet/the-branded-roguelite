using System;

namespace Branded.Meta
{
    // Save-file DTO: plain public fields, because JsonUtility writes these names to disk.
    [Serializable]
    public class UpgradeLevel
    {
        public string id;
        public int level;
    }
}
