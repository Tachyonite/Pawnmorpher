using Verse;

namespace Pawnmorph.ThingComps
{
    /// <summary>
    /// properties for the always former human comp 
    /// </summary>
    public class CompProperties_AlwaysFormerHuman : CompProperties
    {
        /// <summary>
        /// the former human hediff 
        /// </summary>
        public HediffDef hediff;

        /// <summary>
        /// create a new instance of this class 
        /// </summary>
        public CompProperties_AlwaysFormerHuman()
        {
            compClass = typeof(Comp_AlwaysFormerHuman);
        }
    }
}
