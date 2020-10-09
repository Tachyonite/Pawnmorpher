// BuildingDefOf.cs modified by Iron Wolf for Pawnmorph on 08/26/2019 11:18 AM
// last updated 08/26/2019  11:18 AM

using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// def of for frequently accessed building thingDefs 
    /// </summary>
    [DefOf]
    public static class BuildingDefOf
    {
        static BuildingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDef));
        }
#pragma warning disable 1591
        public static ThingDef BigMutagenicChamber;
        public static ThingDef MutagenicModulator;
        public static ThingDef MutagenicChamber;
#pragma warning restore 
    }
}