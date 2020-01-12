// Giver_MutationCategoryGiver.cs modified by Iron Wolf for Pawnmorph on 11/24/2019 5:09 PM
// last updated 11/24/2019  5:09 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Multiplayer.API;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    ///     mutation hediff giver that will only grab mutations from specific categories
    /// </summary>
    /// <seealso cref="Verse.HediffGiver" />
    public class Giver_MutationCategoryGiver : HediffGiver
    {
        /// <summary>
        ///     list of mutation categories to look for
        /// </summary>
        [NotNull] public List<MutationCategoryDef> mutationCategories = new List<MutationCategoryDef>();

        /// <summary>
        ///     The morph categories to get mutations from
        /// </summary>
        [NotNull] public List<MorphCategoryDef> morphCategories = new List<MorphCategoryDef>();

        /// <summary>
        ///     The MTB days
        /// </summary>
        public float mtbDays = 0.4f;

        /// <summary>
        ///     The MTB unit
        /// </summary>
        public float mtbUnits = 60000f;

        private List<HediffGiver_Mutation> _mutationsGivers;

        private List<MutationDef> _mutations;

        [NotNull]
        private List<MutationDef> Mutations
        {
            get
            {
                if (_mutations == null)
                {
                    _mutations = new List<MutationDef>();
                    foreach (MorphDef morphDef in morphCategories.SelectMany(m => m.AllMorphsInCategories))
                    foreach (MutationDef mutation in morphDef.AllAssociatedMutations)
                        if (!_mutations.Contains(mutation))
                            _mutations.Add(mutation); //add only distinct values 

                    foreach (MutationDef mutation in mutationCategories.SelectMany(m => m.AllMutations))
                    {
                        if (!_mutations.Contains(mutation))
                            _mutations.Add(mutation); 
                    }
                }

                return _mutations;
            }
        }

        [NotNull]
        [Obsolete]
        private List<HediffGiver_Mutation> MutationsObs
        {
            get
            {
                if (_mutationsGivers == null)
                {
                    var foundMutations = new HashSet<HediffDef>();
                    _mutationsGivers = new List<HediffGiver_Mutation>();
                    foreach (MorphCategoryDef morphCategoryDef in morphCategories)
                    foreach (HediffGiver_Mutation hediffGiverMutation in
                        morphCategoryDef.AllMorphsInCategories.SelectMany(m => m.AllAssociatedAndAdjacentMutationGivers))
                    {
                        if (hediffGiverMutation.hediff == null) continue;
                        if (foundMutations.Contains(hediffGiverMutation.hediff)) continue;
                        _mutationsGivers.Add(hediffGiverMutation);
                        foundMutations.Add(hediffGiverMutation.hediff);
                    }

                    foreach (HediffDef hediffDef in mutationCategories.SelectMany(c => c.AllMutationsInCategoryObs))
                    {
                        if (foundMutations.Contains(hediffDef)) continue;

                        var mDef = hediffDef as MutationDef;
                        if (mDef == null)
                        {
                            Log.Error($"{hediffDef.defName} does not use {nameof(MutationDef)} and is not attached to a hediff giver");
                            continue;
                        }

                        _mutationsGivers.Add(mDef.CreateMutationGiver(hediffDef));
                        foundMutations.Add(hediffDef);
                    }
                }

                return _mutationsGivers;
            }
        }

        /// <summary>
        ///     occurs every so often for all hediffs that have this giver
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="cause"></param>
        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (Mutations.Count == 0) return;

            if (MP.IsInMultiplayer) Rand.PushState(RandUtilities.MPSafeSeed);

            if (Rand.MTBEventOccurs(mtbDays, mtbUnits, 60) && pawn.RaceProps.intelligence == Intelligence.Humanlike)
            {
                MutagenDef mutagen = (cause as Hediff_Morph)?.GetMutagenDef() ?? MutagenDefOf.defaultMutagen;
                TryApply(pawn, cause, mutagen);
            }

            if (MP.IsInMultiplayer) Rand.PopState();
        }

       
        /// <summary>
        ///     Tries to apply this hediff giver
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="cause">The cause.</param>
        /// <param name="mutagen">The mutagen.</param>
        public void TryApply(Pawn pawn, Hediff cause, [NotNull] MutagenDef mutagen)
        {
            if (mutagen == null) throw new ArgumentNullException(nameof(mutagen));
            var mut = Mutations[Rand.Range(0, Mutations.Count)]; //grab a random mutation 
            if (MutationUtilities.AddMutation(pawn, mut))
            {
                IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);
                if (cause.def.HasComp(typeof(HediffComp_Single))) pawn.health.RemoveHediff(cause);
                mutagen.TryApplyAspects(pawn); 
                if (mut.mutationTale != null) TaleRecorder.RecordTale(mut.mutationTale, pawn);
            }
        }
    }
}