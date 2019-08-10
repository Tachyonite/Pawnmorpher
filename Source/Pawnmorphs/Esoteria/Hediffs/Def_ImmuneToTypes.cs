// Def_ImmuneToTypes.cs modified by Iron Wolf for Pawnmorph on 08/09/2019 8:00 AM
// last updated 08/09/2019  8:00 AM

using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// def for the stabilizer to generate the correct immune to message 
    /// </summary>
    public class Def_ImmuneToType : HediffDef
    {
        public Type immuneToType;
        public List<HediffDef> blackList = new List<HediffDef>();

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors())
            {
                yield return configError; 
            }

            if (immuneToType == null) yield return nameof(immuneToType) + " is null";
            if (stages.Count == 0) yield return "there are no stages set"; 

        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();


           
            var stage = stages[0];

            stage.makeImmuneTo = stage.makeImmuneTo ?? new List<HediffDef>();
            var defs = DefDatabase<HediffDef>.AllDefs.Where(def => def != this && !stage.makeImmuneTo.Contains(def)
                                                                && !blackList.Contains(def)
                                                                && immuneToType.IsAssignableFrom(def.hediffClass));
            stage.makeImmuneTo.AddRange(defs); 

        }
    }
}