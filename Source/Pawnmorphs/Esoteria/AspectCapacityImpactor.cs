using Verse;

namespace Pawnmorph
{
    public class AspectCapacityImpactor : PawnCapacityUtility.CapacityImpactor
    {
        public AspectCapacityImpactor(Aspect aspect)
        {
            Aspect = aspect;
        }

        public Aspect Aspect { get; }

        public override bool IsDirect => false;

        public override string Readable(Pawn pawn)
        {
            return Aspect.Label;
        }
    }
}
