// SpreadingMutationComp.cs modified by Iron Wolf for Pawnmorph on 09/25/2019 6:47 PM
// last updated 09/25/2019  6:47 PM

using System.Linq;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// hediff comp for making a mutation spread over a body 
	/// </summary>
	public class SpreadingMutationComp : HediffCompBase<SpreadingMutationCompProperties>
	{
		private bool _finishedSearching;

		private const int RECHECK_PART_PERIOD = 1000;
		private int _doneTick = 0;
		private Comp_MutationSeverityAdjust _severityAdjustComp;

		public override void CompPostPostAdd(DamageInfo? dinfo)
		{
			base.CompPostPostAdd(dinfo);
			_severityAdjustComp = parent.GetComp<Comp_MutationSeverityAdjust>();
		}

		public void UpdateComp()
		{
			// Don't spread skin if skin progression is halted.
			if (_severityAdjustComp?.Halted == true)
				return;

			if (_finishedSearching == false)
			{
				// Have the mutagen sensitivity stat affect the rate of spread
				// This stat is expensive to calculate, but we're only checking once a second
				// Don't worry about division by zero, MTBEventOccurs handles infinity correctly
				var mtb = Props.mtb / StatsUtility.GetStat(Pawn, PMStatDefOf.MutagenSensitivity, 1000) ?? 0;

				if (!Rand.MTBEventOccurs(mtb, 60000f, 60f)) return;

				if (TryInfectPart(parent.Part, false)) return; //try infecting downward first 
				if (TryInfectPart(parent.Part)) return; //try infecting upward

				_finishedSearching = true; //searched all available parts, don't bother looking anymore 
				_doneTick = Find.TickManager.TicksGame;
			}
			else if (Find.TickManager.TicksGame - _doneTick > RECHECK_PART_PERIOD)
				_finishedSearching = false;
		}


		bool IsBlocker(Hediff h)
		{
			if (h.def == Def)
			{

				return true;
			}
			var tComp = parent.TryGetComp<RemoveFromPartComp>();
			Assert(tComp != null, "tComp != null");
			var oComp = h.TryGetComp<RemoveFromPartComp>();
			if (oComp == null) return false; //can't be a blocker if it doesn't have a remover comp 


			if ((oComp.Layer & tComp.Layer) != 0)
			{
				return oComp.AddedTick > tComp.AddedTick;

			}

			return false;

			//to add the hediff must be on a different layer or be younger then the other hediff, thus capable of removing it 

		}

		/// <summary>
		/// try to infect a single part 
		/// </summary>
		/// <param name="record"></param>
		/// <param name="upward"></param>
		/// <param name="depth"></param>
		/// <returns>true if a part could be successfully infected </returns>
		bool TryInfectPart(BodyPartRecord record, bool upward = true, int depth = 0)
		{
			if (!CanTraverse(record)) return false; //make sure not to traverse over missing body parts 
			if (CanInfect(record))
			{
				var hediff = HediffMaker.MakeHediff(Def, Pawn, record);
				if (hediff is Hediff_AddedMutation mutation)
				{
					// Include the skin mutation as the actual cause.
					mutation.Causes.Add(string.Empty, parent.def);

					// Include the source body part too.
					mutation.Causes.Add("SOURCEPART", parent.Part.def);
				}


				Pawn.health.AddHediff(hediff, record);

				return true;
			}

			if (depth >= Props.maxTreeSearchDepth)
			{
				return false; //at max depth, don't go any farther 
			}

			//recurse up or down the tree 
			if (upward)
			{
				if (record.parent == null) return false; //no parent just return false 
				int add = Pawn.RaceProps.body.GetAllMutableParts().Contains(record.parent) && IsSkinPart(record.parent) ? 1 : 0;

				return TryInfectPart(record.parent, true, depth + add);
			}
			else if (record.parts != null)
			{
				foreach (BodyPartRecord bodyPartRecord in record.parts)
				{
					int add = Pawn.RaceProps.body.GetAllMutableParts().Contains(bodyPartRecord) && IsSkinPart(bodyPartRecord) ? 1 : 0;
					if (TryInfectPart(bodyPartRecord, false, depth + add))
					{
						return true; //abort on the first successful add 
					}
				}
			}

			return false;


		}


		bool CanTraverse(BodyPartRecord record)
		{
			return !Pawn.health.hediffSet.PartIsMissing(record);
		}

		bool IsSkinPart(BodyPartRecord record)
		{
			return record?.def?.IsSkinCovered(record, Pawn.health.hediffSet) ?? false;
		}



		bool CanInfect(BodyPartRecord record)
		{
			if (Pawn.health.hediffSet.PartIsMissing(record)) return false; //don't infect missing parts 
			if (!IsSkinPart(record)) return false;

			if (!Pawn.RaceProps.body.GetAllMutableParts().Contains(record))
			{
				return false;
			}
			return !Pawn.health.hediffSet.hediffs.Any(h => h.Part == record && IsBlocker(h)); //only true if the part does not already have a part on it 
		}

		/// <summary>expose all data. in this comp</summary>
		public override void CompExposeData()
		{
			base.CompExposeData();
			Scribe_Values.Look(ref _finishedSearching, nameof(_finishedSearching));
			Scribe_Values.Look(ref _doneTick, nameof(_doneTick));
		}
	}

	/// <summary>
	/// properties for the HediffComp Spreading 
	/// </summary>
	/// <seealso cref="HediffCompPropertiesBase{T}" />
	public class SpreadingMutationCompProperties : HediffCompPropertiesBase<SpreadingMutationComp>
	{
		/// <summary>
		/// how far from the parent's part will this comp search for a part to spread to
		/// </summary>
		/// setting this too high can cause lag 
		public int maxTreeSearchDepth = 2;
		/// <summary>
		/// The mean time between spread checks, in days
		/// </summary>
		public float mtb = 2f;
	}
}