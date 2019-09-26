// AspectStage.cs created by Iron Wolf for Pawnmorph on 09/23/2019 12:16 PM
// last updated 09/23/2019  12:16 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// class representing a single stage of a mutation 'aspect'
    /// </summary>
    public class AspectStage
    {
        public string label;
        public string modifier; //will be added to the label in parentheses
        public string description; //if null or empty the aspect will use the def's description 

        [CanBeNull] public List<PawnCapacityModifier> capMods; 

        

    }

    public class AspectCapacityImpactor : PawnCapacityUtility.CapacityImpactor
    {
        public Aspect Aspect { get; }

        public AspectCapacityImpactor(Aspect aspect)
        {
            Aspect = aspect;
        }

        public override string Readable(Pawn pawn)
        {
            return Aspect.Label;
        }

        public override bool IsDirect => false; 
    }
}