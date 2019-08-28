// ThingComp_ModulatorOptions.cs modified by Iron Wolf for Pawnmorph on 08/27/2019 8:53 AM
// last updated 08/27/2019  8:53 AM

using System.Collections.Generic;
using Verse;

namespace Pawnmorph.Chambers
{
    public class ThingComp_ModulatorOptions : ThingComp
    {
        private ThingCompProperties_ModulatorOptions Props => (ThingCompProperties_ModulatorOptions) props; 



    }
    /// <summary>
    /// property for the mutagen chamber to get it's default set animal options 
    /// </summary>
    /// <seealso cref="Verse.CompProperties" />
    public class ThingCompProperties_ModulatorOptions : CompProperties
    {
        public List<PawnKindDef> defaultAnimals = new List<PawnKindDef>();
        public List<PawnKindDef> merges = new List<PawnKindDef>(); 


        public ThingCompProperties_ModulatorOptions()
        {
            compClass = typeof(ThingComp_ModulatorOptions); 
        }
    }
}