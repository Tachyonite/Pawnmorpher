// PMIncidentDefOf.cs modified by Iron Wolf for Pawnmorph on 08/29/2019 3:34 PM
// last updated 08/29/2019  3:34 PM

using RimWorld;

namespace Pawnmorph
{
    /// <summary>
    /// static container for incident defs
    /// </summary>
    [DefOf]
    public static class PMIncidentDefOf
    {
        static PMIncidentDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(IncidentDef));
        }

        public static IncidentDef MutagenicShipPartCrash;
        public static IncidentDef Disease_Cowflu;
        public static IncidentDef Disease_Foxflu;
        public static IncidentDef Disease_Chookflu;
        public static IncidentDef MutagenicFallout; 
    }
}