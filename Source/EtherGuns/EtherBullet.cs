using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace EtherGun
{
    public class ThingDef_EtherBullet : ThingDef
    {
        public float AddHediffChance = 0.99f;
        public HediffDef HediffToAdd = null;
    }
}
