// MorphPawnKindExtension.cs created by Iron Wolf for Pawnmorph on 09/15/2019 9:43 PM
// last updated 09/15/2019  9:43 PM

using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Pawnmorph
{
    /// <summary> Mod extension for applying morphs to various PawnKinds. </summary>
    public class MorphPawnKindExtension : DefModExtension
    {
        
        /// <summary>the min and max number of hediffs this kind can have</summary>
        public IntRange hediffRange;

        /// <summary>The range for the number of aspects that can be added</summary>
        public IntRange aspectRange; 

        /// <summary>the chance that a given pawn will be a hybrid race</summary>
        /// percentage, [0,1]
        public float morphChance = 0.6f;


        /// <summary>The aspects that can be added by this extension </summary>
        public List<AspectEntry> aspects = new List<AspectEntry>(); 

        /// <summary>
        /// class for a single aspect entry in the <see cref="MorphPawnKindExtension"/>
        /// </summary>
        public class AspectEntry
        {
            /// <summary>The aspect to add</summary>
            public AspectDef aspect;
            /// <summary>The stage to add the aspect at</summary>
            public int stage = 0; 
        }



        /// <summary>The morph categories that can be chosen from</summary>
        public List<MorphCategoryDef> morphCategories = new List<MorphCategoryDef>();

        /// <summary>
        ///     The mutation categories that can be chosen from, this is in addition to those added by
        ///     <seealso cref="morphCategories" />
        /// </summary>
        public List<MutationCategoryDef> mutationCategories = new List<MutationCategoryDef>();

        /// <summary>list of morphs to get mutations from</summary>
        public List<MorphDef> morphs = new List<MorphDef>(); 

        [Unsaved] private List<HediffGiver_Mutation> _mutationGivers;
        /// <summary>Gets all mutation givers that pawns in this group can receive.</summary>
        /// <value>All mutation givers.</value>
        /// this is created by combining the mutations from <seealso cref="morphCategories"/> <seealso cref="mutationCategories"/> and <seealso cref="morphs"/>
        public IEnumerable<HediffGiver_Mutation> AllMutationGivers
        {
            get
            {

                if (_mutationGivers == null)
                {
                    HashSet<MorphDef> morphSet = new HashSet<MorphDef>(); 
                    var defSet = new HashSet<HediffDef>();
                    _mutationGivers = new List<HediffGiver_Mutation>();
                    foreach (MorphDef morphDef in morphCategories.SelectMany(c => c.AllMorphsInCategories)
                    ) //grab all mutation givers from the morphs 
                    {
                        morphSet.Add(morphDef);
                        foreach (HediffGiver_Mutation mutationGiver in morphDef.AllAssociatedAndAdjacentMutations)
                        {
                            if (defSet.Contains(mutationGiver.hediff)) continue; //don't include duplicates 
                            _mutationGivers.Add(mutationGiver);
                            defSet.Add(mutationGiver.hediff);
                        }
                    }

                    foreach (MorphDef morphDef in morphs.Where(m => !morphSet.Contains(m))) //only grab morphs not already included in the categories 
                    {
                        foreach (HediffGiver_Mutation mutationGiver in morphDef.AllAssociatedAndAdjacentMutations)
                        {
                            if (defSet.Contains(mutationGiver.hediff)) continue; //don't include duplicates 
                            _mutationGivers.Add(mutationGiver);
                            defSet.Add(mutationGiver.hediff);
                        }
                    }

                    foreach (HediffDef hediffDef in mutationCategories.SelectMany(c => c.AllMutationsInCategory))
                    {
                        if (defSet.Contains(hediffDef)) continue;
                        var defExtension = hediffDef.GetModExtension<MutationHediffExtension>();
                        if (defExtension == null) //need a def extension to make a hediff giver for them 
                        {
                            Log.Error($"mutation {hediffDef.defName} does not have a mutation def extension!");
                            continue;
                        }

                        var giver = new HediffGiver_Mutation
                        {
                            hediff = hediffDef,
                            partsToAffect = defExtension.parts.ToList(),
                            countToAffect = defExtension.countToAffect
                        };
                        _mutationGivers.Add(giver);
                        defSet.Add(hediffDef);
                    }
                }

                return _mutationGivers;
            }
        }
    }
}