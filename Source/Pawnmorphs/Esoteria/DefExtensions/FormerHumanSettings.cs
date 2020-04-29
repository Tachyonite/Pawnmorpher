// FormerHumanSettings.cs modified by Iron Wolf for Pawnmorph on 12/24/2019 9:20 AM
// last updated 12/24/2019  9:20 AM

using System.Collections.Generic;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.FormerHumans;
using Verse;

namespace Pawnmorph.DefExtensions
{
    /// <summary>
    /// def extension meant to be used on race defs to add setting specific to former human 
    /// </summary>
    /// <seealso cref="Verse.DefModExtension" />
    public class FormerHumanSettings: DefModExtension

    {
        /// <summary>
        /// if true, the attached race will never be a former human 
        /// </summary>
        public bool neverFormerHuman; 

        /// <summary>
        /// The backstory, uses a default if not set 
        /// </summary>
        public BackstoryDef backstory;

        /// <summary>
        /// The food thought settings
        /// </summary>
        public FoodThoughtSettings foodThoughtSettings; 

    }
}