using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.TfSys;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// hediff giver that tries to transform a pawn
	/// </summary>
	/// <seealso cref="Verse.HediffGiver" />
	public class HediffGiver_TF : HediffGiver, IPawnTransformer, IInitializable
	{
		/// <summary>
		/// Gets all Configuration errors in this instance.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string configError in base.ConfigErrors())
			{
				yield return configError;
			}
			if (pawnkinds.NullOrEmpty()) yield return "no pawnkinds set";
		}

		/// The pawnKind of the animal to be transformed into.
		public List<PawnKindDef> pawnkinds;
		/// Tale to add to the tales.
		public TaleDef tale;
		/// The gender that will be forced (i.e. a ChookMorph will be forced female).
		public TFGender forceGender = TFGender.Original;
		/// If forceGender is provided, this is the chance the gender will be forced.
		public float forceGenderChance = 50f;

		private float changeChance = -1;

		/// <summary>Tries to transform the pawn</summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="cause">The cause.</param>
		/// <returns></returns>
		bool IPawnTransformer.TryTransform(Pawn pawn, [CanBeNull] Hediff cause)
		{
			float chance = changeChance < 0 // If changeChance wasn't overriden use the default from the settings.
				? LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().transformChance
				: changeChance;

			//apply the new stat 

			chance = Mathf.Clamp(chance * pawn.GetStatValue(PMStatDefOf.TransformationSensitivity), 0, 100);

			bool changed = false;

			if (Rand.Range(0, 100) < chance)
			{
				changed = TransformPawn(pawn, cause);
			}

			return changed;
		}
		/// <summary>Transforms the pawn.</summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="cause">The cause.</param>
		/// <returns></returns>
		public bool TransformPawn(Pawn pawn, [CanBeNull] Hediff cause)
		{
			var mutagen = cause?.def?.GetMutagenDef() ?? MutagenDefOf.defaultMutagen;

			var request = new TransformationRequest(pawnkinds.RandElement(), pawn)
			{
				forcedGender = forceGender,
				forcedGenderChance = forceGenderChance,
				cause = cause,
				tale = tale,
				transformedTick = Find.TickManager?.TicksAbs
			};

			var inst = mutagen.MutagenCached.Transform(request);

			if (inst != null)
			{
				var comp = Find.World.GetComponent<PawnmorphGameComp>();
				comp.AddTransformedPawn(inst);
			}

			return inst != null;
		}
	}
}