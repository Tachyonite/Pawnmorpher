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
    }
    public class ThingDef_TaggingBullet : ThingDef
    {
    }
}
