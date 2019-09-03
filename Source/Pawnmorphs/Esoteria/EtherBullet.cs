using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Pawnmorph;

namespace EtherGun
{
    public class ThingDef_EtherBullet : ThingDef
    {
        public float AddHediffChance = 0.99f;
        public HediffDef HediffToAdd = null;

        /// <summary>
        /// list of pawnDefs to not allow the hediff to be given to 
        /// </summary>
        public List<ThingDef> raceBlackList; //this feels a bit hacky, targeting info should probably be somewhere more central 


        /// <summary>
        /// check if the given pawn is a valid target to add the hediff to 
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public bool CanAddHediffToPawn(Pawn pawn)
        {
            if (raceBlackList == null) return true;
            return !raceBlackList.Contains(pawn.def);  //Pawn.def is the race ThingDef 
        }

    }
    public class ThingDef_TaggingBullet : ThingDef
    {
    }
}
