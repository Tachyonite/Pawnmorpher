// CompProperties_RemoveType.cs modified by Iron Wolf for Pawnmorph on 08/09/2019 7:32 AM
// last updated 08/09/2019  7:32 AM

using System;
using System.Collections.Generic;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// hediff comp_properties for a comp that removes all hediffs of a certain type 
    /// </summary>
    public class CompProperties_RemoveType : HediffCompProperties
    {
        public Type removeType; //the type of hediff to remove 

        public List<HediffDef> blackList = new List<HediffDef>(); //list of hediffs to make immune to the effect 

        public List<Hediff> ScratchList { get; } = new List<Hediff>(); //scratch list for the comp to use, saves on memory 




        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string configError in base.ConfigErrors(parentDef))
            {
                yield return configError;
            }

            if (removeType == null)
            {
                yield return "remove type is null"; 
            }
        }

        public CompProperties_RemoveType()
        {
            compClass = typeof(Comp_RemoveType);
        }

    }
    /// <summary>
    /// hediff comp that removes all hediffs of a given type 
    /// </summary>
    public class Comp_RemoveType : HediffComp
    {
        private CompProperties_RemoveType Props => (CompProperties_RemoveType) props;


        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            var hediffs = Pawn.health.hediffSet.hediffs; 

            Props.ScratchList.Clear();
            foreach (Hediff hediff in hediffs)
            {
                if (!Props.blackList.Contains(hediff.def) && Props.removeType.IsInstanceOfType(hediff))
                {
                    Props.ScratchList.Add(hediff);
                }
            }


            foreach (Hediff hediff in Props.ScratchList)
            {
                Pawn.health.RemoveHediff(hediff); 
            }


        }
    }
}