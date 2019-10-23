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
        /// <summary>
        /// The hediff type to make the pawn immune to 
        /// </summary>
        public Type immuneToType;
        /// <summary>
        /// list of hediffDefs to ignore
        /// </summary>
        public List<HediffDef> blackList = new List<HediffDef>();
        /// <summary>
        /// Get all Configuration Errors with this instance
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors())
            {
                yield return configError; 
            }

            if (immuneToType == null) yield return nameof(immuneToType) + " is null";
            if (stages.Count == 0) yield return "there are no stages set"; 

        }
        /// <summary>
        /// Resolves all references. Called after DefOfs are loaded 
        /// </summary>
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