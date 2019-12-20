// SimpleMechaniteMutagen.cs modified by Iron Wolf Pawnmorph on 08/14/2019 4:07 PM
// last updated 08/15/2019  1:40 PM

using System;
using System.Linq;
using Pawnmorph.DebugUtils;
using Pawnmorph.Thoughts;
using Pawnmorph.Utilities;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Pawnmorph.TfSys
{
    /// <summary>
    ///     simple implementation of Mutagen that just transforms a single pawn into a single animal
    /// </summary>
    /// <seealso cref="TransformedPawnSingle" />
    public class SimpleMechaniteMutagen : Mutagen<TransformedPawnSingle>
    {
        /// <summary>
        ///     Determines whether this instance can transform the specified pawn.
        /// </summary>
        /// <param name="pawn">The pawns.</param>
        public override bool CanTransform(Pawn pawn)
        {
            return CanInfect(pawn);
        }


        /// <summary>
        ///     Tries to revert the given pawn.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns></returns>
        public override bool TryRevert(Pawn transformedPawn)
        {
            Tuple<TransformedPawn, TransformedStatus> status = GameComp.GetTransformedPawnContaining(transformedPawn);
            if (status != null)
            {
                if (status.Second != TransformedStatus.Transformed) return false;
                if (status.First is TransformedPawnSingle inst)
                    if (TryRevertImpl(inst))
                    {
                        GameComp.RemoveInstance(inst);
                        return true;
                    }

                return false;
            }

            Hediff formerHuman =
                transformedPawn.health.hediffSet.hediffs.FirstOrDefault(h => h.def == TfHediffDefOf.TransformedHuman);
            if (formerHuman == null) return false;

            PawnGenerationRequest request = TransformerUtility.GenerateRandomPawnFromAnimal(transformedPawn);
            Pawn pawnTf = PawnGenerator.GeneratePawn(request);

            pawnTf.needs.food.CurLevel = transformedPawn.needs.food.CurLevel;
            pawnTf.needs.rest.CurLevel = transformedPawn.needs.rest.CurLevel;

            var spawned = (Pawn) GenSpawn.Spawn(pawnTf, transformedPawn.PositionHeld, transformedPawn.MapHeld);
            spawned.equipment.DestroyAllEquipment();
            spawned.apparel.DestroyAll();


            for (var i = 0; i < 10; i++)
            {
                IntermittentMagicSprayer.ThrowMagicPuffDown(spawned.Position.ToVector3(), spawned.MapHeld);
                IntermittentMagicSprayer.ThrowMagicPuffUp(spawned.Position.ToVector3(), spawned.MapHeld);
            }


            AddReversionThought(spawned, formerHuman.CurStageIndex);



            transformedPawn.Destroy();
            return true;
        }

        /// <summary>
        ///     Determines whether this instance can revert pawn the specified transformed pawn.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns>
        ///     <c>true</c> if this instance can revert pawn  the specified transformed pawn; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanRevertPawnImp(TransformedPawnSingle transformedPawn)
        {
            if (!transformedPawn.IsValid) return false;
            return transformedPawn.animal.health.hediffSet.HasHediff(TfHediffDefOf.TransformedHuman);
        }

        /// <summary>Returns true if the specified request is valid.</summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// <c>true</c> if the specified request is valid; otherwise, <c>false</c>.
        /// </returns>
        protected override bool IsValid(TransformationRequest request)
        {
            return base.IsValid(request) && request.originals.Length == 1;
        }

        /// <summary>
        ///     preform the requested transform
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        protected override TransformedPawnSingle TransformImpl(TransformationRequest request)
        {
            Pawn original = request.originals[0];

            float newAge = TransformerUtility.ConvertAge(original, request.outputDef.race.race);

            Faction faction;
            if (request.forcedFaction != null) //forced faction should be the highest priority if set 
                faction = request.forcedFaction;
            else if (original.IsColonist)
                faction = original.Faction;
            else
                faction = null;

            Gender newGender =
                TransformerUtility.GetTransformedGender(original, request.forcedGender, request.forcedGenderChance);

            var pRequest = new PawnGenerationRequest( //create the request 
                                                     request.outputDef, faction, PawnGenerationContext.NonPlayer, -1, false,
                                                     false,
                                                     false, false, true, false, 1f, false, true, true, false, false, false,
                                                     false, null, null, null, newAge,
                                                     original.ageTracker.AgeChronologicalYearsFloat, newGender);


            Pawn animalToSpawn = PawnGenerator.GeneratePawn(pRequest); //make the temp pawn 


            animalToSpawn.needs.food.CurLevel =
                original.needs.food.CurLevel; // Copies the original pawn's food need to the animal's.
            animalToSpawn.needs.rest.CurLevel =
                original.needs.rest.CurLevel; // Copies the original pawn's rest need to the animal's.
            animalToSpawn.Name = original.Name; // Copies the original pawn's name to the animal's.



            Pawn spawnedAnimal = SpawnAnimal(original, animalToSpawn); // Spawns the animal into the map.
            bool wasPrisoner = original.IsPrisonerOfColony;
            ReactionsHelper.OnPawnTransforms(original, animalToSpawn, wasPrisoner); //this needs to happen before MakeSapientAnimal because that removes relations 

            FormerHumanUtilities.MakeAnimalSapient(original, spawnedAnimal, Rand.Range(0.4f, 1)); //use a normal distribution? 
            
            var inst = new TransformedPawnSingle
            {
                original = original,
                animal = spawnedAnimal
            };


            if (original.Spawned)
                for (var i = 0; i < 10; i++) // Create a cloud of magic.
                {
                    IntermittentMagicSprayer.ThrowMagicPuffDown(spawnedAnimal.Position.ToVector3(), spawnedAnimal.MapHeld);
                    IntermittentMagicSprayer.ThrowMagicPuffUp(spawnedAnimal.Position.ToVector3(), spawnedAnimal.MapHeld);
                }

            if (request.tale != null) // If a tale was provided, push it to the tale recorder.
                TaleRecorder.RecordTale(request.tale, original, animalToSpawn);

            Faction oFaction = original.Faction;
            Map oMap = original.Map;

            
            //apply apparel damage 
            ApplyApparelDamage(original, spawnedAnimal.def);

            FormerHumanUtilities.TryAssignBackstoryToTransformedPawn(spawnedAnimal, original);
            TransformerUtility
               .CleanUpHumanPawnPostTf(original, request.cause); //now clean up the original pawn (remove apparel, drop'em, ect) 

            //notify the faction that their member has been transformed 
            oFaction.Notify_MemberTransformed(original, spawnedAnimal, oMap == null, oMap);

            if(original.Faction.IsPlayer || wasPrisoner) //only send the letter for colonists and prisoners 
                SendLetter(request, original, spawnedAnimal);

            if (original.Spawned)
                original.DeSpawn(); // Remove the original pawn from the current map.

            DebugLogUtils.Assert(!PrisonBreakUtility.CanParticipateInPrisonBreak(original),
                                 $"{original.Name} has been cleaned up and de-spawned but can still participate in prison breaks");


            return inst;
        }

        private static void SendLetter(TransformationRequest request, Pawn original, Pawn spawnedAnimal)
        {
            Find.LetterStack
                .ReceiveLetter("LetterHediffFromTransformationLabel".Translate(original.LabelShort, request.outputDef.LabelCap).CapitalizeFirst(),
                               "LetterHediffFromTransformation"
                                  .Translate(original.LabelShort, request.outputDef.LabelCap)
                                  .CapitalizeFirst(),
                               LetterDefOf.NeutralEvent,
                               spawnedAnimal); // Creates a letter saying "Oh no! Pawn X has transformed into a Y!"
            Find.TickManager.slower.SignalForceNormalSpeedShort(); // Slow down the speed of the game.
        }


        /// <summary>
        ///     Tries to revert the transformed pawn instance, implementation.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns></returns>
        protected override bool TryRevertImpl(TransformedPawnSingle transformedPawn)
        {
            if (transformedPawn == null) throw new ArgumentNullException(nameof(transformedPawn));
            if (!transformedPawn.IsValid) return false;


            Pawn animal = transformedPawn.animal;

            Hediff tfHumanHediff = animal?.health?.hediffSet?.GetFirstHediffOfDef(TfHediffDefOf.TransformedHuman);
            if (tfHumanHediff == null) return false;

            var spawned = (Pawn) GenSpawn.Spawn(transformedPawn.original, animal.PositionHeld, animal.MapHeld);

            for (var i = 0; i < 10; i++)
            {
                IntermittentMagicSprayer.ThrowMagicPuffDown(spawned.Position.ToVector3(), spawned.MapHeld);
                IntermittentMagicSprayer.ThrowMagicPuffUp(spawned.Position.ToVector3(), spawned.MapHeld);
            }

            FormerHumanUtilities.TransferRelations(animal, spawned, r => r != PawnRelationDefOf.Bond); //transfer whatever relations from the animal to the human pawn 
                                                                                            //do NOT transfer the bond relationship to humans, Rimworld doesn't like that 
            AddReversionThought(spawned, tfHumanHediff.CurStageIndex);

            spawned.Faction.Notify_MemberReverted(spawned, animal, spawned.Map == null, spawned.Map);

            ReactionsHelper.OnPawnReverted(spawned, animal);


            animal.Destroy();
            return true;
        }

        /// <summary>
        ///     add the correct reversion thought at the correct stage
        /// </summary>
        /// <param name="spawned"></param>
        /// <param name="curStageIndex"></param>
        private void AddReversionThought(Pawn spawned, int curStageIndex)
        {
            TraitSet traits = spawned.story.traits;
            ThoughtDef thoughtDef;
            var hasPrimalWish = spawned.GetAspectTracker()?.Contains(AspectDefOf.PrimalWish) == true;
            if (hasPrimalWish)
                thoughtDef = def.revertedPrimalWish ?? def.revertedThoughtBad; //substitute with the bad thought if null 
            else if (traits.HasTrait(PMTraitDefOf.MutationAffinity))
                thoughtDef = def.revertedThoughtGood;
            else if (traits.HasTrait(TraitDefOf.BodyPurist))
                thoughtDef = def.revertedThoughtBad;
            else
                thoughtDef = Rand.Value > 0.5f ? def.revertedThoughtGood : def.revertedThoughtBad;

            if (thoughtDef != null)
            {
                curStageIndex = Mathf.Min(curStageIndex, thoughtDef.stages.Count - 1); //avoid index out of bounds issues 
                Thought_Memory mem = ThoughtMaker.MakeThought(thoughtDef, curStageIndex);
                spawned.TryGainMemory(mem);
            }
        }

        private static Pawn SpawnAnimal(Pawn original, Pawn animalToSpawn)
        {
            if (original.IsCaravanMember())
            {
                original.GetCaravan().AddPawn(animalToSpawn, true);
                Find.WorldPawns.PassToWorld(animalToSpawn);
                return animalToSpawn;
            }

            if (original.IsWorldPawn())
            {
                Find.WorldPawns.PassToWorld(animalToSpawn);
                return animalToSpawn;
            }

            return (Pawn) GenSpawn.Spawn(animalToSpawn, original.PositionHeld, original.MapHeld);
        }
    }
}