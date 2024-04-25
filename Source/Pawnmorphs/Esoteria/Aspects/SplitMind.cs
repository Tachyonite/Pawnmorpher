// SplitMind.cs created by Iron Wolf for Pawnmorph on 05/09/2020 1:18 PM
// last updated 05/09/2020  1:18 PM

using System;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.Aspects
{
	/// <summary>
	/// aspect for the giving merged pawns a 'split mind'
	/// </summary>
	/// <seealso cref="Pawnmorph.Aspect" />
	public class SplitMind : Aspect
	{

		void RemoveAllOutlooks()
		{
			var asTracker = Pawn.GetAspectTracker();
			if (asTracker == null) return;

			if (asTracker.Contains(AspectDefOf.PrimalWish))
			{
				asTracker.Remove(AspectDefOf.PrimalWish);
			}

			var story = Pawn.story;

			TraitSet storyTraits = story?.traits;
			storyTraits?.allTraits?.RemoveAll(t => t.def == TraitDefOf.BodyPurist || t.def == PMTraitDefOf.MutationAffinity);
		}

		private MutationOutlook? _curOutlook;

		/// <summary> Called every tick. </summary>
		public override void PostTick()
		{
			base.PostTick();

			if (Pawn.IsHashIntervalTick(60))
			{
				int seed;
				unchecked
				{
					seed = Pawn.thingIDNumber + ((Find.TickManager?.TicksAbs ?? 0) / TimeMetrics.TICKS_PER_HOUR);
				}

				Rand.PushState(seed);
				try
				{
					var outlook = (MutationOutlook)Rand.Range(0, 4);
					if (outlook != _curOutlook)
					{
						_curOutlook = outlook;
						RemoveAllOutlooks();
						AddOutlook(outlook);
					}
				}
				finally
				{
					Rand.PopState();
				}
			}

		}

		void AddOutlook(MutationOutlook mOutlook)
		{
			TraitSet st = Pawn.story?.traits;
			AspectTracker at = Pawn.GetAspectTracker();
			if (st == null) return;
			if (at == null) return;
			switch (mOutlook)
			{
				case MutationOutlook.Neutral:
					return;
				case MutationOutlook.Furry:
					st.GainTrait(new Trait(PMTraitDefOf.MutationAffinity, 0, true));

					break;
				case MutationOutlook.BodyPurist:
					st.GainTrait(new Trait(TraitDefOf.BodyPurist, 0, true));

					break;
				case MutationOutlook.PrimalWish:
					at.Add(AspectDefOf.PrimalWish);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mOutlook), mOutlook, null);
			}
		}


	}
}