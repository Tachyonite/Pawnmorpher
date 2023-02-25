using System;
using Verse;

#pragma warning disable 01591

namespace Pawnmorph
{
	[Obsolete("use new mutagen system")]
	public class PawnMorphInstanceMerged : IExposable
	{
		public Pawn origin;
		public Pawn origin2;
		public Pawn replacement;
		public PawnMorphInstanceMerged() { }
		public PawnMorphInstanceMerged(Pawn original, Pawn original2, Pawn polymorph)
		{
			origin = original;
			origin2 = original2;
			replacement = polymorph;
		}
		public void ExposeData()
		{
			Scribe_Deep.Look(ref this.origin, true, "originmerged");
			Scribe_Deep.Look(ref this.origin2, true, "originmerged2");
			Scribe_References.Look(ref this.replacement, "replacement", true);
		}
	}
}
