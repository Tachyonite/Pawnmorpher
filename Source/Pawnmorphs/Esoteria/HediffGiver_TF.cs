using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Pawnmorph
{
    public class HediffGiver_TF : HediffGiver
    {
        public float chance = LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().transformChance; // The current chance for a full transformation as determined by the settings.
        public PawnKindDef pawnkind; // The pawnKind of the animal to be transformed into.
        public TaleDef tale; // Tale to add to the tales.
        public TFGender forceGender; // The gender that will be forced (i.e. a ChookMorph will be forced female).
        public float forceGenderChance = 50; // If forceGender is provided, this is the chance the gender will be forced.
        private bool triggered = false; // A flag to prevent us from checking endlessly.

        public override void OnIntervalPassed(Pawn transformedPawn, Hediff cause)
        // Whenever the timer expires.
        {
            if (transformedPawn.Map == null) // If the pawn is not currently in the world (i.e. is on caravan)...
            {
                transformedPawn.health.RemoveHediff(cause); // ...remove the hediff that would otherwise cause a transformation.
                // (We do it this way because it's a little hard to check for and this keeps the hediff from erroring out.)
            }
            // && Rand.MTBEventOccurs(mtbDays, 60000f, 60f) 
            else if (!triggered && transformedPawn.RaceProps.intelligence == Intelligence.Humanlike && Rand.RangeInclusive(0, 100) <= chance)
            // If we haven't already checked for the pawn to be tf'd and it possesses humanlike intellegence, give it a chance to transform.
            {
                float animalAge = pawnkind.race.race.lifeExpectancy * transformedPawn.ageTracker.AgeBiologicalYears / transformedPawn.def.race.lifeExpectancy;
                // The animal is the same percent of the way through it's life as the source pawn is.

                Gender animalGender = transformedPawn.gender;
                if (forceGender != null && Rand.RangeInclusive(0, 100) <= forceGenderChance)
                // If forceGender was provided, give it a chance to be applied.
                {
                    if (forceGender == TFGender.Male)
                    {
                        animalGender = Gender.Male;
                    }
                    else if (forceGender == TFGender.Female)
                    {
                        animalGender = Gender.Female;
                    }
                    else if (forceGender == TFGender.Switch)
                    {
                        if (transformedPawn.gender == Gender.Male)
                        {
                            animalGender = Gender.Female;
                        }
                        else if (transformedPawn.gender == Gender.Female)
                        {
                            animalGender = Gender.Male;
                        }
                    }
                }

                Pawn animalToSpawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                    pawnkind, transformedPawn.Faction, PawnGenerationContext.NonPlayer, -1, false, false,
                    false, false, true, false, 1f, false, true, true, false, false, false,
                    false, null, null, null, animalAge, transformedPawn.ageTracker.AgeChronologicalYearsFloat, animalGender));
                // Creates a new animal of pawnkind type, with some of its stats set as those calculated above.

                if (tale != null) // If a tale was provided, push it to the tale recorder.
                {
                    TaleRecorder.RecordTale(tale, new object[]
                    {
                            transformedPawn,
                            animalToSpawn
                    });
                }

                animalToSpawn.needs.food.CurLevel = transformedPawn.needs.food.CurLevel; // Copies the original pawn's food need to the animal's.
                animalToSpawn.needs.rest.CurLevel = transformedPawn.needs.rest.CurLevel; // Copies the original pawn's rest need to the animal's.
                animalToSpawn.training.SetWantedRecursive(TrainableDefOf.Obedience, true); // Sets obediance training to on for the animal.
                animalToSpawn.training.Train(TrainableDefOf.Obedience, null, true); // Sets the animal's obedience to be fully trained.
                animalToSpawn.Name = transformedPawn.Name; // Copies the original pawn's name to the animal's.

                Pawn spawnedAnimal = (Pawn)GenSpawn.Spawn(animalToSpawn, transformedPawn.PositionHeld, transformedPawn.MapHeld, 0); // Spawns the animal into the map.

                for (int i = 0; i < 10; i++) // Create a cloud of magic.
                {
                    IntermittentMagicSprayer.ThrowMagicPuffDown(spawnedAnimal.Position.ToVector3(), spawnedAnimal.MapHeld);
                    IntermittentMagicSprayer.ThrowMagicPuffUp(spawnedAnimal.Position.ToVector3(), spawnedAnimal.MapHeld);
                }

                Find.LetterStack.ReceiveLetter("LetterHediffFromTransformationLabel".Translate(transformedPawn.LabelShort, pawnkind.LabelCap).CapitalizeFirst(),
                    "LetterHediffFromTransformation".Translate(transformedPawn.LabelShort, pawnkind.LabelCap).CapitalizeFirst(),
                    LetterDefOf.NeutralEvent, spawnedAnimal, null, null); // Creates a letter saying "Oh no! Pawn X has transformed into a Y!"
                transformedPawn.apparel.DropAll(transformedPawn.PositionHeld); // Makes the original pawn drop all apparel...
                transformedPawn.equipment.DropAllEquipment(transformedPawn.PositionHeld); // ... and equipment (i.e. guns).

                if (transformedPawn.RaceProps.intelligence == Intelligence.Humanlike) // If the original pawn possesed humanlike intelligence...
                {
                    Hediff hediff = HediffMaker.MakeHediff(base.hediff, spawnedAnimal, null); // ...create a hediff from the one provided (i.e. TransformedHuman)...
                    hediff.Severity = Rand.Range(0.00f, 1.00f); // ...give it a random severity...
                    spawnedAnimal.health.AddHediff(hediff); // ...and apply it to the new animal.
                }

                transformedPawn.health.RemoveHediff(cause); // Remove the hediff that caused the transformation so they don't transform again if reverted.
                PawnMorphInstance pm = new PawnMorphInstance(transformedPawn, spawnedAnimal); // Put the original pawn somewhere safe and tie it to the animal.
                Find.World.GetComponent<PawnmorphGameComp>().addPawn(pm); // ...and put this data somewhere safe.

                if (transformedPawn.ownership.OwnedBed != null) // If the original pawn owned a bed somewhere...
                {
                    transformedPawn.ownership.UnclaimBed(); // ...unclaim it.
                }

                if (transformedPawn.CarriedBy != null) // If the original pawn was being carried when they transformed...
                {
                    Pawn carryingPawn = transformedPawn.CarriedBy;
                    Thing outPawn;
                    carryingPawn.carryTracker.TryDropCarriedThing(carryingPawn.Position, ThingPlaceMode.Direct, out outPawn); // ...drop them so they can be removed.

                }

                transformedPawn.DeSpawn(); // Remove the original pawn from the current map.
                Find.TickManager.slower.SignalForceNormalSpeedShort();
            }
            else
            {
                triggered = true;
            }
        }
    }
}
