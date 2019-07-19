using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class Hediff_AddedMutation : HediffWithComps
    {
        public string mutationDescription;

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

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            for (int i = 0; i < Part.parts.Count; i++)
            {
                IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.Map);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit && base.Part == null)
            {
                Log.Error("Hediff_AddedPart has null part after loading.", false);
                this.pawn.health.hediffSet.hediffs.Remove(this);
                return;
            }
        }
    }
}
