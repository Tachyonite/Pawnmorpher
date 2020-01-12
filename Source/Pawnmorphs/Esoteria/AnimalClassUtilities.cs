// AnimalClassUtilities.cs modified by Iron Wolf for Pawnmorph on 01/10/2020 6:51 PM
// last updated 01/10/2020  6:51 PM

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
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
            Dictionary<IAnimalClass, List<MorphDef>> _morphsUnderCache = new Dictionary<IAnimalClass, List<MorphDef>>();

        [NotNull] private static readonly Dictionary<IAnimalClass, float> _accumInfluenceCache =
            new Dictionary<IAnimalClass, float>();

        [NotNull] private static readonly Dictionary<IAnimalClass, float> _trickleCache = new Dictionary<IAnimalClass, float>();
        [NotNull] private static readonly List<IAnimalClass> _pickedInfluencesCache = new List<IAnimalClass>();
        [NotNull] private static readonly List<IAnimalClass> _removeList = new List<IAnimalClass>();
        
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
            PostorderTreeInternal = TreeUtilities.Postorder<IAnimalClass>(AnimalClassDefOf.Animal, c => c.Children);
            PreorderTreeInternal = TreeUtilities.Preorder<IAnimalClass>(AnimalClassDefOf.Animal, c => c.Children).ToList();


            string treeStr =
                TreeUtilities.PrettyPrintTree<IAnimalClass>(AnimalClassDefOf.Animal, a => a.Children, a => ((Def) a).defName);

            Log.Message(treeStr); //print a pretty tree c: 
        }

        private static List<IAnimalClass> PreorderTreeInternal { get; }

        private static List<IAnimalClass> PostorderTreeInternal { get; }

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
                                             [NotNull] Dictionary<IAnimalClass, float> outDict)
        {
            if (mutations == null) throw new ArgumentNullException(nameof(mutations));
            if (outDict == null) throw new ArgumentNullException(nameof(outDict));

            outDict.Clear();
            //initialize the returning dictionary with the direct influences 
            foreach (Hediff_AddedMutation mutation in mutations)
            {
                float i = outDict.TryGetValue(mutation.Influence);
                i++;
                outDict[mutation.Influence] = i;
            }

            CalculateAccumulatedInfluence(outDict);
            CalculateTrickledInfluence(outDict); //now all caches are set 


            foreach (IAnimalClass animalClass in _pickedInfluencesCache) //now calculate the final values 
                outDict[animalClass] = _trickleCache.TryGetValue(animalClass) + outDict.TryGetValue(animalClass);

            _removeList.Clear(); //remove unneeded classes 
            _removeList.AddRange(outDict.Keys.Where(k => !_pickedInfluencesCache
                                                            .Contains(k))); //remove everything from the dict that was not picked 
            foreach (IAnimalClass animalClass in _removeList) outDict.Remove(animalClass);
        }

        /// <summary>
        ///     Gets all morphs in the given class.
        /// </summary>
        /// <param name="classDef">The class definition.</param>
        /// <returns></returns>
        [NotNull]
        public static IEnumerable<MorphDef> GetAllMorphsInClass([NotNull] this IAnimalClass classDef)
        {
            if (classDef == null) throw new ArgumentNullException(nameof(classDef));
           
                if (_morphsUnderCache.TryGetValue(classDef, out List<MorphDef> morphs))
                    return morphs;
                morphs = new List<MorphDef>();

                var preorder = TreeUtilities.Preorder(classDef, c => c.Children);
                foreach (MorphDef morphDef in preorder.OfType<MorphDef>())
                {
                    morphs.Add(morphDef);
                }
                _morphsUnderCache[classDef] = morphs;
                return morphs;
        }



        /// <summary>
        ///     Calculates the accumulated influence.
        /// </summary>
        /// here we iterate over the classification tree in postorder, accumulating the influence points upward
        /// <param name="initialDict">The initial dictionary.</param>
        private static void CalculateAccumulatedInfluence([NotNull] Dictionary<IAnimalClass, float> initialDict)
        {
            _pickedInfluencesCache.Clear();
            _accumInfluenceCache.Clear();
            foreach (IAnimalClass animalClass in PostorderTreeInternal)
            {
                float accum = initialDict.TryGetValue(animalClass);
                foreach (IAnimalClass child in animalClass.Children)
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
        private static void CalculateTrickledInfluence([NotNull] Dictionary<IAnimalClass, float> initialDict)
        {
            _pickedInfluencesCache.Clear();
            _trickleCache.Clear();
            foreach (IAnimalClass animalClass in PreorderTreeInternal)
            {
                IAnimalClass hChild = null;
                float maxInfluence = 0;
                float trickleDownAmount = initialDict.TryGetValue(animalClass);
                if (animalClass.ParentClass != null)
                    trickleDownAmount +=
                        _trickleCache.TryGetValue(animalClass); //add in any amount the parent chose to give this node 

                foreach (IAnimalClass child in animalClass.Children)
                {
                    float cInf = initialDict.TryGetValue(child) + _accumInfluenceCache[child];
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