// Giver_MutationChaotic.cs modified by Iron Wolf for Pawnmorph on 08/08/2019 5:36 PM
// last updated 08/08/2019  5:36 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Multiplayer.API;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// hediff giver based off of HediffGiver_Mutation, but instead of one mutation it gives one of many 
    /// </summary>
    public class Giver_MutationChaotic : HediffGiver
    {
        private SimpleCurve _mtbVSeverityCurve;

        SimpleCurve MtbVSeverityCurve
        {
            get
            {
                float min = float.PositiveInfinity; 
                if (_mtbVSeverityCurve == null)
                {
                    List<CurvePoint> points = new List<CurvePoint>(); 
                    foreach (HediffStage hediffStage in hediff.stages)
                    {
                        var mtbs = hediffStage.hediffGivers.MakeSafe()
                                                    .OfType<Giver_MutationChaotic>()
                                                    .Select(g => g.mtbDays)
                                                    .ToList();
                        if (mtbs.Count == 0)
                            continue;
                        var averageMtb = mtbs.Average();
                        if (min > averageMtb) min = averageMtb; //get the min of all stages 
                        points.Add(new CurvePoint(hediffStage.minSeverity, averageMtb)); 

                    }

                    var lStage = hediff.stages.Last();
                    var lSeverity = lStage.minSeverity;
                    points.Add(new CurvePoint(lSeverity, min / 10));

                    _mtbVSeverityCurve = new SimpleCurve();
                    _mtbVSeverityCurve.SetPoints(points); 
                   
                }


                return _mtbVSeverityCurve; 
            }
        }

        private float? _averageMtbUnits;


        float MTBUnits
        {
            get
            {
                if (_averageMtbUnits == null)
                {
                    _averageMtbUnits = hediff.GetAllHediffGivers()
                                             .OfType<Giver_MutationChaotic>()
                                             .Select(g => g.mtbUnits)
                                             .Average(); 
                }

                return _averageMtbUnits.Value; 
            }
        }

      
        private List<MutationDef> _possibleMutations;
        /// <summary>
        /// the morphType to get hediff givers from 
        /// </summary>
        public System.Type morphType = typeof(Hediff_Morph); //the hediff type to get possible mutations from 
        /// <summary>
        /// list of morph categories to exclude 
        /// </summary>
        public List<MorphCategoryDef> blackListCategories = new List<MorphCategoryDef>();
        /// <summary>
        /// list of hediff defs to ignore 
        /// </summary>
        public List<HediffDef> blackListDefs = new List<HediffDef>();
        /// <summary>
        /// list of morphs to exclude 
        /// </summary>
        public List<MorphDef> blackListMorphs = new List<MorphDef>();
        
    
        /// <summary>
        /// how often to give mutations 
        /// </summary>
        public float mtbDays = 0.4f; 

        [NotNull]
        List<MutationDef> Mutations //hediff giver doesn't seem to have a on load or resolve references so I'm using lazy initialization
        {
            get
            {
                if (_possibleMutations == null)
                {
                    _possibleMutations = MutationDef.AllMutations.Where(CheckMutation).ToList();

                    if (_possibleMutations.Count == 0)
                    {
                        Log.Error($"a ChaoticMutation can't get any mutations to add! either things didn't load or the black lists are too large ");
                    }
                   





                }

                return _possibleMutations; 
            }
        }

        private bool CheckMutation(MutationDef arg)
        {
            foreach (var blackMorph in blackListCategories.MakeSafe().SelectMany(c => c.AllMorphsInCategories))
            {
                if (arg.classInfluence == blackMorph) return false;
            }

            return true; 
        }

        /// <summary>
        /// The MTB unit
        /// </summary>
        public float mtbUnits = 60000f; 

        /// <summary>
        /// occurs every so often for all hediffs that have this giver 
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="cause"></param>
        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (Mutations.Count == 0) { return;}
            
            if (MP.IsInMultiplayer)
            {
                Rand.PushState(RandUtilities.MPSafeSeed); 
            }

            var singleComp = cause.TryGetComp<HediffComp_Single>();
            float mult = singleComp?.stacks
                      ?? 1; //the more stacks of partial morphs the pawn has the faster the mutation rate should be 
            mult *= pawn.GetStatValue(PMStatDefOf.MutagenSensitivity);
            mult *= singleComp?.Props?.mutationRateMultiplier ?? 1;
            
            mult = Mathf.Max(0.001f, mult); //prevent division by zero 
           
            if (Rand.MTBEventOccurs(mtbDays / mult, mtbUnits, 60) && pawn.RaceProps.intelligence == Intelligence.Humanlike)
            {
                //mutagen is what contains information like infect-ability of a pawn and post mutation effects 
                var mutagen = (cause as Hediff_Morph)?.GetMutagenDef() ?? MutagenDefOf.defaultMutagen;

                TryApply(pawn, cause, mutagen);
            }

            if (MP.IsInMultiplayer)
            {
                Rand.PopState();
            }

        }
        /// <summary>
        /// Tries to apply this hediff giver 
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="cause">The cause.</param>
        /// <param name="mutagen">The mutagen.</param>
        public void TryApply(Pawn pawn, Hediff cause, MutagenDef mutagen)
        {
            var mut = GetRandomMutation(pawn); //grab a random mutation 
            var mPart = mut.parts?.Count ?? 0;

            int maxCount = mPart == 0 ? 0 : Rand.Range(1, mPart+1); 
            if (MutationUtilities.AddMutation(pawn, mut, maxCount))
            {
                IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);

                var comp = cause.TryGetComp<HediffComp_Single>();
                if (comp != null)
                {
                    comp.stacks--;
                    if (comp.stacks <= 0)
                    {
                        pawn.health.RemoveHediff(cause);
                    }
                }

            }
        }
        
        private const int MAX_TRIES = 10; 

        private MutationDef GetRandomMutation([NotNull] Pawn pawn)
        {
            var mutationGiver = Mutations[Rand.Range(0, Mutations.Count)];
            int i = 0;
            while (i < MAX_TRIES) //doing don't waist too much memory building temporary lists with LINQ 
                                                                            //also means we won't return null if no mutation can be given 
            {
                i++; //make sure we terminate eventually 
                
                mutationGiver = Mutations[Rand.Range(0, Mutations.Count)];

                if (!pawn.HasMutation(mutationGiver))  break; //break if the pawn does not have the mutation yet

            }

            return mutationGiver; 
        }
    }
}