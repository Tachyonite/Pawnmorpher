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

        /// <summary>
        ///     warning, this is slow and recalculates each call!
        /// </summary>
        public static IEnumerable<HediffDef> AllMorphs
        {
            get { return DefDatabase<HediffDef>.AllDefs.Where(def => typeof(Hediff_Morph).IsAssignableFrom(def.hediffClass)); }
        }


        /// <summary>
        ///     all morphs loaded at the start of the game
        /// </summary>
        public static IEnumerable<HediffDef> AllMorphsCached => AllTransformationLst;

        /// <summary>
        /// Gets a random transformation hediff def.
        /// </summary>
        /// <returns></returns>
        public static HediffDef GetRandomTransformation() //needs MP compatibility done in MPCompat branch 
        {
            return AllTransformationLst.RandElement();
        }



        private static List<HediffDef> AllTransformationLst { get; set; } 

        /// <summary>
        ///     the number of morphs loaded at the start of the game
        /// </summary>
        public static int AllMorphsCachedCount { get; }

        static MorphTransformationDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDef));


           AllTransformationLst = AllMorphs.ToList();
            AllMorphsCachedCount = AllTransformationLst.Count;

            if (AllMorphsCachedCount == 0) Log.Warning("there are no morph tf hediffs loaded!");
        }
    }
}