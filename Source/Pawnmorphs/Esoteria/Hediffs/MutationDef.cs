// MutationDef.cs modified by Iron Wolf for Pawnmorph on 01/11/2020 8:16 AM
// last updated 01/11/2020  8:16 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    ///     custom hediff def for mutations
    /// </summary>
    /// <seealso cref="Verse.HediffDef" />
    public class MutationDef : HediffDef
    {
        /// <summary>
        ///     list of body parts this mutation can be added to
        /// </summary>
        /// note: this does not affect HediffGiver_AddedMutation, this is for adding mutations without a hediff giver
        [NotNull] public List<BodyPartDef> parts = new List<BodyPartDef>();

        /// <summary>the number of parts to add this mutation to</summary>
        public int countToAffect;

        /// <summary>
        ///     the various mutation categories this mutation belongs to
        /// </summary>
        public List<MutationCategoryDef> categories = new List<MutationCategoryDef>();

        /// <summary>
        ///     the rule pack to use when generating mutation logs for this mutation
        /// </summary>
        [CanBeNull] public RulePackDef mutationLogRulePack;


        /// <summary>The mutation tale</summary>
        [CanBeNull] public TaleDef mutationTale;

        /// <summary>
        ///     The morph this part gives morph influence for
        /// </summary>
        public MorphDef morphInfluence;

        /// <summary>
        ///     The class this part gives influence for
        /// </summary>
        /// only should be set if morphInfluence is not set!
        public AnimalClassDef classInfluence;

        /// <summary>The mutation memory</summary>
        [CanBeNull] public ThoughtDef mutationMemory;


        [Unsaved] private RemoveFromPartCompProperties _rmComp;

        [Unsaved] private bool? _isRestricted;

        /// <summary>
        ///     Gets all mutations.
        /// </summary>
        /// <value>
        ///     All mutations.
        /// </value>
        [NotNull]
        public static IEnumerable<MutationDef> AllMutations => DefDatabase<MutationDef>.AllDefs;

        /// <summary>
        ///     Gets the remover comp.
        /// </summary>
        /// this is the comp used to remove 'overlapping' mutations
        /// <value>
        ///     The remove comp.
        /// </value>
        [NotNull]
        public RemoveFromPartCompProperties RemoveComp => _rmComp;

        /// <summary>Gets a value indicating whether this instance is restricted to special PawnKindGroups</summary>
        /// <value>
        ///     <c>true</c> if this instance is restricted the mutation can only be given to special PawnKindGroups; otherwise it
        ///     can show up in any group, <c>false</c>.
        /// </value>
        public bool IsRestricted
        {
            get
            {
                if (_isRestricted == null)
                    _isRestricted =
                        categories.Any(c => c.restricted); //if any of the categories are restricted the whole mutation is restricted 

                return _isRestricted.Value;
            }
        }

        [NotNull]
        internal IAnimalClass InternalInfluence
        {
            get
            {
                if (morphInfluence == null) return classInfluence ?? AnimalClassDefOf.Animal;

                return morphInfluence;
            }
        }

        /// <summary>
        ///     Gets all configuration errors
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors()) yield return configError;

            if (!typeof(Hediff_AddedMutation).IsAssignableFrom(hediffClass))
                yield return $"{hediffClass.Name} is not {nameof(Hediff_AddedMutation)} or a subtype of it";

            if (parts.NullOrEmpty()) yield return "parts list is null or empty!";

            if (morphInfluence != null && classInfluence != null)
                yield return $"both {nameof(morphInfluence)} and {nameof(classInfluence)} are set!";

            _rmComp = CompProps<RemoveFromPartCompProperties>();
            if (_rmComp == null)
                yield return "mutation does not have a remover comp!";
        }


        /// <summary>
        ///     Creates the mutation giver for the parent hediff.
        /// </summary>
        /// <param name="parentDef">The parent definition.</param>
        /// <returns></returns>
        [NotNull]
        public HediffGiver_Mutation CreateMutationGiver([NotNull] HediffDef parentDef)
        {
            var mutationGiver = new HediffGiver_Mutation
            {
                hediff = parentDef,
                partsToAffect = parts.ToList(),
                countToAffect = countToAffect
            };
            return mutationGiver;
        }

        /// <summary>
        ///     checks if this instance gives influence for the given animal class
        /// </summary>
        /// <param name="classDef">The class definition.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">classDef</exception>
        public bool GivesInfluence([NotNull] AnimalClassDef classDef)
        {
            if (classDef == null) throw new ArgumentNullException(nameof(classDef));
            if (InternalInfluence == null) return false;
            return ((IAnimalClass) classDef).Contains(InternalInfluence);
        }

        /// <summary>
        ///     checks if this instance gives influence for the given morph
        /// </summary>
        /// <param name="morph">The morph.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">morph</exception>
        public bool GivesInfluence([NotNull] MorphDef morph)
        {
            if (morph == null) throw new ArgumentNullException(nameof(morph));
            if (InternalInfluence == null) return false;
            return ((IAnimalClass) morph).Contains(InternalInfluence);
        }
    }
}