// Comp_MorphTrigger.cs modified by Iron Wolf for Pawnmorph on 08/05/2019 6:44 PM
// last updated 08/05/2019  6:44 PM

using System.Collections.Generic;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// hediff comp that will set the severity to a certain value when the pawn's race changes to a certain morph 
    /// </summary>
    /// <seealso cref="Verse.HediffComp" />
    public class Comp_MorphTrigger : HediffComp
    {
        /// <summary>
        /// Gets the props.
        /// </summary>
        /// <value>
        /// The props.
        /// </value>
        public CompProperties_MorphTrigger Props => (CompProperties_MorphTrigger) props;

        /// <summary>
        /// attempts to trigger this instance.
        /// </summary>
        /// <param name="def">The definition.</param>
        public void TryTrigger(MorphDef def)
        {
            if (def != Props.morph) return;
            parent.Severity = Props.severityValue;
        }
        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            parent.Severity = parent.def.initialSeverity; 
        }

    }

    /// <summary>
    /// comp properties for morph trigger hediff comp 
    /// </summary>
    /// <seealso cref="Verse.HediffCompProperties" />
    public class CompProperties_MorphTrigger : HediffCompProperties
    {
        /// <summary>
        /// The morph that will trigger the change in severity 
        /// </summary>
        public MorphDef morph;
        /// <summary>
        /// The severity value that the hediff will be set to when the pawn is the given morph 
        /// </summary>
        public float severityValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompProperties_MorphTrigger"/> class.
        /// </summary>
        public CompProperties_MorphTrigger() 
        {
            compClass = typeof(Comp_MorphTrigger);
        }
        /// <summary>
        /// gets a collection of configuration errors with this instance 
        /// </summary>
        /// <param name="parentDef">The parent definition.</param>
        /// <returns></returns>
        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string configError in base.ConfigErrors(parentDef)) yield return configError;

            if (morph == null)
            {
                var comp = parentDef.CompProps<CompProperties_MorphInfluence>();
                if (comp != null) morph = comp.morph;

                if (morph == null)
                    yield return "there is no morph set for Comp_MorphTrigger and no MorphInfluence component with a set morph!";
            }
        }
    }
}