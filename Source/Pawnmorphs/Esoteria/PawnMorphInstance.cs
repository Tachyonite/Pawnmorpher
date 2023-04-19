using System;
using Verse;

#pragma warning disable 01591
namespace Pawnmorph
{
	[Obsolete]
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
