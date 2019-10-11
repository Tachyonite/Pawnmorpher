using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
    public class Hediff_AddedMutation : HediffWithComps
    {
        public string mutationDescription;

        /// <summary> The influence this mutation exerts on a pawn. </summary>
        [CanBeNull]
        public Comp_MorphInfluence Influence => (comps?.OfType<Comp_MorphInfluence>().FirstOrDefault());

        public override bool ShouldRemove
        {
            get
            {
                return false;
            }
        }

        public string MutationDescription
        {
            get
            {
                if (mutationDescription == null)
                {
                    StringBuilder builder = new StringBuilder();
                    CreateDescription(builder);
                    mutationDescription = builder.ToString();
                }
                return mutationDescription;
            }
        }

        public override string TipStringExtra
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(base.TipStringExtra);
                stringBuilder.AppendLine("Efficiency".Translate() + ": " + def.addedPartProps.partEfficiency.ToStringPercent());
                return stringBuilder.ToString();
            }
        }

        public virtual void CreateDescription(StringBuilder builder)
        {
            if (def.description == null)
            {
                
                builder.AppendLine("PawnmorphTooltipNoDescription".Translate());
                return;
            }

            string res = def.description.AdjustedFor(pawn);
            builder.AppendLine(res);
        }

        public override void PostAdd(DamageInfo? dinfo)
        // After the hediff has been applied.
        {
            base.PostAdd(dinfo); // Do the inherited method.
            IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.Map); // Spawn some fairy dust ;).

            if (MutationUtilities.AllMutationsWithGraphics.Contains(def) && pawn.IsColonist)
            {
                pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                PortraitsCache.SetDirty(pawn);
            }

            pawn.GetMutationTracker()?.NotifyMutationAdded(this); 
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            pawn.GetMutationTracker()?.NotifyMutationRemoved(this);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit && Part == null)
            {
                Log.Error($"Hediff_AddedPart [{def.defName},{Label}] has null part after loading.", false);
                pawn.health.hediffSet.hediffs.Remove(this);
                return;
            }
        }
    }
}
