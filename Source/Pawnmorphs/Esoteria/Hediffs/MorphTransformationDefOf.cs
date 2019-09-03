// MorphDefs.cs modified by Iron Wolf for Pawnmorph on 07/31/2019 1:27 PM
// last updated 07/31/2019  1:27 PM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

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

        public static HediffDef PawnmorphWolfTF;
        public static HediffDef PawnmorphWargTF;
        public static HediffDef PawnmorphHuskyTF;
        public static HediffDef PawnmorphFoxTF;
        public static HediffDef PawnmorphBearTF;
        public static HediffDef PawnmorphChickenTF;
        public static HediffDef PawnmorphCowTF;
        public static HediffDef PawnmorphPigTF;
        public static HediffDef PawnmorphBoarTF;
        public static HediffDef PawnmorphDeerTF;
        public static HediffDef PawnmorphRatTF;
        public static HediffDef PawnmorphBoomalopeTF;
        public static HediffDef PawnmorphAlpacaTF; //these are initialized by rimworld at load time 


        public static HediffDef StabiliserHigh;  //should move this somewhere else 

        //special def 
        public static HediffDef MutagenicBuildup; 


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