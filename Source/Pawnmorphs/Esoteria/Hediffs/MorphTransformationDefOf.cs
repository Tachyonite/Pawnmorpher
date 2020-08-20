// MorphDefs.cs modified by Iron Wolf for Pawnmorph on 07/31/2019 1:27 PM
// last updated 07/31/2019  1:27 PM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
#pragma warning disable 1591
namespace Pawnmorph.Hediffs
{
    /// <summary>
    ///     static def of class containing morph transformation defs
    /// </summary>
    [DefOf]
    public static class MorphTransformationDefOf
    {
        public static HediffDef FullRandomTF;
        public static HediffDef FullRandomTFAnyOutcome; 



        public static HediffDef StabiliserHigh;  //should move this somewhere else 

        //special def 
        public static HediffDef MutagenicBuildup;
        public static HediffDef MutagenicBuildup_Weapon;


        static MorphTransformationDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MorphTransformationDefOf));
            
        }
    }
}