using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace EtherGun
{
    public class CompProperties_EtherExplosive : CompProperties_Explosive
    {
        public CompProperties_EtherExplosive()
        {
            this.compClass = typeof(CompEtherExplosive);
        }

        public float AddHediffChance = 0.99f;
        public HediffDef HediffToAdd = null;
    }
}
