using RimWorld;
using System;
using System.Text;
using Verse;

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
                builder.AppendLine("RadiologyTooltipNoDescription".Translate());
                return;
            }

            string res = def.description;
            res = res.Replace("PART", Part.customLabel == null ? Part.def.label : Part.customLabel);
            res = res.Replace("NAME", pawn.LabelShortCap);
            res = res.Replace("HE", pawn.gender == Gender.Female ? "RadiologyTooltipShe".Translate() : "RadiologyTooltipHe".Translate());
            res = res.Replace("HIS", pawn.gender == Gender.Female ? "RadiologyTooltipHisHer".Translate() : "RadiologyTooltipHis".Translate());
            res = res.Replace("HIM", pawn.gender == Gender.Female ? "RadiologyTooltipHer".Translate() : "RadiologyTooltipHim".Translate());
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
