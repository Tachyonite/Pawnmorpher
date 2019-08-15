// SimpleMechaniteMutagen.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/14/2019 4:07 PM
// last updated 08/15/2019  1:40 PM

using System;
using System.Collections.Generic;
using Pawnmorph.Hybrids;
using Pawnmorph.Thoughts;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.TfSys
{
    /// <summary>
    ///     simple implementation of Mutagen that just transforms a single pawn into a single animal
    /// </summary>
    /// <seealso cref="TransformedPawnSingle" />
    public class SimpleMechaniteMutagen : Mutagen<TransformedPawnSingle>
    {
        private static List<Pawn> _scratchList = new List<Pawn>();

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

        /// <summary>
        ///     Transforms the pawns into a TransformedPawn instance of the given ace .
        /// </summary>
        /// <param name="originals">The originals.</param>
        /// <param name="outputPawnKind"></param>
        /// <param name="forcedGender"></param>
        /// <param name="forcedGenderChance"></param>
        /// <param name="cause">The cause.</param>
        /// <param name="tale"></param>
        /// <returns></returns>
        protected override TransformedPawnSingle TransformPawnsImpl(IEnumerable<Pawn> originals,
            PawnKindDef outputPawnKind, TFGender forcedGender,
            float forcedGenderChance, Hediff_Morph cause, TaleDef tale)
        {
            _scratchList.Clear();
            _scratchList.AddRange(originals);
            if (_scratchList.Count > 1)
            {
                Log.Warning(
                    $"Mutagen {def.defName} is trying to transform more then one pawn into a meld but this is not supported by {nameof(SimpleMechaniteMutagen)}");
                return null;
            }

            if (_scratchList.Count == 0)
            {
                Log.Warning($"Mutagen {def.defName} received an empty enumeration of pawns to transform!");
                return null;
            }

            var original = _scratchList[0];

            var newAge = TransformerUtility.ConvertAge(original, outputPawnKind.race.race);

            var faction = original.IsColonist ? original.Faction : null;

            var newGender = TransformerUtility.GetTransformedGender(original, forcedGender, forcedGenderChance);

            var request = new PawnGenerationRequest( //create the request 
                outputPawnKind, faction, PawnGenerationContext.NonPlayer, -1, false, false,
                false, false, true, false, 1f, false, true, true, false, false, false,
                false, null, null, null, newAge, original.ageTracker.AgeChronologicalYearsFloat, newGender);


            var animalToSpawn = PawnGenerator.GeneratePawn(request); //make the temp pawn 


            animalToSpawn.needs.food.CurLevel = original.needs.food.CurLevel; // Copies the original pawn's food need to the animal's.
            animalToSpawn.needs.rest.CurLevel = original.needs.rest.CurLevel; // Copies the original pawn's rest need to the animal's.
            animalToSpawn.Name = original.Name; // Copies the original pawn's name to the animal's.

            if (animalToSpawn.Faction != null)
            {
                animalToSpawn.training.SetWantedRecursive(TrainableDefOf.Obedience, true); // Sets obediance training to on for the animal.
                animalToSpawn.training.Train(TrainableDefOf.Obedience, null, true); // Sets the animal's obedience to be fully trained.
            }

            Pawn spawnedAnimal = (Pawn)GenSpawn.Spawn(animalToSpawn, original.PositionHeld, original.MapHeld, 0); // Spawns the animal into the map.
            Hediff hediff = HediffMaker.MakeHediff(TfHediffDefOf.TransformedHuman, spawnedAnimal, null); // Create a hediff from the one provided (i.e. TransformedHuman)...
            hediff.Severity = Rand.Range(0.00f, 1.00f); // ...give it a random severity...
            spawnedAnimal.health.AddHediff(hediff); // ...and apply it to the new animal.

            var inst = new TransformedPawnSingle()
            {
                original = original,
                animal = spawnedAnimal
            };



            for (int i = 0; i < 10; i++) // Create a cloud of magic.
            {
                IntermittentMagicSprayer.ThrowMagicPuffDown(spawnedAnimal.Position.ToVector3(), spawnedAnimal.MapHeld);
                IntermittentMagicSprayer.ThrowMagicPuffUp(spawnedAnimal.Position.ToVector3(), spawnedAnimal.MapHeld);
            }

            if (tale != null) // If a tale was provided, push it to the tale recorder.
            {
                TaleRecorder.RecordTale(tale, new object[]
                {
                    original,
                    animalToSpawn
                });
            }

            bool wasPrisoner = original.IsPrisonerOfColony;
            TransformerUtility.CleanUpHumanPawnPostTf(original, cause);  //now clean up the original pawn (remove apparel, drop'em, ect) 

            Find.LetterStack.ReceiveLetter("LetterHediffFromTransformationLabel".Translate(original.LabelShort, outputPawnKind.LabelCap).CapitalizeFirst(),
                "LetterHediffFromTransformation".Translate(original.LabelShort, outputPawnKind.LabelCap).CapitalizeFirst(),
                LetterDefOf.NeutralEvent, spawnedAnimal, null, null); // Creates a letter saying "Oh no! Pawn X has transformed into a Y!"
            Find.TickManager.slower.SignalForceNormalSpeedShort(); // Slow down the speed of the game.

            original.DeSpawn(); // Remove the original pawn from the current map.

            ReactionsHelper.OnPawnTransforms(original, animalToSpawn, wasPrisoner);

            return inst; 
            
        }

        /// <summary>
        ///     Tries to revert the transformed pawn instance, implementation.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns></returns>
        protected override bool TryRevertImpl(TransformedPawnSingle transformedPawn)
        {
            if (!transformedPawn.IsValid) return false;

            if (!transformedPawn.animal.health.hediffSet.HasHediff(TfHediffDefOf.TransformedHuman)) return false;

            var animal = transformedPawn.animal;
            var spawned = (Pawn) GenSpawn.Spawn(transformedPawn.original, animal.PositionHeld, animal.MapHeld);

            for (int i = 0; i < 10; i++)
            {
                IntermittentMagicSprayer.ThrowMagicPuffDown(spawned.Position.ToVector3(), spawned.MapHeld);
                IntermittentMagicSprayer.ThrowMagicPuffUp(spawned.Position.ToVector3(), spawned.MapHeld);
            }

            if (def.reversionThoughts.Count > 0)
            {
                var hediff = HediffMaker.MakeHediff(def.reversionThoughts.RandElement(), spawned);
                var tfHumanHediff = animal.health.hediffSet.GetFirstHediffOfDef(TfHediffDefOf.TransformedHuman);
                hediff.Severity = tfHumanHediff.Severity;

                spawned.health.AddHediff(hediff); 
            }

            ReactionsHelper.OnPawnReverted(spawned, animal);

            if (spawned.IsHybridRace())
                RaceShiftUtilities.RevertPawnToHuman(spawned);


            animal.Destroy();
            return true;

        }
    }
}