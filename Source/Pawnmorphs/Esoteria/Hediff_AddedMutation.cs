using RimWorld;
using System;
using System.Text;
using Verse;
using Multiplayer.API;

namespace Pawnmorph
{
    public class Hediff_AddedMutation : HediffWithComps
    {
        public override bool ShouldRemove
        {
            get
            {
                return false;
            }
        }
        
        string mutationDescription;
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
                stringBuilder.AppendLine("Efficiency".Translate() + ": " + this.def.addedPartProps.partEfficiency.ToStringPercent());
                return stringBuilder.ToString();
            }
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            if (base.Part == null)
            {
                Log.Error("Part is null. It should be set before PostAdd for " + this.def + ".", false);
                return;
            }
            this.pawn.health.RestorePart(base.Part, this, false);
            for (int i = 0; i < base.Part.parts.Count; i++)
            {
                Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, this.pawn, null);
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
