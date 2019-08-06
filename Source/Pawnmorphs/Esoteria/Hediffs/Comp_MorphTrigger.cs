// Comp_MorphTrigger.cs modified by Iron Wolf for Pawnmorph on 08/05/2019 6:44 PM
// last updated 08/05/2019  6:44 PM

using System.Collections.Generic;
using Verse;

namespace Pawnmorph.Hediffs
{
    public class Comp_MorphTrigger : HediffComp
    {
        private float _severityForIndex = -1; 


        public CompProperties_MorphTrigger Props => (CompProperties_MorphTrigger) props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            var def = parent.def;

            var stage = def.stages[Props.stageIndex]; //calculate the correct severity to use to jump to the given index 
            var minSeverityStage = stage.minSeverity; 

            float minSeverityNext;
            if (Props.stageIndex == def.stages.Count - 1) //get the last and current index, averaging the severities should put the hediff in the middle of the desired stage
            {
                minSeverityNext = 0; 
            }
            else
            {
                minSeverityNext = def.stages[Props.stageIndex + 1].minSeverity; 
            }

            _severityForIndex = (minSeverityNext + minSeverityStage) / 2;



        }


        public void TryTrigger(MorphDef def)
        {
            if (_severityForIndex < 0)
            {
                Log.Error($"in {parent.def.defName} Comp_MorphTrigger was not set yet?");
                return;
            }

            if (def != Props.morph) return;

            parent.Severity = _severityForIndex; 



        }


    }


    public class CompProperties_MorphTrigger : HediffCompProperties
    {
        public MorphDef morph;
        public int stageIndex;


        public override void PostLoad()
        {
            base.PostLoad();

            

        }

        public CompProperties_MorphTrigger() 
        {
            compClass = typeof(Comp_MorphTrigger);
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            if (stageIndex == 0)
            {
                Log.Warning($"in {parentDef.defName}, morph trigger has trigger stage of 0. This is most likely not intentional!");
            }




            foreach (string configError in base.ConfigErrors(parentDef))
            {
                yield return configError;
            }

            if (stageIndex < 0 || stageIndex >= (parentDef.stages?.Count ?? 0))
            {
                yield return $"stageIndex:{stageIndex} is out of bounds";
            }


            if (morph == null)
            {
                yield return "there is no morph set!"; 
            }

        }
    }
}