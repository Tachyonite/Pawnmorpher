// RandomMorphTransformationStage.cs modified by Iron Wolf for Pawnmorph on 01/25/2020 11:52 AM
// last updated 01/25/2020  11:52 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// transformation stage that picks a random set of mutations for each pawn 
    /// </summary>
    /// <seealso cref="Pawnmorph.Hediffs.TransformationStageBase" />
    public class RandomMorphTransformationStage : TransformationStageBase
    {




        /// <summary>
        /// returns all configuration errors in this stage
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> ConfigErrors()
        {
            if (morph == null)
                yield return "morph field not set"; 
        }

        /// <summary>
        /// The morph or class to pick from 
        /// </summary>
        public AnimalClassBase morph; 

        [Unsaved,NotNull] private readonly Dictionary<Thing, List<MutationEntry>> _cache = new Dictionary<Thing, List<MutationEntry>>();

        [Unsaved] private List<MorphDef> _morphs;
        /// <summary>
        /// a list of morph categories not to include 
        /// </summary>
        public List<MorphCategoryDef> categoryBlackList = new List<MorphCategoryDef>();

        [NotNull]
        List<MorphDef> AllMorphs
        {
            get
            {
                if (_morphs == null)
                {
                    _morphs = new List<MorphDef>();
                    foreach (MorphDef morphDef in morph.GetAllMorphsInClass())
                    {
                        //if this morph is in any of the black listed categories ignore it 
                        if (morphDef.categories.MakeSafe().Any(c => categoryBlackList.Contains(c)))
                            continue;
                        _morphs.Add(morphDef);
                    }

                }

                return _morphs;
            }
        }
        /// <summary>
        /// The chance to add a mutation 
        /// </summary>
        public float addChance = 1; 

        /// <summary>
        /// Gets the entries for the given pawn
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        public override IEnumerable<MutationEntry> GetEntries(Pawn pawn)
        {
            if (_cache.TryGetValue(pawn, out List<MutationEntry> lst))
                return lst;
            lst = new List<MutationEntry>(); 

           Rand.PushState(pawn.thingIDNumber);
           try
           {
               var rMorph = AllMorphs.RandomElement();
               foreach (MutationDef rMorphAllAssociatedMutation in rMorph.AllAssociatedMutations) //get all mutations from the randomly picked morph 
               {
                   var mEntry = new MutationEntry
                   {
                       addChance = addChance,
                       blocks = false,
                       mutation = rMorphAllAssociatedMutation
                   };
                   lst.Add(mEntry); 
               }

               _cache[pawn] = lst; //cache the results so we only have to do this once per pawn 
               return lst; 
           }
           finally
           {
                Rand.PopState();
           }
        }
    }
}