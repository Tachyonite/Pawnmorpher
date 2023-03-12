using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Hediffs.Composable
{
	/// <summary>
	/// A callback that's called on the transformed pawn after a full transformation
	/// TODO - these can probably just be comps
	/// </summary>
	public abstract class TFCallback
	{
		/// <summary>
		/// A callback that's called on the transformed pawn after a full transformation
		/// </summary>
		/// <param name="pawn">The post-tf Pawn.</param>
		/// <param name="parentHediff">The hediff doing the transformation.</param>
		public abstract void PostTransformation(Pawn pawn, Hediff_MutagenicBase parentHediff);

		/// <summary>
		/// A debug string printed out when inspecting the hediffs
		/// </summary>
		/// <param name="hediff">The parent hediff.</param>
		/// <returns>The string.</returns>
		public virtual string DebugString(Hediff_MutagenicBase hediff) => "";
	}

	/// <summary>
	/// A callback that adds a hediff to a post-transformation pawn
	/// </summary>
	public class TFCallback_AddHediff : TFCallback
	{
		/// <summary>
		/// The hediff to add.
		/// </summary>
		[UsedImplicitly] public HediffDef hediff;

		/// <summary>
		/// The body parts to affect.  All specified body parts will be affected, if present.
		/// </summary>
		[UsedImplicitly] public List<BodyPartRecord> bodyPartsToAffect;

		/// <summary>
		/// A callback that's called on the transformed pawn after a full transformation
		/// </summary>
		/// <param name="pawn">The post-tf Pawn.</param>
		/// <param name="parentHediff">The hediff doing the transformation.</param>
		public override void PostTransformation(Pawn pawn, Hediff_MutagenicBase parentHediff)
		{
			if (bodyPartsToAffect != null)
			{
				foreach (BodyPartRecord bp in pawn.health.hediffSet.GetNotMissingParts().Where(bodyPartsToAffect.Contains))
				{
					var hd = HediffMaker.MakeHediff(hediff, pawn, bp);
					pawn.health.AddHediff(hd, bp);
				}
			}
			else
			{
				var hd = HediffMaker.MakeHediff(hediff, pawn);
				pawn.health.AddHediff(hd);
			}
		}
	}

	/// <summary>
	/// A callback that adds a mental state to a post-transformation pawn
	/// </summary>
	public class TFCallback_AddMentalState : TFCallback
	{
		/// <summary>
		/// The mental state to add.
		/// </summary>
		[UsedImplicitly] public MentalStateDef mentalState;

		/// <summary>
		/// A callback that's called on the transformed pawn after a full transformation
		/// </summary>
		/// <param name="pawn">The post-tf Pawn.</param>
		/// <param name="parentHediff">The hediff doing the transformation.</param>
		public override void PostTransformation(Pawn pawn, Hediff_MutagenicBase parentHediff)
		{
			pawn.mindState.mentalStateHandler.TryStartMentalState(mentalState, "MentalStateReason_Hediff".Translate(parentHediff.Label));
		}
	}
}
