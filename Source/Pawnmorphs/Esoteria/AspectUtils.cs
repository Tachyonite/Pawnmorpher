// AspectUtils.cs modified by Iron Wolf for Pawnmorph on 09/22/2019 11:30 AM
// last updated 09/22/2019  11:30 AM

using System;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph
{
    public static class AspectUtils
    {
        
        [CanBeNull]
        public static AspectTracker GetAspectTracker([NotNull] this Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

            return pawn.GetComp<AspectTracker>(); 
        }
    }
}