// Comp_MorphTrigger.cs modified by Iron Wolf for Pawnmorph on 08/05/2019 6:44 PM
// last updated 08/05/2019  6:44 PM

using System.Collections.Generic;
using Verse;

namespace Pawnmorph.Hediffs
{
    public class Comp_MorphTrigger : HediffComp
    {


        public CompProperties_MorphTrigger Props => (CompProperties_MorphTrigger) props;

       

        public void TryTrigger(MorphDef def)
        {
           

            if (def != Props.morph) return;

            parent.Severity = Props.severityValue; 



        }

        public void Reset()
        {
            parent.Severity = parent.def.initialSeverity; 
        }


    }


    public class CompProperties_MorphTrigger : HediffCompProperties
    {
        public MorphDef morph;
        public float severityValue;


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
           



            foreach (string configError in base.ConfigErrors(parentDef))
            {
                yield return configError;
            }

           
            if (morph == null)
            {

                var comp = parentDef.CompProps<CompProperties_MorphInfluence>();
                if (comp != null)
                {
                    morph = comp.morph; 
                }

                if(morph == null)
                    yield return "there is no morph set for Comp_MorphTrigger and no MorphInfluence component with a set morph!"; 
            }

        }
    }
}