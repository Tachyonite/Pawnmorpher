// Comp_MutaniteBed.cs modified by Iron Wolf for Pawnmorph on 11/12/2019 5:36 PM
// last updated 11/12/2019  5:37 PM

using Pawnmorph.Hediffs;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// comp for the mutanite bed 
	/// </summary>
	/// <seealso cref="Verse.ThingComp" />
	public class Comp_MutaniteBed : ThingComp
	{
		private Building_Bed _parent;

		private const float SEV_PER_DAY_TO_TICKS = 0.00333333341f; //constant used in SeverityPerDay comp 

		private float _initialSeverity;
		private float _severityPerTicks;

		private const int TICK_INTERVAL = 250;

		private const float MULTIPLIER = 30f;

		private const float MAX_SEVERITY = 0.8f;

		/// <summary>
		/// called when the parent is spawned 
		/// </summary>
		/// <param name="respawningAfterLoad">if set to <c>true</c> [respawning after load].</param>
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			_parent = parent as Building_Bed;

			var sevPerDayComp = MorphTransformationDefOf.MutagenicBuildup.CompProps<HediffCompProperties_Immunizable>();
			var sevPerDay = sevPerDayComp.severityPerDayNotImmune;

			_initialSeverity = Mathf.Ceil(TICK_INTERVAL / (200f)) * SEV_PER_DAY_TO_TICKS * sevPerDay * MULTIPLIER;
			_initialSeverity = Mathf.Abs(_initialSeverity);

			_severityPerTicks = TICK_INTERVAL * SEV_PER_DAY_TO_TICKS * sevPerDay * MULTIPLIER / 200f;
			_severityPerTicks = Mathf.Abs(_severityPerTicks);

		}


		float GetSeverityOffset(Pawn pawn)
		{
			float toxicSensitivity = 1f - pawn.GetStatValue(StatDefOf.ToxicResistance);
			return toxicSensitivity * _severityPerTicks;
		}

		private const float EPSILON = 0.00001f;

		/// <summary>
		/// Called every 250 ticks.
		/// </summary>
		public override void CompTickRare()
		{
			base.CompTickRare();
			foreach (Pawn curOccupant in _parent.CurOccupants)
			{
				if (curOccupant == null) continue;
				if (!MutagenDefOf.defaultMutagen.CanInfect(curOccupant)) continue;
				ApplyMutagenicBuildup(curOccupant);
			}
		}

		private void ApplyMutagenicBuildup(Pawn curOccupant)
		{
			HediffDef hediffDef = MorphTransformationDefOf.MutagenicBuildup;
			var hediff = curOccupant.health.hediffSet.GetFirstHediffOfDef(hediffDef);
			if (hediff == null)
			{
				hediff = HediffMaker.MakeHediff(hediffDef, curOccupant);
				hediff.Severity = _initialSeverity;
				curOccupant.health.AddHediff(hediff);
				return;
			}

			if (hediff.Severity > MAX_SEVERITY) return;

			float sevOffset = GetSeverityOffset(curOccupant);
			if (sevOffset < EPSILON) return;
			MutagenicBuildupUtilities.AdjustMutagenicBuildup(parent?.def, curOccupant, sevOffset);

		}
	}
}