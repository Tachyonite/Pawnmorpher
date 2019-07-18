using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Pawnmorph
{
    public class HediffGiver_TFRandom : HediffGiver
    {
        public string pawnkind;
        public List<string> pawnkinds;
        public float chance = LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().transformChance;
        public float mtbDays;
        public string forceGender = "Original";
        public float forceGenderChance = 50;
        private bool triggered = false;
        private string hediffDef = "TransformedHuman";
        public string faction;
        public MentalStateDef mentalState = null;
        public float mentalStateChance;



        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (((Rand.RangeInclusive(0, 100) <= chance || chance == 100) && !triggered) && (mtbDays == 0.0f || Rand.MTBEventOccurs(mtbDays, 60000f, 60f)) && pawn.RaceProps.intelligence == Intelligence.Humanlike)
            {

                pawnkind = pawnkinds.RandomElement();

                PawnKindDef pawnTFKind = PawnKindDef.Named(pawnkind);

                Gender newGender = pawn.gender;

                if (Rand.RangeInclusive(0, 100) <= forceGenderChance)
                {
                    switch (forceGender)
                    {
                        case ("Male"):
                            newGender = Gender.Male;
                            break;
                        case ("Female"):
                            newGender = Gender.Female;
                            break;
                        case ("Switch"):
                            switch (pawn.gender)
                            {
                                case (Gender.Male):
                                    newGender = Gender.Female;
                                    break;
                                case (Gender.Female):
                                    newGender = Gender.Male;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
                float humanLE = pawn.def.race.lifeExpectancy;
                float animalLE = pawnTFKind.race.race.lifeExpectancy;
                float humanAge = pawn.ageTracker.AgeBiologicalYears;

                float animalDelta = humanAge / humanLE;
                float animalAge = animalLE * animalDelta;

                Pawn pawnTF = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnTFKind, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 1f, false, true, true, false, false, false, false, null, null, null, new float?(animalAge), new float?(pawn.ageTracker.AgeChronologicalYearsFloat), new Gender?(newGender), null, null));

                pawnTF.needs.food.CurLevel = pawn.needs.food.CurLevel;
                pawnTF.needs.rest.CurLevel = pawn.needs.rest.CurLevel;
                pawnTF.training.SetWantedRecursive(TrainableDefOf.Obedience, true);
                pawnTF.training.Train(TrainableDefOf.Obedience, null, true);
                pawnTF.Name = pawn.Name;
                Pawn pawn3 = (Pawn)GenSpawn.Spawn(pawnTF, pawn.PositionHeld, pawn.MapHeld, 0);
                for (int i = 0; i < 10; i++)
                {
                    IntermittentMagicSprayer.ThrowMagicPuffDown(pawn3.Position.ToVector3(), pawn3.MapHeld);
                    IntermittentMagicSprayer.ThrowMagicPuffUp(pawn3.Position.ToVector3(), pawn3.MapHeld);
                }

                if (faction == "wild")
                {
                    pawn3.SetFaction(null, null);
                }

                if (pawn.Faction == Faction.OfPlayer)
                {
                    pawn3.SetFaction(Faction.OfPlayer, null);
                }
                Find.LetterStack.ReceiveLetter("LetterHediffFromTransformationLabel".Translate(pawn.LabelShort, pawnTFKind.LabelCap).CapitalizeFirst(), "LetterHediffFromTransformation".Translate(pawn.LabelShort, pawnTFKind.LabelCap).CapitalizeFirst(), LetterDefOf.NeutralEvent, pawn, null, null);
                pawn.apparel.DropAll(pawn.PositionHeld);
                pawn.equipment.DropAllEquipment(pawn.PositionHeld);
                pawn.DropAndForbidEverything();
                if (pawn.RaceProps.intelligence == Intelligence.Humanlike)
                {
                    Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named(hediffDef), pawn3, null);
                    hediff.Severity = Rand.Range(0.00f, 1.00f);
                    pawn3.health.AddHediff(hediff, null, null, null);

                }/*
                if (TransformerUtility.TryGivePostTransformationBondRelation(ref pawn3, pawn, out Pawn otherPawn))
                {
                    Find.LetterStack.ReceiveLetter("LetterHediffFromTransformationBondLabel".Translate(pawn.LabelShort, pawnTFKind.LabelCap).CapitalizeFirst(), "LetterHediffFromTransformationBond".Translate(pawn.LabelShort, pawnTFKind.LabelCap, otherPawn.LabelShort).CapitalizeFirst(), LetterDefOf.NeutralEvent, pawn, null, null);
                }
                else
                {

                }*/
                pawn.health.RemoveHediff(cause);
                PawnMorphInstance pm = new PawnMorphInstance(pawn, pawn3); //pawn is human, pawn3 is animal
                Find.World.GetComponent<PawnmorphGameComp>().addPawn(pm);
                if (pawn.ownership.OwnedBed != null)
                {
                    pawn.ownership.UnclaimAll();
                }
                if (pawn.CarriedBy != null)
                {
                    Pawn carryingPawn = pawn.CarriedBy;
                    Thing outPawn;
                    carryingPawn.carryTracker.TryDropCarriedThing(carryingPawn.Position, ThingPlaceMode.Direct, out outPawn);

                }
                pawn.DeSpawn();

                Find.TickManager.slower.SignalForceNormalSpeedShort();
            }
            else { triggered = true; }
        }
    }
}
