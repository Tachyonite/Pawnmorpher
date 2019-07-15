using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using static RimWorld.MoteMaker;
using RimWorld;
using Multiplayer.API;

namespace Pawnmorph
{
    public static class PawnmorphHediffGiverUtility
    {
        public static bool TryApply(Pawn pawn, HediffDef hediff, List<BodyPartDef> partsToAffect = null, bool canAffectAnyLivePart = false, int countToAffect = 1, List<Hediff> outAddedHediffs = null)
        {
            try
            {
                if (canAffectAnyLivePart || partsToAffect != null)
                {
                    bool result = false;
                    for (int i = 0; i < countToAffect; i++)
                    {

                        IEnumerable<BodyPartRecord> source = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null);
                        if (partsToAffect != null)
                        {
                            source = from p in source
                                     where partsToAffect.Contains(p.def)
                                     select p;
                        }
                        if (canAffectAnyLivePart)
                        {
                            source = from p in source
                                     where p.def.alive
                                     select p;
                        }
                        source = from p in source
                                 where !pawn.health.hediffSet.HasHediff(hediff, p, false) && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(p)
                                 select p;
                        if (!source.Any<BodyPartRecord>())
                        {
                            break;
                        }

                        BodyPartRecord partRecord = source.First();
                        Hediff hediff2 = HediffMaker.MakeHediff(hediff, pawn, partRecord);
                        pawn.health.AddHediff(hediff2, null, null, null);
                        if (outAddedHediffs != null)
                        {
                            outAddedHediffs.Add(hediff2);
                        }
                        result = true;
                    }
                    return result;
                }
                if (!pawn.health.hediffSet.HasHediff(hediff, false))
                {
                    Hediff hediff3 = HediffMaker.MakeHediff(hediff, pawn, null);
                    pawn.health.AddHediff(hediff3, null, null, null);
                    if (outAddedHediffs != null)
                    {
                        outAddedHediffs.Add(hediff3);
                    }
                    return true;
                }
                return false;
            }
            catch { return false; }
        }
    }

    public class HediffGiver_Esoteric : HediffGiver
    {
        public float mtbDays;
        public bool once = false;
        private bool triggered = false;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            try
            {
                if (pawn.RaceProps.intelligence == Intelligence.Humanlike)
                {
                    if (Rand.MTBEventOccurs(this.mtbDays, 60000f, 60f) && base.TryApply(pawn) && ((!triggered && once) || !once))
                    {
                        IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);
                        if (once)
                        {
                            triggered = true;
                        }
                        if (cause.def.HasComp(typeof(HediffComp_Single)))
                        {

                            pawn.health.RemoveHediff(cause);

                        }
                    }
                }
            }
            catch { }

            
        }
    }

    public class HediffGiver_Esoteric_RandomList : HediffGiver
    {

        public List<HediffDef> hediffDefs;
        public List<HediffDef> hediffDefsComplete;
        public float completeChance;
        private HediffDef hediffDef;

        public float severity = -1f;

        public ChemicalDef toleranceChemical;

        public bool divideByBodySize = false;

        public float mtbDays;
        public bool once = false;
        private bool triggered = false;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {

            try
            {
                if (Rand.RangeInclusive(0, 100) <= completeChance)
                {

                    hediffDef = hediffDefsComplete.RandomElement();

                }
                else
                {
                    hediffDef = hediffDefs.RandomElement();

                }

                Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn, null);
                float num;
                if (this.severity > 0f)
                {
                    num = this.severity;
                }
                else
                {
                    num = hediffDef.initialSeverity;
                }
                if (this.divideByBodySize)
                {
                    num /= pawn.BodySize;
                }
                AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, this.toleranceChemical, ref num);
                hediff.Severity = num;
                pawn.health.AddHediff(hediff, null, null, null);
            }
            catch { }


        }

    }

    public class HediffGiver_Esoteric_Fixed : HediffGiver
    {
        public float mtbDays;
        public List<BodyPartDef> fixedParts;
        public TaleDef tale;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            try
            {
                if (Rand.MTBEventOccurs(this.mtbDays, 60000f, 60f) && PawnmorphHediffGiverUtility.TryApply(pawn, hediff, fixedParts) && pawn.RaceProps.intelligence == Intelligence.Humanlike)
                {
                    IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);
                    if (cause.def.HasComp(typeof(HediffComp_Single)))
                    {

                        pawn.health.RemoveHediff(cause);

                    }
                    if (tale != null)
                    {
                        TaleRecorder.RecordTale(tale, new object[]
                        {
                            pawn
                        });
                    }
                }
            }
            catch { }
            
        }
    }


    public class HediffGiver_Esoteric_GenderChance : HediffGiver
    {
        public float mtbDays;
        public Gender gender;
        public int chance;
        private bool triggered = false;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            try
            {
                if (pawn.gender == gender || (Rand.RangeInclusive(0, 100) <= chance && !triggered) && pawn.RaceProps.intelligence == Intelligence.Humanlike)
                {
                    if (Rand.MTBEventOccurs(this.mtbDays, 60000f, 60f) && base.TryApply(pawn, null))
                    {
                        IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);
                        if (cause.def.HasComp(typeof(HediffComp_Single)))
                        {

                            pawn.health.RemoveHediff(cause);

                        }
                    }
                }
                else { triggered = false; }

            }
            catch { }
        }
    }

    public class HediffGiver_Esoteric_GenderChanceFixed : HediffGiver
    {
        public float mtbDays;
        public Gender gender;
        public int chance;
        private bool triggered = false;
        public List<BodyPartDef> fixedParts;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            try
            {
                if (pawn.gender == gender || (Rand.RangeInclusive(0, 100) <= chance && !triggered) && pawn.RaceProps.intelligence == Intelligence.Humanlike)
                {
                    if (Rand.MTBEventOccurs(this.mtbDays, 60000f, 60f) && PawnmorphHediffGiverUtility.TryApply(pawn, hediff, fixedParts))
                    {
                        IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);
                        if (cause.def.HasComp(typeof(HediffComp_Single)))
                        {

                            pawn.health.RemoveHediff(cause);

                        }
                    }
                }
                else { triggered = false; }

            }
            catch { }
        }
    }

    public class HediffGiver_Esoteric_Chance : HediffGiver
    {
        public float mtbDays;
        public int chance = 50;
        private bool triggered = false;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            try
            {
                if ((Rand.RangeInclusive(0, 100) <= chance && !triggered) && pawn.RaceProps.intelligence == Intelligence.Humanlike)
                {
                    if (Rand.MTBEventOccurs(this.mtbDays, 60000f, 60f) && base.TryApply(pawn, null))
                    {
                        IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);
                        if (cause.def.HasComp(typeof(HediffComp_Single)))
                        {

                            pawn.health.RemoveHediff(cause);

                        }
                    }
                }
                else { triggered = true; }

            }
            catch { }
        }
    }

    public class HediffGiver_Esoteric_Diet : HediffGiver
    {
        public float mtbDays;
        public PawnKindDef pawnTFKind;
        public String newFood;
        private bool triggered = false;


        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
                if (Rand.MTBEventOccurs(this.mtbDays, 60000f, 60f) && !triggered)
                {
                IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);

                Pawn pawnTF = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnTFKind, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 1f, false, true, true, false, false, false, false, null, null, null, null, null, null, null, null));
                pawnTF.needs = pawn.needs;
                pawnTF.jobs = pawn.jobs;
                pawnTF.health = pawn.health;
                pawnTF.mindState = pawn.mindState;
                pawnTF.records = pawn.records;
                pawnTF.stances = pawn.stances;
                pawnTF.equipment = pawn.equipment;
                pawnTF.apparel = pawn.apparel;
                pawnTF.skills = pawn.skills;
                pawnTF.story = pawn.story;
                pawnTF.workSettings = pawn.workSettings;
                pawnTF.relations = pawn.relations;
                pawnTF.skills = pawn.skills;
                pawnTF.Name = pawn.Name;
                pawnTF.gender = pawn.gender;
                pawnTF.skills = pawn.skills;




                Pawn pawn3 = (Pawn)GenSpawn.Spawn(pawnTF, pawn.PositionHeld, pawn.MapHeld, 0);
                IntermittentMagicSprayer.ThrowMagicPuffDown(pawn3.Position.ToVector3(), pawn3.MapHeld);
                Find.TickManager.slower.SignalForceNormalSpeedShort();
                Find.LetterStack.ReceiveLetter("LetterHediffFromDietChangeLabel".Translate(pawn.LabelShort, newFood).CapitalizeFirst(), "LetterHediffFromDietChange".Translate(pawn.LabelShort, newFood).CapitalizeFirst(), LetterDefOf.NeutralEvent, pawn, null, null);
                pawn.DeSpawn(0);
                triggered = true;         
    
                }

        }
    }

    public class HediffGiver_PermanentFeral : HediffGiver
    {
        public float mtbDays;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (base.TryApply(pawn, null))
            {
                Find.LetterStack.ReceiveLetter("LetterHediffFromPermanentTFLabel".Translate(pawn.LabelShort).CapitalizeFirst(), "LetterHediffFromPermanentTF".Translate(pawn.LabelShort).CapitalizeFirst(), LetterDefOf.NegativeEvent, pawn, null, null);
            }
        }
    }

    public class HediffGiver_TF : HediffGiver
    {
        public string pawnkind;
        public float chance = LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().transformChance;
        public float mtbDays;
        public string forceGender = "Original";
        public float forceGenderChance = 50;
        private bool triggered = false;
        private string hediffDef = "TransformedHuman";
        public TaleDef tale;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if ((Rand.RangeInclusive(0, 100) <= chance && !triggered) && (mtbDays == 0.0f || Rand.MTBEventOccurs(mtbDays, 60000f, 60f)) && pawn.RaceProps.intelligence == Intelligence.Humanlike)
            {
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

                if (tale != null)
                {
                    TaleRecorder.RecordTale(tale, new object[]
                    {
                            pawn,
                            pawnTF
                    });
                }
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
                Find.LetterStack.ReceiveLetter("LetterHediffFromTransformationLabel".Translate(pawn.LabelShort, pawnTFKind.LabelCap).CapitalizeFirst(), "LetterHediffFromTransformation".Translate(pawn.LabelShort, pawnTFKind.LabelCap).CapitalizeFirst(), LetterDefOf.NeutralEvent, pawn, null, null); 
                pawn.apparel.DropAll(pawn.PositionHeld);
                pawn.equipment.DropAllEquipment(pawn.PositionHeld);

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
                if (pawn.ownership.OwnedBed != null) { 
                    pawn.ownership.UnclaimBed();
                }
                if (pawn.CarriedBy != null)
                {
                    Pawn carryingPawn = pawn.CarriedBy;
                    Thing outPawn;
                    carryingPawn.carryTracker.TryDropCarriedThing(carryingPawn.Position,ThingPlaceMode.Direct, out outPawn);
                    
                }
                pawn.DeSpawn();
                Find.TickManager.slower.SignalForceNormalSpeedShort();
            }
            else { triggered = true; }
        }
    }

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

    public class HediffGiver_EsotericInstant : HediffGiver
    {
        public float mtbDays;


        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            try
            {
                if (Rand.RangeInclusive(0, 5) == 1 && base.TryApply(pawn, null))
                {
                    if (cause.def.HasComp(typeof(HediffComp_Single)))
                    {

                        pawn.health.RemoveHediff(cause);

                    }
                }
            }
            catch { }
        }
    }    

}
