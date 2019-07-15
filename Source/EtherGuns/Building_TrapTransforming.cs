using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace EtherGun
{
    // All this does is override the SpringSub() method to use our custom Comp.
    // SpringSub() is called inside of Building_Traps's Spring() method.
    class Building_TrapTransforming : Building_TrapExplosive
    {
        protected override void SpringSub(Pawn p)
        {
            base.GetComp<CompEtherExplosive>().StartWick(null);
        }
    }
}
