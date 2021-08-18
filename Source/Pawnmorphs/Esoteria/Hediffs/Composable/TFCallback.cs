using System.Linq;
using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Hediffs.Composable
{
    /// <summary>
    /// A callback that's called on the transformed pawn after a full transformation
    /// </summary>
    public abstract class TFCallback
    {
        /// <summary>
        /// A callback that's called on the transformed pawn after a full transformation
        /// </summary>
        /// <param name="pawn">The post-tf Pawn.</param>
        /// <param name="parentHediff">The hediff doing the transformation.</param>
        public abstract void PostTransformation(Pawn pawn, Hediff_MutagenicBase parentHediff);
    }

    /// <summary>
    /// A callback that adds a hediff to a post-transformation pawn
    /// </summary>
    public class TFCallback_AddHediff : TFCallback
    {
        [UsedImplicitly] HediffDef hediff;
        [UsedImplicitly] List<BodyPartRecord> bodyPartsToAffect;

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
        [UsedImplicitly] MentalStateDef mentalState;

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
}
