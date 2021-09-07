// AlertSettings.cs created by Iron Wolf for Pawnmorph on 09/07/2021 6:32 AM
// last updated 09/07/2021  6:32 AM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using RimWorld;
using Verse;

namespace Pawnmorph.Composable.Hediffs
{
    /// <summary>
    /// component that sends an alert when triggered 
    /// </summary>
    /// <seealso cref="Pawnmorph.Hediffs.IInitializableStage" />
    public  class StageAlert : IInitializableStage
    {

        public LetterDef letterDef; 


        /// <summary>
        /// Sends the alert.
        /// </summary>
        /// <param name="mBase">The m base.</param>
        public virtual void SendAlert([NotNull] Hediff_MutagenicBase mBase)
        {

        } 


        /// <summary>
        /// gets all configuration errors in this stage .
        /// </summary>
        /// <param name="parentDef">The parent definition.</param>
        /// <returns></returns>
        public IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            return Enumerable.Empty<string>(); 
        }

        /// <summary>
        /// Resolves all references in this instance.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public void ResolveReferences(HediffDef parent)
        {
            letterDef = letterDef ?? LetterDefOf.NeutralEvent;
        }
    }

   

}