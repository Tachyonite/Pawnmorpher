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
        public float AddHediffChance = 0.7f;
        public HediffDef HediffToAdd;

        public CompProperties_EtherExplosive()
        {
            compClass = typeof(CompEtherExplosive);
        }
    }
}
