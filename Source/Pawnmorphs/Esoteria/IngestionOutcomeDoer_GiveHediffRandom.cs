using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using static RimWorld.MoteMaker;
using RimWorld;

namespace Pawnmorph
{
    public class IngestionOutcomeDoer_GiveHediffRandom : IngestionOutcomeDoer
    {
        public List<HediffDef> hediffDefs;
        public List<HediffDef> hediffDefsComplete;
        public float completeChance = LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().partialChance;
        private HediffDef hediffDef;

        public float severity = -1f;

        public ChemicalDef toleranceChemical;

        public bool divideByBodySize = false;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
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

    public class IngestionOutcomeDoer_GiveHediffAll : IngestionOutcomeDoer
    {
        public List<HediffDef> hediffDefs;
        public List<HediffDef> hediffDefsComplete;
        public float completeChance;

        public float severity = -1f;

        public ChemicalDef toleranceChemical;

        public bool divideByBodySize = false;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            try
            {
                foreach (HediffDef h in hediffDefs)
                {

                    Hediff hediff = HediffMaker.MakeHediff(h, pawn, null);
                    float num;
                    if (this.severity > 0f)
                    {
                        num = this.severity;
                    }
                    else
                    {
                        num = h.initialSeverity;
                    }
                    if (this.divideByBodySize)
                    {
                        num /= pawn.BodySize;
                    }
                    AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, this.toleranceChemical, ref num);
                    hediff.Severity = num;
                    pawn.health.AddHediff(hediff, null, null, null);
                }
            }
            catch { }
        }
    }

    public class IngestionOutcomeDoer_EsotericRevert : IngestionOutcomeDoer
    {
        public float mtbDays;
        public List<HediffDef> defsToRevert;
        public List<HediffDef> revertThoughts;
        public string transformedHuman = "TransformedHuman";

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {

            List<Hediff> hS = new List<Hediff>(pawn.health.hediffSet.hediffs);

            foreach (Hediff hediffx in hS)
            {
                if (hediffx.def.hediffClass == typeof(Hediff_AddedMutation))
                {
                    pawn.health.RemoveHediff(hediffx);
                }
                if (hediffx.def.hediffClass == typeof(HediffGiver_TFRandom))
                {
                    pawn.health.RemoveHediff(hediffx);
                }
                if (hediffx.def.hediffClass == typeof(HediffGiver_TF))
                {
                    pawn.health.RemoveHediff(hediffx);
                }
            }

            if (pawn.health.hediffSet.hediffs.Any(x => defsToRevert.Contains(x.def)))
            {
                PawnmorphGameComp loader = Find.World.GetComponent<PawnmorphGameComp>();

                PawnMorphInstance pm = loader.retrieve(pawn);
                Pawn pawn3 = null;
                HediffDef rThought = revertThoughts.RandomElement();
                if (pm != null)
                {
                    pawn3 = (Pawn)GenSpawn.Spawn(pm.origin, pawn.PositionHeld, pawn.MapHeld, 0);
                    pawn3.apparel.DestroyAll();
                    pawn3.equipment.DestroyAllEquipment();
                    for (int i = 0; i < 10; i++)
                    {
                        IntermittentMagicSprayer.ThrowMagicPuffDown(pawn3.Position.ToVector3(), pawn3.MapHeld);
                        IntermittentMagicSprayer.ThrowMagicPuffUp(pawn3.Position.ToVector3(), pawn3.MapHeld);
                    }

                    Hediff h = HediffMaker.MakeHediff(rThought, pawn, null);

                    Hediff hf = pm.replacement.health.hediffSet.hediffs.Find(x => x.def == HediffDef.Named(transformedHuman));

                    if (hf != null)
                    {
                        h.Severity = hf.Severity;
                    }

                    pawn3.health.AddHediff(h);
                    pm.replacement.DeSpawn(0);
                }
                else
                {

                    Gender newGender = pawn.gender;

                    if (Rand.RangeInclusive(0, 100) <= 50)
                    {
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
                        
                    }

                    float animalAge = pawn.ageTracker.AgeBiologicalYearsFloat;
                    float animalLifeExpectancy = pawn.def.race.lifeExpectancy;
                    float humanLifeExpectancy = 80f;

                    float converted = animalLifeExpectancy / animalAge;

                    float lifeExpectancy = humanLifeExpectancy / converted;

                    List<PawnKindDef> pkds = new List<PawnKindDef>();
                    pkds.Add(PawnKindDefOf.Slave);
                    pkds.Add(PawnKindDefOf.WildMan);
                    pkds.Add(PawnKindDefOf.Colonist);
                    pkds.Add(PawnKindDefOf.SpaceRefugee);
                    pkds.Add(PawnKindDefOf.Villager);
                    pkds.Add(PawnKindDefOf.Drifter);
                    pkds.Add(PawnKindDefOf.AncientSoldier);

                    Pawn pawnTF = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pkds.RandomElement(), Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 1f, false, true, true, false, false, false, false, null, null, null, new float?(lifeExpectancy), new float?(Rand.Range(lifeExpectancy, lifeExpectancy + 200)), new Gender?(newGender), null, null));

                    pawnTF.needs.food.CurLevel = pawn.needs.food.CurLevel;
                    pawnTF.needs.rest.CurLevel = pawn.needs.rest.CurLevel;

                    pawn3 = (Pawn)GenSpawn.Spawn(pawnTF, pawn.PositionHeld, pawn.MapHeld, 0);
                    pawn3.apparel.DestroyAll();
                    pawn3.equipment.DestroyAllEquipment();
                    for (int i = 0; i < 10; i++)
                    {
                        IntermittentMagicSprayer.ThrowMagicPuffDown(pawn3.Position.ToVector3(), pawn3.MapHeld);
                        IntermittentMagicSprayer.ThrowMagicPuffUp(pawn3.Position.ToVector3(), pawn3.MapHeld);
                    }

                    Hediff h = HediffMaker.MakeHediff(rThought, pawn, null);

                    Hediff hf = pawn.health.hediffSet.hediffs.Find(x => x.def == HediffDef.Named(transformedHuman));

                    if (hf != null)
                    {
                        h.Severity = hf.Severity;
                    }

                    pawn3.health.AddHediff(h);
                    pawn.DeSpawn(0);

                }

                List<Hediff> hS2 = new List<Hediff>(pawn.health.hediffSet.hediffs);

                foreach (Hediff hediffx in hS2)
                {
                    if (hediffx.def.hediffClass == typeof(Hediff_AddedMutation))
                    {
                        pawn.health.RemoveHediff(hediffx);
                    }
                    if (hediffx.def.hediffClass == typeof(HediffGiver_TFRandom))
                    {
                        pawn.health.RemoveHediff(hediffx);
                    }
                    if (hediffx.def.hediffClass == typeof(HediffGiver_TF))
                    {
                        pawn.health.RemoveHediff(hediffx);
                    }
                }

            }
        }
    }

    public class IngestionOutcomeDoer_GiveHediffIfNonePresent : IngestionOutcomeDoer
    {
        public List<HediffDef> hediffDefs;
        public List<HediffDef> hediffDefsComplete;
        public float completeChance;
        private HediffDef hediffDef;

        public float severity = -1f;

        public ChemicalDef toleranceChemical;

        public bool divideByBodySize = false;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            try
            {

                if (!pawn.health.hediffSet.hediffs.Any(x => hediffDefs.Contains(x.def)))
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


            }
            catch { }
        }
    }


    public class IngestionOutcomeDoer_CompleteTF : IngestionOutcomeDoer
    {

        public List<HediffDef> hediffDefs;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            try
            {

                foreach (Hediff hD in pawn.health.hediffSet.hediffs)
                {

                    HediffGiver checkHed = hD.def.stages.First().hediffGivers.Find(x => x.GetType() == typeof(HediffGiver_TF));

                    if (checkHed != null && hD.Severity <= hD.def.stages[1].minSeverity)
                    {
                        HediffGiver_TF hed = (HediffGiver_TF)checkHed;

                        hed.TryApply(pawn);

                    }


                }

                        

            }
            catch { }
        }
    }

    
}
