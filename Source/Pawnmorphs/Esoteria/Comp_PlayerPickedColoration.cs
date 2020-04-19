using Pawnmorph.Dialogs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph
{
    public class Comp_PlayerPickedRecoloration : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            ColonistColorPicker.showDialogForPawn(usedBy);
        }
    }
}
