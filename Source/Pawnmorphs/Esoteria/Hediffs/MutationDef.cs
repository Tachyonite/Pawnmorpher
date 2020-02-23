// MutationDef.cs modified by Iron Wolf for Pawnmorph on 01/11/2020 8:16 AM
// last updated 01/11/2020  8:16 AM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
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
        [CanBeNull] public List<BodyPartDef> parts = new List<BodyPartDef>();

        /// <summary>the number of parts to add this mutation to</summary>
        [Obsolete] public int countToAffect;

        /// <summary>
        ///     the various mutation categories this mutation belongs to
        /// </summary>
        public List<MutationCategoryDef> categories = new List<MutationCategoryDef>();

        /// <summary>
        /// The default chance to add this mutation 
        /// </summary>
        public float defaultAddChance = 1f;

        /// <summary>
        /// The default value indicating weather or not this mutation blocks a transformation chain until it is added 
        /// </summary>
        public bool defaultBlocks = false; 

        /// <summary>
        ///     the rule pack to use when generating mutation logs for this mutation
        /// </summary>
        [CanBeNull] public RulePackDef mutationLogRulePack;


        /// <summary>The mutation tale</summary>
        [CanBeNull] public TaleDef mutationTale;

        /// <summary>
        ///     The class this part gives influence for
        /// </summary>
        /// only should be set if morphInfluence is not set!
        [NotNull] [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public AnimalClassBase classInfluence;

        /// <summary>The mutation memory</summary>
        [CanBeNull] public ThoughtDef mutationMemory;

        /// <summary>
        ///     if true, the mutation will not respect the max mutation thoughts mod setting
        /// </summary>
        public bool memoryIgnoresLimit;


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


        /// <summary>
        ///     Gets all configuration errors
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors()) yield return configError;

            if (!typeof(Hediff_AddedMutation).IsAssignableFrom(hediffClass))
                yield return $"{hediffClass.Name} is not {nameof(Hediff_AddedMutation)} or a subtype of it";


            _rmComp = CompProps<RemoveFromPartCompProperties>();
            if (_rmComp == null)
                yield return "mutation does not have a remover comp!";
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
            if (classInfluence == null) return false;
            return classDef.Contains(classInfluence);
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
            if (classInfluence == null) return false;
            return classInfluence.Contains(morph);
        }

        /// <summary>
        ///     Resolves the references.
        /// </summary>
        public override void ResolveReferences()
        {
            base.ResolveReferences();
            classInfluence = classInfluence ?? AnimalClassDefOf.Animal;

            if (mutationMemory == null)
            {
                mutationMemory = DefDatabase<ThoughtDef>.GetNamedSilentFail(defName);
                if (mutationMemory != null)
                {
                    //Log.Message($"{defName} has implicitly defined {nameof(mutationMemory)}, this should be assigned explicitly");
                }
            }

        }
    }
}