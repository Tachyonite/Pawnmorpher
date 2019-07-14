using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace EtherGun
{
    class Building_TrapTransforming : Building_TrapExplosive
    {
        #region Overrides
        protected override void SpringSub(Pawn p)
        {
            base.GetComp<CompEtherExplosive>().StartWick(null);
        }
        #endregion
    }
}
