using Verse;

namespace Pawnmorph
{
    public class PawnMorphInstance : IExposable
    {
        public Pawn origin;
        public Pawn replacement;

        public PawnMorphInstance()
        {
        }

        public PawnMorphInstance(Pawn original, Pawn polymorph)
        {
            origin = original;
            replacement = polymorph;
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref origin, true, "origin");
            Scribe_References.Look(ref replacement, "replacement", true);
        }
    }
}
