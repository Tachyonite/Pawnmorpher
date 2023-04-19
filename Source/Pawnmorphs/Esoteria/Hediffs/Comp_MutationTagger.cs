// MutationTagger.cs created by Iron Wolf for Pawnmorph on 02/12/2021 7:12 AM
// last updated 02/12/2021  7:12 AM

using System;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Chambers;
using Pawnmorph.Genebank.Model;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// hediff comp to tag mutations that added during the duration of the hediff 
	/// </summary>
	/// <seealso cref="Verse.HediffComp" />
	/// <seealso cref="Pawnmorph.IMutationEventReceiver" />
	public class Comp_MutationTagger : HediffComp, IMutationEventReceiver
	{



		[CanBeNull] private SimpleCurve Curve => (props as CompProps_MutationTagger)?.tagChancePerValue;

		bool CanTag(MutationDef mDef)
		{
			if (mDef.IsRestricted) return false;
			if (DB.StoredMutations.Contains(mDef)) return false;

			var curve = Curve;
			float chance;
			if (curve != null)
			{
				chance = curve.Evaluate(mDef.value);
			}
			else
			{
				chance = CompProps_MutationTagger.DEFAULT_TAG_CHANCE;
			}

			return Rand.Chance(chance);
		}


		private ChamberDatabase DB => Find.World.GetComponent<ChamberDatabase>();


		/// <summary>called when a mutation is added</summary>
		/// <param name="mutation">The mutation.</param>
		/// <param name="tracker">The tracker.</param>
		public void MutationAdded(Hediff_AddedMutation mutation, MutationTracker tracker)
		{
			try
			{
				var mutationDef = (MutationDef)mutation.def;
				MutationGenebankEntry bankEntry = new MutationGenebankEntry(mutationDef);

				if (CanTag(mutationDef))
				{
					if (!DB.TryAddToDatabase(bankEntry, out string reason))
					{
						Messages.Message(reason, MessageTypeDefOf.RejectInput);
						return;
					}
				}
			}
			catch (InvalidCastException e)
			{
				Log.Error($"in {mutation.Label}/{mutation.def.defName} cannot convert {mutation.def.GetType().Name} to {nameof(MutationDef)}!\n{e}");
			}
		}

		/// <summary>called when a mutation is removed</summary>
		/// <param name="mutation">The mutation.</param>
		/// <param name="tracker">The tracker.</param>
		public void MutationRemoved(Hediff_AddedMutation mutation, MutationTracker tracker)
		{
			//empty 
		}
	}


	/// <summary>
	/// comp properties for the mutation tagger comp 
	/// </summary>
	/// <seealso cref="Verse.HediffCompProperties" />
	public class CompProps_MutationTagger : HediffCompProperties
	{
		/// <summary>
		/// The default tag chance 
		/// </summary>
		public const float DEFAULT_TAG_CHANCE = 0.5f;

		/// <summary>
		/// The tag chance per value
		/// </summary>
		public SimpleCurve tagChancePerValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="CompProps_MutationTagger"/> class.
		/// </summary>
		public CompProps_MutationTagger()
		{
			compClass = typeof(Comp_MutationTagger);
		}
	}

}