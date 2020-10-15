// MutationDef.cs modified by Iron Wolf for Pawnmorph on 01/11/2020 8:16 AM
// last updated 01/11/2020  8:16 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    ///     custom hediff def for mutations
    /// </summary>
    /// <seealso cref="Verse.HediffDef" />
    public class MutationDef : HediffDef, IDebugString
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
        ///     The default chance to add this mutation
        /// </summary>
        public float defaultAddChance = 1f;

        /// <summary>
        ///     The default value indicating weather or not this mutation blocks a transformation chain until it is added
        /// </summary>
        public bool defaultBlocks = false;

        /// <summary>
        /// if this mutation can be tagged and stored 
        /// </summary>
        public bool isTaggable = true;

        /// <summary>
        /// optional field that will act as an explicit description for the mutations 'genome' object
        /// </summary>
        public string customGenomeDescription;

        /// <summary>
        ///     list of other mutations this mutation blocks
        /// </summary>
        public List<BlockEntry> blockList = new List<BlockEntry>();

        /// <summary>
        /// list of body part defs that this mutations blocks other mutations from being added onto 
        /// </summary>
        public List<BodyPartDef> blockSites = new List<BodyPartDef>();


        /// <summary>
        /// The abstract 'value' of this mutation, can be negative or zero if the mutation is in general negative 
        /// </summary>
        public int value; 

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
        ///     returns a full, detailed, representation of the object in string form
        /// </summary>
        /// <returns></returns>
        public string ToStringFull()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"-{defName}/{label}-");
            if (parts == null)
                builder.AppendLine("full body mutation");
            else
                builder.AppendLine($"parts:[{parts.Join(n => n.defName)}]");

            return builder.ToString();
        }

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
        public RemoveFromPartCompProperties RemoveComp => CompProps<RemoveFromPartCompProperties>();

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
        ///     checks if this mutation blocks the addition of the otherMutation.
        /// </summary>
        /// checks if this mutation on the 'thisPart' blocks the addition of the otherMutation on the 'addPart'
        /// checks if this mutation on the 'thisPart' blocks the addition of the otherMutation on the 'addPart'
        /// <param name="otherMutation">The other mutation.</param>
        /// <param name="thisPart">The part this mutation is already on.</param>
        /// <param name="addPart">The  part the otherMutation will be added to.</param>
        /// <returns></returns>
        public bool BlocksMutation([NotNull] MutationDef otherMutation, [CanBeNull] BodyPartRecord thisPart,
                                   [CanBeNull] BodyPartRecord addPart)
        {
            if (blockSites?.Contains(addPart?.def) == true) return true; 
            BlockEntry entry = blockList?.FirstOrDefault(e => e.mutation == otherMutation);
            if (entry == null) return false;
            return thisPart == addPart || entry.blockOnAnyPart;
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

            foreach (BlockEntry entry in blockList.MakeSafe())
                if (entry.mutation == null)
                    yield return "block entry has missing mutation def!";
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

            if (parts != null)
            {
                //get rid of any duplicates 
                _tmpPartLst.Clear();
                _tmpPartLst.AddRange(parts.Distinct()); 
                parts.Clear();
                parts.AddRange(_tmpPartLst);

            }
        }

        [NotNull]
        private static readonly List<BodyPartDef> _tmpPartLst = new List<BodyPartDef>();

        /// <summary>
        /// The explicit genome definition
        /// </summary>
        public ThingDef explicitGenomeDef;

        /// <summary>
        /// The implicit genome definition
        /// </summary>
        internal ThingDef implicitGenomeDef;

        /// <summary>
        /// Gets the thing def for the genome item that gives this mutation.
        /// </summary>
        /// <value>
        /// The genome definition that gives this mutation, can be null if none exist.
        /// </value>
        [CanBeNull]
        public ThingDef GenomeDef => explicitGenomeDef ?? implicitGenomeDef; 

        /// <summary>
        ///     simple class for a single 'block entry'
        /// </summary>
        public class BlockEntry
        {
            /// <summary>
            ///     The mutation to block from being added
            /// </summary>
            public MutationDef mutation;

            /// <summary>
            ///     if true, the mutation will be block from any part, not just on the same part this mutation is on
            /// </summary>
            public bool blockOnAnyPart;

            /// <summary>
            /// Checks if the given source mutation blocks the given otherMutation being added at the given part
            /// </summary>
            /// <param name="sourceMutation">The source mutation.</param>
            /// <param name="otherMutation">The other mutation.</param>
            /// <param name="addPart">The add part.</param>
            /// <returns></returns>
            public bool Blocks([NotNull] Hediff_AddedMutation sourceMutation, [NotNull] MutationDef otherMutation, [CanBeNull] BodyPartRecord addPart)
            {
                if (sourceMutation == null) throw new ArgumentNullException(nameof(sourceMutation));
                if (otherMutation == null) throw new ArgumentNullException(nameof(otherMutation));

                if (otherMutation != mutation)
                {
                    return false; 
                }

                return blockOnAnyPart || addPart == sourceMutation.Part; 

            }
        }

        
    }
}