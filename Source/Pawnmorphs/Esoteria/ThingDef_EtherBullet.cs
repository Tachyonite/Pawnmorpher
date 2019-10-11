using System.Collections.Generic;
using Verse;

namespace EtherGun
{
    public class ThingDef_EtherBullet : ThingDef
    {
        public float AddHediffChance = 0.99f;
        public HediffDef HediffToAdd = null;

        /// <summary> List of pawnDefs to not allow the hediff to be given to. </summary>
        public List<ThingDef> raceBlackList; // This feels a bit hacky, targeting info should probably be somewhere more central.

        /// <summary> Check if the given pawn is a valid target to add the hediff to.</summary>
        public bool CanAddHediffToPawn(Pawn pawn)
        {
            if (raceBlackList == null) return true;
            return !raceBlackList.Contains(pawn.def);  // Pawn.def is the race ThingDef 
        }
    }
}
