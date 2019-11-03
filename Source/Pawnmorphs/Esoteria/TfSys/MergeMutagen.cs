// MergeMutagen.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/14/2019 3:15 PM
// last updated 08/14/2019  3:15 PM

using System;
using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Thoughts;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.TfSys
{
    /// <summary>
    ///     implementation of mutagen that merges 2 or more pawns into a single meld
    /// </summary>
    /// <seealso cref="Pawnmorph.TfSys.Mutagen{T}" />
    public class MergeMutagen : Mutagen<MergedPawns>
    {
        private const string FORMER_HUMAN_HEDIFF = "2xMergedHuman"; //can't put this in a hediffDefOf because of the name 


        


        /// <summary>
        ///     Determines whether this instance can revert pawn the specified transformed pawn.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns>
        ///     <c>true</c> if this instance can revert pawn  the specified transformed pawn; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanRevertPawnImp(MergedPawns transformedPawn)
        {
            if (!transformedPawn.IsValid) return false;

            HediffDef def = HediffDef.Named(FORMER_HUMAN_HEDIFF);

            return transformedPawn.meld.health.hediffSet.HasHediff(def);
        }
        /// <summary>Returns true if the given request is valid.</summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// <c>true</c> if the specified request is valid; otherwise, <c>false</c>.
        /// </returns>
        protected override bool IsValid(TransformationRequest request)
        {
            return base.IsValid(request) && request.originals.Length == 2;
        }
        /// <summary>
        /// Determines whether this instance can transform the specified pawn.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>
        ///   <c>true</c> if this instance can transform the specified pawn; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanTransform(Pawn pawn)
        {
            return base.CanInfect(pawn);
        }

        /// <summary>
        /// preform the requested transform 
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        protected override MergedPawns TransformImpl(TransformationRequest request)
        {
            Pawn firstPawn = request.originals[0];
            Pawn secondPawn = request.originals[1];
            float averageAge = firstPawn.ageTracker.AgeBiologicalYearsFloat
                             + secondPawn.ageTracker.AgeBiologicalYearsFloat;
            averageAge /= 2;


            float newAge = averageAge * request.outputDef.race.race.lifeExpectancy / firstPawn.RaceProps.lifeExpectancy;

            Faction faction = request.forcedFaction ?? Faction.OfPlayer;

            var pRequest = new PawnGenerationRequest(request.outputDef,faction ,
                                                    PawnGenerationContext.NonPlayer, -1, false,
                                                    false, false, false, true, false, 1f,
                                                    false, true, true, false, false, false,
                                                    false, null, null, null,
                                                    newAge, averageAge);


            Pawn meldToSpawn = PawnGenerator.GeneratePawn(pRequest);

            Pawn_NeedsTracker needs = meldToSpawn.needs;
            needs.food.CurLevel = firstPawn.needs.food.CurLevel;
            needs.rest.CurLevel = firstPawn.needs.rest.CurLevel;
            meldToSpawn.training.SetWantedRecursive(TrainableDefOf.Obedience, true);
            meldToSpawn.training.Train(TrainableDefOf.Obedience, null, true);
            meldToSpawn.Name = firstPawn.Name;
            var meld = (Pawn)GenSpawn.Spawn(meldToSpawn, firstPawn.PositionHeld, firstPawn.MapHeld);
            for (var i = 0; i < 10; i++)
            {
                IntermittentMagicSprayer.ThrowMagicPuffDown(meld.Position.ToVector3(), meld.MapHeld);
                IntermittentMagicSprayer.ThrowMagicPuffUp(meld.Position.ToVector3(), meld.MapHeld);
            }

            meld.SetFaction(Faction.OfPlayer);
            
            HediffDef hediffToAdd = HediffDef.Named(FORMER_HUMAN_HEDIFF);

            Hediff hediff = HediffMaker.MakeHediff(hediffToAdd, meld);
            hediff.Severity = Rand.Range(request.minSeverity, request.maxSeverity);
            meld.health.AddHediff(hediff);

            ReactionsHelper.OnPawnsMerged(firstPawn, firstPawn.IsPrisoner, secondPawn, secondPawn.IsPrisoner, meld);


            TransformerUtility.CleanUpHumanPawnPostTf(firstPawn, null);
            TransformerUtility.CleanUpHumanPawnPostTf(secondPawn, null);

            var inst = new MergedPawns
            {
                originals = request.originals.ToList(), //we want to make a copy here 
                meld = meld,
                mutagenDef = def
            };
            return inst;



        }

        void CheckForBrainDamage(Pawn meld, Pawn human0, Pawn human1)
        {
            RandUtilities.PushState();

            var brains = meld.health.hediffSet.GetNotMissingParts()
                             .Where(p => p.def.tags.Contains(BodyPartTagDefOf.ConsciousnessSource))
                             .ToList();
            if (brains.Count != 2)
            {
                var pawn = Rand.Value < 0.5f ? human0 : human1;
                //var brain = pawn.health.hediffSet.GetBrain();
                DamageInfo dInfo = new DamageInfo(DamageDefOf.Stab, 100, 1);
                pawn.Kill(dInfo);
                
            }

            RandUtilities.PopState();
        }


        /// <summary>
        ///     Tries to revert the transformed pawn instance, implementation.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns></returns>
        protected override bool TryRevertImpl(MergedPawns transformedPawn)
        {
            if (!transformedPawn.IsValid) return false;


            if (transformedPawn.originals.Count != 2) return false;
            HediffDef hDef = HediffDef.Named(FORMER_HUMAN_HEDIFF);
            Pawn meld = transformedPawn.meld;

            Hediff formerHumanHediff = meld?.health?.hediffSet?.hediffs?.FirstOrDefault(h => h.def == hDef);
            if (formerHumanHediff == null) return false;


            PawnRelationDef mergMateDef = DefDatabase<PawnRelationDef>.GetNamed("MergeMate");
            PawnRelationDef mergeMateEx = DefDatabase<PawnRelationDef>.GetNamed("ExMerged"); //TODO put these in a DefOf 


            var firstO = (Pawn) GenSpawn.Spawn(transformedPawn.originals[0], meld.PositionHeld, meld.MapHeld);
            var secondO = (Pawn) GenSpawn.Spawn(transformedPawn.originals[1], meld.PositionHeld, meld.MapHeld);

            var thoughtDef = AddRandomThought(firstO, formerHumanHediff.CurStageIndex);

            AddRandomThought(secondO, formerHumanHediff.CurStageIndex); 

            for (var i = 0; i < 10; i++)
            {
                IntermittentMagicSprayer.ThrowMagicPuffDown(meld.Position.ToVector3(), meld.MapHeld);
                IntermittentMagicSprayer.ThrowMagicPuffUp(meld.Position.ToVector3(), meld.MapHeld);
            }


            PawnRelationDef addDef;

            bool relationIsMergeMate = thoughtDef == def.revertedThoughtGood;
            addDef = relationIsMergeMate ? mergMateDef : mergeMateEx; //first element is "WasMerged"

            firstO.relations.AddDirectRelation(addDef, secondO);

            firstO.SetFaction(Faction.OfPlayer);
            secondO.SetFaction(Faction.OfPlayer);
            //TransformerUtility.RemoveAllMutations(firstO);
            //TransformerUtility.RemoveAllMutations(secondO); 

            ReactionsHelper.OnPawnReverted(firstO, meld);
            ReactionsHelper.OnPawnReverted(secondO, meld);
            CheckForBrainDamage(meld, firstO, secondO);
            meld.DeSpawn();

            return true;
        }

        private ThoughtDef AddRandomThought(Pawn p, int stageNum)
        {
            var traits = p.story.traits;
            ThoughtDef thoughtDef; 
            if (traits.HasTrait(PMTraitDefOf.MutationAffinity))
            {
                thoughtDef= def.revertedThoughtGood;
            }else if (traits.HasTrait(TraitDefOf.BodyPurist))
            {
                thoughtDef= def.revertedThoughtBad; 
            }else
                thoughtDef = Rand.Value < 0.5 ? def.revertedThoughtGood : def.revertedThoughtBad;

            var memories = p.needs.mood.thoughts.memories;
            var thought = ThoughtMaker.MakeThought(thoughtDef, stageNum);
            memories.TryGainMemory(thought);
            return thoughtDef; 
        }

        /// <summary>
        ///     Tries to revert the given pawn.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns></returns>
        public override bool TryRevert(Pawn transformedPawn)
        {
            if (transformedPawn == null)
            {
                throw new ArgumentNullException(nameof(transformedPawn)); 
            }

            Tuple<TransformedPawn, TransformedStatus> status = GameComp.GetTransformedPawnContaining(transformedPawn);

            if (status != null)
            {
                if (status.Second != TransformedStatus.Transformed) return false;

                if (status.First is MergedPawns merged)
                {
                    if (merged.mutagenDef != def) return false;

                    if (TryRevertImpl(merged))
                    {
                        GameComp.RemoveInstance(merged);
                        return true; 
                    }
                }

                return false;
            }

            HediffDef formerDef = HediffDef.Named(FORMER_HUMAN_HEDIFF);
            if (transformedPawn.health.hediffSet.HasHediff(formerDef))
            {
                Hediff formerHediff = transformedPawn.health.hediffSet.hediffs.First(h => h.def == formerDef);
                ThoughtDef thoughtDef = null; 

                for (var i = 0; i < 2; i++)
                {
                    PawnGenerationRequest request = TransformerUtility.GenerateRandomPawnFromAnimal(transformedPawn);
                    Pawn pawnTf = PawnGenerator.GeneratePawn(request);
                    pawnTf.needs.food.CurLevel = transformedPawn.needs.food.CurInstantLevel;
                    pawnTf.needs.rest.CurLevel = transformedPawn.needs.rest.CurLevel;

                    var spawnedPawn = (Pawn) GenSpawn.Spawn(pawnTf, transformedPawn.PositionHeld, transformedPawn.MapHeld);
                    spawnedPawn.apparel.DestroyAll();
                    spawnedPawn.equipment.DestroyAllEquipment();
                    for (var j = 0; j < 10; j++)
                    {
                        IntermittentMagicSprayer.ThrowMagicPuffDown(spawnedPawn.Position.ToVector3(), spawnedPawn.MapHeld);
                        IntermittentMagicSprayer.ThrowMagicPuffUp(spawnedPawn.Position.ToVector3(), spawnedPawn.MapHeld);
                    }

                    thoughtDef = AddRandomThought(spawnedPawn, formerHediff.CurStageIndex); 
                    _scratchArray[i] = spawnedPawn;
                }
                PawnRelationDef relationDef;
                bool relationIsMergeMate = thoughtDef == def.revertedThoughtGood;
                relationDef = relationIsMergeMate ? TfRelationDefOf.MergeMate : TfRelationDefOf.ExMerged;
                _scratchArray[0].relations.AddDirectRelation(relationDef, _scratchArray[1]);

                CheckForBrainDamage(transformedPawn, _scratchArray[0], _scratchArray[1]); 
                transformedPawn.Destroy();

               

                return true;
            }

            return false;
        }

        private readonly Pawn[] _scratchArray = new Pawn[2];
    }
}