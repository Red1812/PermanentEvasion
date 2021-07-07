using System.Collections.Generic;
using BattleTech;

namespace PermanentEvasion

{
    public class ModSettings
    {
        public bool Debug = false;
        public string modDirectory;

        public int PercentageToKeepPips;
        public int AcePilotBonusPercentage;
        public int LightKeepPipsCount;
        public int MediumKeepPipsCount;
        public int HeavyKeepPipsCount;
        public int AssaultKeepPipsCount;
        public int AcePilotBonusPips;
        public bool PilotSkillToKeepPips;
        public int PerSkillPointToKeepPips;
        public int AcePilotPointToKeepPips;
        public int MaxTotalChanceTokeepPips;
        public float MinDamageForEvasionStrip;
        public bool AllowHitStrip;
        public int Movement210KeepPipsCount;
        public int Movement190KeepPipsCount;
        public int Movement165KeepPipsCount;
        public int Movement140KeepPipsCount;
        public int Movement120KeepPipsCount;
        public int Movement95KeepPipsCount;
        public bool UseMovement;
        public int JumpBonusPip;
        public bool LinkedToAcePilot;
        public bool UseQuirks;
    }
}
