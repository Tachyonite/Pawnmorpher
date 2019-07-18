using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Pawnmorph
{
    public class HediffCompProperties_AddSeverity : HediffCompProperties
    {
        public HediffDef hediff = null;
        public float severity = 0;
        public float mtbDays = 0;

        public HediffCompProperties_AddSeverity()
        {
            compClass = typeof(HediffComp_AddSeverity);
        }
    }
}
