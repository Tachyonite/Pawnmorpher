// MorphUtilities.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 3:48 PM
// last updated 08/02/2019  3:48 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Hybrids;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    ///     Static collection of useful morph related functions. <br />
    ///     TransformerUtilities was getting a bit crowded.
    /// </summary>
    public static class MorphUtilities
    {
        /// <summary>
        ///     scalar used to make it easier for pawns to become hybrids
        /// </summary>
        [Obsolete] public const float HUMAN_CHANGE_FACTOR = 0.65f;

        /// <summary>the percent influence needed for a single morph to be selected to turn the pawn into, rather then a chimera</summary>
        public const float CHIMERA_THRESHOLD = 0.4f;

        /// <summary>
        ///     the percent human influence below which a pawn is 'no longer considered human'
        /// </summary>
        public const float MORPH_TF_THRESHOLD = 2f / 3f;

        
        private static Dictionary<HediffDef, List<MorphDef>> _morphAssociationCache =
            new Dictionary<HediffDef, List<MorphDef>>(); // So we don't calculate the associations more then we have to.

        private static float? _maxHumanInfluence;

        /// <summary> The maximum possible human influence. </summary>
        public static float MaxHumanInfluence
        {
            get
            {
                if (_maxHumanInfluence == null) _maxHumanInfluence = BodyDefOf.Human.GetAllMutableParts().Count();

                return _maxHumanInfluence.Value;
            }
        }


        /// <summary>
        ///     Checks the race of this pawn. If the pawn is mutated enough it's race is changed to one of the hybrids
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="addMissingMutations">if true, any missing mutations from the highest morph influence will be added</param>
        /// <exception cref="System.ArgumentNullException">pawn</exception>
        public static void CheckRace([NotNull] this Pawn pawn, bool addMissingMutations = true)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

            if (pawn.ShouldBeConsideredHuman()) return;

            MutationTracker mutTracker = pawn.GetMutationTracker();

            MorphDef hInfluence = mutTracker?.HighestInfluence;

            if (hInfluence == null) return;

            float morphInfluence = mutTracker.GetNormalizedInfluence(hInfluence);
            int morphInfluenceCount = mutTracker.NormalizedInfluences.Count();
            if (morphInfluence < CHIMERA_THRESHOLD && morphInfluenceCount > 1) hInfluence = GetChimeraRace(hInfluence, pawn);


            MorphDef curMorph = pawn.def.GetMorphOfRace();
            if (curMorph != hInfluence) RaceShiftUtilities.ChangePawnToMorph(pawn, hInfluence, addMissingMutations);
        }

        /// <summary>Gets all morphs.</summary>
        /// <value>All morphs.</value>
        public static IEnumerable<MorphDef> AllMorphs => DefDatabase<MorphDef>.AllDefs; 

        
        /// <summary> Gets all morphDefs associated with the given transformation. </summary>
        /// <param name="transformationDef"> The transformation definition. </param>
        [NotNull]
        public static IEnumerable<MorphDef> GetAssociatedMorph(HediffDef transformationDef)
        {
            if (_morphAssociationCache.TryGetValue(transformationDef, out List<MorphDef> lst)) return lst;

            lst = GetAssociatedMorphInternal(transformationDef).ToList();
            _morphAssociationCache[transformationDef] = lst;
            return lst;
        }

        /// <summary> Gets the amount of influence a pawn has that's still human.</summary>
        /// <param name="pawn">the pawn</param>
        /// <param name="normalize"> Whether or not the resulting influence should be normalized between [0,1] </param>
        /// <returns></returns>
        public static float GetHumanInfluence([NotNull] this Pawn pawn, bool normalize = false)
        {
            var mutatedRecords = new HashSet<BodyPartRecord>();

            foreach (Hediff_AddedMutation hediffAddedMutation in pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>())
                mutatedRecords.Add(hediffAddedMutation.Part);

            var humanInfluence = (float) pawn.health.hediffSet.GetNotMissingParts()
                                             .Count(p => BodyDefOf.Human.GetAllMutableParts().Contains(p)
                                                      && !mutatedRecords.Contains(p));

            if (normalize)
                humanInfluence /= MaxHumanInfluence;

            return humanInfluence;
        }

        
        /// <summary> Gets the type of the transformation. </summary>
        /// <param name="inst"> The instance. </param>
        /// <returns> The type of the transformation. </returns>
        /// <exception cref="ArgumentNullException"> inst is null </exception>
        public static MorphTransformationTypes GetTransformationType([NotNull] this HediffDef inst)
        {
            if (inst == null) throw new ArgumentNullException(nameof(inst));
            if (!typeof(Hediff_Morph).IsAssignableFrom(inst.hediffClass)) return 0;

            var comp = inst.CompProps<HediffCompProperties_Single>();
            return comp == null ? MorphTransformationTypes.Full : MorphTransformationTypes.Partial;
        }


        /// <summary>checks if the hybrid system is enabled for the given race def.</summary>
        /// <param name="raceDef">The race definition.</param>
        /// <returns></returns>
        public static bool HybridsAreEnabledFor(ThingDef raceDef)
        {
            if (raceDef == ThingDefOf.Human) return true;
            return raceDef.IsHybridRace();
        }

        /// <summary>
        ///     Determines whether this instance is a chimera morph.
        /// </summary>
        /// <param name="morphDef">The morph definition.</param>
        /// <returns>
        ///     <c>true</c> if this instance is a chimera morph; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsChimeraMorph([NotNull] this MorphDef morphDef)
        {
            return MorphCategoryDefOf.Chimera.AllMorphsInCategories.Contains(morphDef);
        }


        /// <summary>
        ///     Determines whether this pawn is a hybrid race.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>
        ///     <c>true</c> if this pawn is a hybrid race ; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">pawn</exception>
        public static bool IsHybridRace([NotNull] this Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            foreach (MorphDef morphDef in DefDatabase<MorphDef>.AllDefs)
                if (pawn.def == morphDef.hybridRaceDef)
                    return true;

            return false;
        }

        /// <summary>
        ///     Determines whether this instance is hybrid race.
        /// </summary>
        /// <param name="raceDef">The race definition.</param>
        /// <returns>
        ///     <c>true</c> if this instance is a hybrid race; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsHybridRace([NotNull] this ThingDef raceDef)
        {
            foreach (MorphDef morphDef in DefDatabase<MorphDef>.AllDefs)
                if (raceDef == morphDef.hybridRaceDef)
                    return true;

            return false;
        }

        /// <summary> Get whether or not the given pawn should still be considered 'human'. </summary>
        public static bool ShouldBeConsideredHuman([NotNull] this Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (pawn.health?.hediffSet?.hediffs == null) return true;

            MutationTracker tracker = pawn.GetMutationTracker();
            if (tracker == null) return true;


            float humanInfluence = GetHumanInfluence(pawn, true);

            return humanInfluence > MORPH_TF_THRESHOLD;
        }

        /// <summary> Get all morphs defs associated with this transformation hediff def. </summary>
        /// <param name="transformationDef"> The transformation definition. </param>
        private static IEnumerable<MorphDef>
            GetAssociatedMorphInternal(
                HediffDef transformationDef) //might want to add it the hediff defs themselves rather then check at runtime 
        {
            IEnumerable<HediffGiver_Mutation> mutationsGiven =
                transformationDef.stages?.SelectMany(s => s.hediffGivers?.OfType<HediffGiver_Mutation>()
                                                       ?? Enumerable.Empty<HediffGiver_Mutation>())
             ?? Enumerable.Empty<HediffGiver_Mutation>(); //all mutations in the def 

            foreach (HediffGiver_Mutation hediffGiverMutation in mutationsGiven)
            {
                IEnumerable<CompProperties_MorphInfluence> comps =
                    hediffGiverMutation.hediff.comps?.OfType<CompProperties_MorphInfluence>();

                if (comps == null) continue;

                foreach (CompProperties_MorphInfluence morphInfluence in comps) yield return morphInfluence.morph;
            }
        }

        private static MorphDef GetChimeraRace(MorphDef hInfluence, Pawn pawn)
        {
            if (hInfluence.categories.Contains(MorphCategoryDefOf.Canid))
                return MorphDefOfs.ChaofoxMorph;
            if (hInfluence.categories.Contains(MorphCategoryDefOf.Reptile))
                return MorphDefOfs.ChaodinoMorph;
            if (hInfluence == MorphDefOfs.BoomalopeMorph) return MorphDefOfs.ChaoboomMorph;
            if (hInfluence == MorphDefOfs.CowMorph) return MorphDefOfs.ChaocowMorph;
            try
            {
                Rand.PushState(pawn.thingIDNumber); // make sure this is deterministic for each pawn 
                return MorphCategoryDefOf.Chimera.AllMorphsInCategories.RandomElement();
            }
            finally
            {
                Rand.PopState();
            }
        }
    }
}