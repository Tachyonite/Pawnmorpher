﻿// AnimalClassUtilities.cs modified by Iron Wolf for Pawnmorph on 01/10/2020 6:51 PM
// last updated 01/10/2020  6:51 PM

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    ///     static container for various animal classification related utility functions
    /// </summary>
    [StaticConstructorOnStartup]
    public static class AnimalClassUtilities
    {
        [NotNull] private static readonly
            Dictionary<AnimalClassBase, List<MorphDef>> _morphsUnderCache = new Dictionary<AnimalClassBase, List<MorphDef>>();

        [NotNull] private static readonly Dictionary<AnimalClassBase, List<MutationDef>> _mutationClassCache =
            new Dictionary<AnimalClassBase, List<MutationDef>>(); 

        [NotNull] private static readonly Dictionary<AnimalClassBase, float> _accumInfluenceCache =
            new Dictionary<AnimalClassBase, float>();

        [NotNull] private static readonly Dictionary<AnimalClassBase, float> _trickleCache = new Dictionary<AnimalClassBase, float>();
        [NotNull] private static readonly List<AnimalClassBase> _pickedInfluencesCache = new List<AnimalClassBase>();
        [NotNull] private static readonly List<AnimalClassBase> _removeList = new List<AnimalClassBase>();

        [NotNull] private static readonly Dictionary<AnimalClassBase, IReadOnlyList<ThingDef>> _associatedAnimalsCached =
            new Dictionary<AnimalClassBase, IReadOnlyList<ThingDef>>(); 

        const float BASE_ASSOCIATED_ANIMAL_BONUS_MUL = 0.25f;
        private const float FALL_OFF = 1.2f; 
        

        static AnimalClassUtilities()
        {
            foreach (AnimalClassDef animalClassDef in DefDatabase<AnimalClassDef>.AllDefs)
                animalClassDef.FindChildren(); //have to do this after all other def's 'ResolveReferences' have been called 

            foreach (AnimalClassDef animalClassDef in DefDatabase<AnimalClassDef>.AllDefs)
            {
                if (animalClassDef.parent != null) continue;
                if (animalClassDef != AnimalClassDefOf.Animal)
                    Log.Warning($"{animalClassDef.defName} does not have a parent! only {nameof(AnimalClassDefOf.Animal)} should not have a parent!");
            }


            if (CheckForCycles()) //don't precede if there are any cycles in the tree 
                throw
                    new InvalidDataException("detected cycles in animal class tree!"); //not sure what we should throw here, but we can't continue with
            //cycles in the class tree


            //save the pre and post order traversal orders for performance reasons 
            PostorderTreeInternal = TreeUtilities.Postorder<AnimalClassBase>(AnimalClassDefOf.Animal, c => c.Children);
            PreorderTreeInternal = TreeUtilities.Preorder<AnimalClassBase>(AnimalClassDefOf.Animal, c => c.Children).ToList();

        }

        private static List<AnimalClassBase> PreorderTreeInternal { get; }

        private static List<AnimalClassBase> PostorderTreeInternal { get; }

        /// <summary>
        /// Gets all mutation in this class 
        /// </summary>
        /// <param name="animalClass">The animal class.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">animalClass</exception>
        [NotNull]
        public static IReadOnlyList<MutationDef> GetAllMutationIn([NotNull] this AnimalClassDef animalClass)
        {
            if (animalClass == null) throw new ArgumentNullException(nameof(animalClass));
            if (_mutationClassCache.TryGetValue(animalClass, out List<MutationDef> mutations))
            {
                return mutations; 
            }

            mutations = animalClass.GetAllMorphsInClass()
                                   .SelectMany(m => m.AllAssociatedMutations)
                                   .Distinct()
                                   .ToList(); //cache this so we only have to calculate this once 
            _mutationClassCache[animalClass] = mutations;
            return mutations; 
        }



        /// <summary>
        /// Gets the associated animal bonus.
        /// </summary>
        /// <param name="mDef">The m definition.</param>
        /// <param name="targetAnimal">The target animal.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// mDef
        /// or
        /// targetAnimal
        /// </exception>
        public static float GetAssociatedAnimalBonus([NotNull] this MorphDef mDef, [NotNull] ThingDef targetAnimal, int maxHeight=int.MaxValue)
        {
            if (mDef == null) throw new ArgumentNullException(nameof(mDef));
            if (targetAnimal == null) throw new ArgumentNullException(nameof(targetAnimal));

            if (mDef.AllAssociatedAnimals.Contains(targetAnimal)) return BASE_ASSOCIATED_ANIMAL_BONUS_MUL + 1;
            int i = 0;
            AnimalClassBase aC = mDef.ParentClass;
            float bonus = BASE_ASSOCIATED_ANIMAL_BONUS_MUL; 
            while (aC != null && i < maxHeight)
            {
                bonus /= FALL_OFF;
                if (aC.GetAssociatedAnimals().Contains(targetAnimal))
                {
                    return bonus + 1; 
                }

                aC = aC.ParentClass;
                i++; 
            }

            return 1;
        }

        /// <summary>
        /// Gets a list of all animals 'adjacent' to this morph.
        /// </summary>
        /// <param name="mDef">The morph definition definition.</param>
        /// <param name="height">how many parents up to look for adjacent animals</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">mDef</exception>
        /// <exception cref="ArgumentOutOfRangeException">height &lt; 0</exception>
        /// get a list of all animals 'adjacent' to this morph by getting all associated animals for this morph and
        /// all animals associated with it's 'height''th parent class
        public static IReadOnlyList<ThingDef> GetAdjacentAnimals([NotNull] this MorphDef mDef, int height=1)
        {
            if (mDef == null) throw new ArgumentNullException(nameof(mDef));
            if (height < 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (height == 0) return mDef.AllAssociatedAnimals;

            var i = 0;
            AnimalClassBase aClass = mDef;

            while (i < height)
            {
                AnimalClassDef nextClass = aClass.ParentClass;
                i++;
                if (nextClass == null) return GetAssociatedAnimals(aClass);
            }

            return GetAssociatedAnimals(aClass);
        }

        /// <summary>
        /// Gets the associated animals for the given animal class 
        /// </summary>
        /// <param name="animalClass">The animal class.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">animalClass</exception>
        [NotNull]
        public static IReadOnlyList<ThingDef> GetAssociatedAnimals([NotNull] this AnimalClassBase animalClass)
        {
            if (animalClass == null) throw new ArgumentNullException(nameof(animalClass));

            if (_associatedAnimalsCached.TryGetValue(animalClass, out var lst)) return lst;

            if (animalClass is MorphDef mD)
            {
                _associatedAnimalsCached[animalClass] = mD.AllAssociatedAnimals;
                return mD.AllAssociatedAnimals; 
            }

            List<ThingDef> newLst = new List<ThingDef>();

            foreach (AnimalClassBase child in animalClass.Children)
            {
                foreach (ThingDef animal in GetAssociatedAnimals(child))
                {
                    if (!newLst.Contains(animal))
                        newLst.Add(animal); 
                }
            }

            _associatedAnimalsCached[animalClass] = newLst;
            return newLst; 

        }

        

        /// <summary>
        /// Gets all mutation in this class 
        /// </summary>
        /// <param name="animalClass">The animal class.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">animalClass</exception>
        [NotNull]
        public static IReadOnlyList<MutationDef> GetAllMutationIn([NotNull] this AnimalClassBase animalClass)
        {
            if (animalClass == null) throw new ArgumentNullException(nameof(animalClass));
            if (animalClass is AnimalClassDef aClass)
            {
                return GetAllMutationIn(aClass); 
            }

            if (animalClass is MorphDef morph)
            {
                return morph.AllAssociatedMutations; 
            }

            throw new NotImplementedException($"{animalClass.GetType().Name}");
        }

        /// <summary>
        /// Generates  debug information on how part influences are calculated.
        /// </summary>
        /// <param name="mutations">The mutations.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">mutations</exception>
        public static string GenerateDebugInfo([NotNull] IEnumerable<Hediff_AddedMutation> mutations)
        {
            if (mutations == null) throw new ArgumentNullException(nameof(mutations));
            var mutationList = mutations.OrderBy(x => x.Influence.Count).ToList();
            Dictionary<AnimalClassBase, float> endDict = new Dictionary<AnimalClassBase, float>();
            StringBuilder builder = new StringBuilder(); 


            //header 

            builder.AppendLine("Mutations:");

            foreach (Hediff_AddedMutation mutation in mutationList)
            {
                builder.AppendLine($"|\t{mutation.def.defName}:{String.Join(", ", mutation.Influence.Select(x => x.defName))}");

                AnimalClassBase animalClass = null;
                if (mutation.Influence.Count > 1)
                    animalClass = mutation.Influence.Where(x => endDict.ContainsKey(x)).OrderByDescending(x => endDict[x]).FirstOrDefault();

                if (animalClass == null)
                    animalClass = mutation.Influence[0];

                var i = endDict.TryGetValue(animalClass);
                i += 1;
                endDict[animalClass] = i;
            }

            builder.AppendLine("Initial Influences:");

            //raw dict 
            foreach (KeyValuePair<AnimalClassBase, float> kvp in endDict)
            {
                builder.AppendLine($"|\t{kvp.Key.defName}:{kvp.Value}");
            }

            CalculateAccumulatedInfluence(endDict);

            builder.AppendLine("Accumulation Cache");
            foreach (KeyValuePair<AnimalClassBase, float> kvp in _accumInfluenceCache)
            {
                builder.AppendLine($"|\t{kvp.Key.defName}:{kvp.Value}");
            }

            //trickle down 
            CalculateTrickledInfluence(endDict);
            builder.AppendLine("Trickle down cache");
            foreach (KeyValuePair<AnimalClassBase, float> kvp in _trickleCache)
            {
                builder.AppendLine($"|\t{kvp.Key.defName}:{kvp.Value}");
            }

            foreach (AnimalClassBase animalClass in _pickedInfluencesCache) //now calculate the final values 
                endDict[animalClass] = _trickleCache.TryGetValue(animalClass) + endDict.TryGetValue(animalClass);

            builder.AppendLine($"End Amounts");


            foreach (KeyValuePair<AnimalClassBase, float> kvp in endDict)
            {
                builder.AppendLine($"|\t{kvp.Key.defName}:{kvp.Value}");
            }

            return builder.ToString();
        }

        /// <summary>
        ///     Fills the influence dictionary.
        /// </summary>
        /// <param name="mutations">The mutations.</param>
        /// <param name="outDict">The out dictionary.</param>
        /// <exception cref="ArgumentNullException">
        ///     mutations
        ///     or
        ///     outDict
        /// </exception>
        public static void FillInfluenceDict([NotNull] List<Hediff_AddedMutation> mutations,
                                             [NotNull] Dictionary<AnimalClassBase, float> outDict)
        {
            if (mutations == null) throw new ArgumentNullException(nameof(mutations));
            if (outDict == null) throw new ArgumentNullException(nameof(outDict));

            outDict.Clear();
            //initialize the returning dictionary with the direct influences 
            foreach (Hediff_AddedMutation mutation in mutations.OrderBy(x => x.Influence.Count))
            {
                AnimalClassBase animalClass = null;
                if (mutation.Influence.Count > 1)
                    animalClass = mutation.Influence.Where(x => outDict.ContainsKey(x)).OrderByDescending(x => outDict[x]).FirstOrDefault();

                if (animalClass == null)
                    animalClass = mutation.Influence[0];

                var i = outDict.TryGetValue(animalClass);
                i += 1;
                outDict[animalClass] = i;
            }



            CalculateAccumulatedInfluence(outDict);
            CalculateTrickledInfluence(outDict); //now all caches are set 


            foreach (AnimalClassBase animalClass in _pickedInfluencesCache) //now calculate the final values 
                outDict[animalClass] = _trickleCache.TryGetValue(animalClass) + outDict.TryGetValue(animalClass);

            _removeList.Clear(); //remove unneeded classes 
            _removeList.AddRange(outDict.Keys.Where(k => !_pickedInfluencesCache
                                                            .Contains(k))); //remove everything from the dict that was not picked 
            foreach (AnimalClassBase animalClass in _removeList) outDict.Remove(animalClass);
        }

        /// <summary>
        ///     Gets all morphs in the given class.
        /// </summary>
        /// <param name="classDef">The class definition.</param>
        /// <returns></returns>
        [NotNull]
        public static IEnumerable<MorphDef> GetAllMorphsInClass([NotNull] this AnimalClassBase classDef)
        {
            if (classDef == null) throw new ArgumentNullException(nameof(classDef));
           
            if (_morphsUnderCache.TryGetValue(classDef, out List<MorphDef> morphs))
                return morphs;
            morphs = new List<MorphDef>();

            if (classDef is MorphDef morph)
            {
                morphs.Add(morph);
                _morphsUnderCache[classDef] = morphs; 
                return morphs; 
            }

            IEnumerable<AnimalClassBase> preorder = TreeUtilities.Preorder(classDef, c => c.Children);
            foreach (MorphDef morphDef in preorder.OfType<MorphDef>()) morphs.Add(morphDef);
            _morphsUnderCache[classDef] = morphs;
            return morphs;
        }



        /// <summary>
        ///     Calculates the accumulated influence.
        /// </summary>
        /// here we iterate over the classification tree in postorder, accumulating the influence points upward
        /// <param name="initialDict">The initial dictionary.</param>
        private static void CalculateAccumulatedInfluence([NotNull] Dictionary<AnimalClassBase, float> initialDict)
        {
            _pickedInfluencesCache.Clear();
            _accumInfluenceCache.Clear();
            foreach (AnimalClassBase animalClass in PostorderTreeInternal)
            {
                float accum = initialDict.TryGetValue(animalClass);
                foreach (AnimalClassBase child in animalClass.Children)
                    accum += _accumInfluenceCache[child]; //add in the child's accumulated influence 

                _accumInfluenceCache[animalClass] = accum;
            }
        }

        /// <summary>
        ///     Calculates the trickled influence.
        /// </summary>
        /// now iterate over the classification tree in preorder, bringing down accumulated influence from parent to highest influence child 
        /// also fill the _pickedInfluencesCache with nodes with non zero influence and no child nodes with non zero influence
        /// <param name="initialDict">The initial dictionary.</param>
        private static void CalculateTrickledInfluence([NotNull] Dictionary<AnimalClassBase, float> initialDict)
        {
            _pickedInfluencesCache.Clear();
            _trickleCache.Clear();
            foreach (AnimalClassBase animalClass in PreorderTreeInternal)
            {
                AnimalClassBase hChild = null;
                float maxInfluence = 0;
                float trickleDownAmount = initialDict.TryGetValue(animalClass);
                if (animalClass.ParentClass != null)
                    trickleDownAmount +=
                        _trickleCache.TryGetValue(animalClass); //add in any amount the parent chose to give this node 

                foreach (AnimalClassBase child in animalClass.Children)
                {
                    float cInf = _accumInfluenceCache[child];
                    if (cInf > maxInfluence)
                    {
                        hChild = child;
                        maxInfluence = cInf;
                    }
                }

                if (hChild != null)
                    _trickleCache[hChild] = trickleDownAmount;
                else if (trickleDownAmount > 0)
                    _pickedInfluencesCache
                       .Add(animalClass); //if there are no children or none have non zero influences pick this node 
            }
        }

        private static bool CheckForCycles()
        {
            var visitedSet = new HashSet<AnimalClassDef>();

            var stk = new Stack<AnimalClassDef>();
            stk.Push(AnimalClassDefOf.Animal);

            var anyLoops = false;
            while (stk.Count > 0) //simple preorder traversal while checking for loops 
            {
                AnimalClassDef classification = stk.Pop();
                visitedSet.Add(classification);
                foreach (AnimalClassDef subClass in classification.SubClasses)
                {
                    if (visitedSet.Contains(subClass))
                    {
                        anyLoops = true;
                        Log.Error($"visited {subClass.defName} more then once! there must be a cycle in the classifications somewhere!");
                        continue;
                    }

                    stk.Push(subClass);
                }
            }

            return anyLoops;
        }
    }
}