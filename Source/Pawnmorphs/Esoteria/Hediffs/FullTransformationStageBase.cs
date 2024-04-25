using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.TfSys;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// base class for all full transformation stages 
	/// </summary>
	/// <seealso cref="Verse.HediffStage" />
	/// <seealso cref="Pawnmorph.Hediffs.IExecutableStage" />
	/// <seealso cref="Pawnmorph.IPawnTransformer" />
	/// <seealso cref="Pawnmorph.Hediffs.IInitializable" />
	public abstract class FullTransformationStageBase : HediffStage, IExecutableStage, IPawnTransformer, IInitializable
	{

		/// Tale to add to the tales.
		public TaleDef transformationTale;
		/// The gender that will be forced (i.e. a ChookMorph will be forced female).
		public TFGender forceGender = TFGender.Original;
		/// If forceGender is provided, this is the chance the gender will be forced.
		public float forceGenderChance = 50f;

		private float changeChance = -1;

		/// <summary>
		/// Gets the pawn kind definition to turn the given pawn into
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		[NotNull]
		protected abstract PawnKindDef GetPawnKindDefFor([NotNull] Pawn pawn);

		/// <summary>Tries to transform the pawn</summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="cause">The cause.</param>
		/// <returns></returns>
		public bool TryTransform(Pawn pawn, Hediff cause)
		{
			bool changed = false;
			float chance = changeChance < 0 // If changeChance wasn't overriden use the default from the settings.
								? LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().transformChance
								: changeChance;

			//apply the new stat 
			chance *= pawn.GetStatValue(PMStatDefOf.TransformationSensitivity);
			chance = Mathf.Clamp(chance, 0, 100);


			if (Rand.Range(0, 100) < chance) changed = TransformPawn(pawn, cause);

			return changed;
		}
		/// <summary>Transforms the pawn.</summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="cause">The cause.</param>
		/// <returns></returns>
		public bool TransformPawn(Pawn pawn, Hediff cause)
		{


			var mutagen = cause?.def?.GetMutagenDef() ?? MutagenDefOf.defaultMutagen;
			var request = new TransformationRequest(GetPawnKindDefFor(pawn), pawn)
			{
				forcedGender = forceGender,
				forcedGenderChance = forceGenderChance,
				cause = cause,
				tale = transformationTale,
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

		/// <summary>called when the given hediff enters this stage</summary>
		/// <param name="hediff">The hediff.</param>
		public void EnteredStage(Hediff hediff)
		{
			if (hediff.pawn.Dead) return; //don't tf dead pawns
			TryTransform(hediff.pawn, hediff);
		}

		/// <summary>
		/// Gets all Configuration errors in this instance.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<string> ConfigErrors()
		{
			yield break;
		}
	}
}