// FormerHumanSettings.cs modified by Iron Wolf for Pawnmorph on 12/24/2019 9:20 AM
// last updated 12/24/2019  9:20 AM

using System.Collections.Generic;
using JetBrains.Annotations;
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
        /// The allowed work types
        /// </summary>
        [NotNull]
        public List<WorkTypeDef> allowedWorkTypes = new List<WorkTypeDef>();

        /// <summary>
        /// The allowed work tags
        /// </summary>
        public WorkTags allowedWorkTags; 
    }
}