// MorphDefs.cs modified by Iron Wolf for Pawnmorph on 07/31/2019 1:27 PM
// last updated 07/31/2019  1:27 PM

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    ///     static def of class containing morph defs
    /// </summary>
    [DefOf]
    public static class TfDefOf
    {
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
        public static IEnumerable<HediffDef> AllMorphsCached { get; }

        /// <summary>
        ///     the number of morphs loaded at the start of the game
        /// </summary>
        public static int AllMorphsCachedCount { get; }

        static TfDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDef));


            List<HediffDef> lst = AllMorphs.ToList();
            AllMorphsCachedCount = lst.Count;
            AllMorphsCached = lst;

            if (AllMorphsCachedCount == 0) Log.Warning("there are no morph tf hediffs loaded!");
        }
    }
}