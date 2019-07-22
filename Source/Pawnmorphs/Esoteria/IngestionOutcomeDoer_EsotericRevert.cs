using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class IngestionOutcomeDoer_EsotericRevert : IngestionOutcomeDoer
    {
        public float mtbDays;
        public List<HediffDef> defsToRevert;
        public List<HediffDef> revertThoughts;
        public List<HediffDef> mergeRevertThoughts;
        public string transformedHuman = "TransformedHuman";

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            List<Hediff> hS = new List<Hediff>(pawn.health.hediffSet.hediffs);

            if (pawn.health.hediffSet.HasHediff(HediffDef.Named("2xMergedHuman")))
            {
                if (pawn.health.hediffSet.hediffs.Any(x => defsToRevert.Contains(x.def)))
                {
                    PawnmorphGameComp loader = Find.World.GetComponent<PawnmorphGameComp>();
                    PawnRelationDef MergeMate = DefDatabase<PawnRelationDef>.GetNamed("MergeMate");
                    PawnRelationDef ExMerge = DefDatabase<PawnRelationDef>.GetNamed("ExMerged");

                    PawnMorphInstanceMerged pm = loader.retrieveMerged(pawn);
                    Pawn pawn3 = null;
                    Pawn pawn4 = null;
                    HediffDef rThought = mergeRevertThoughts.RandomElement();
                    if (pm != null)
                    {
                        pawn3 = (Pawn)GenSpawn.Spawn(pm.origin, pm.replacement.PositionHeld, pm.replacement.MapHeld, 0);
                        pawn3.apparel.DestroyAll();
                        pawn3.equipment.DestroyAllEquipment();
                        for (int i = 0; i < 10; i++)
                        {
                            IntermittentMagicSprayer.ThrowMagicPuffDown(pawn3.Position.ToVector3(), pawn3.MapHeld);
                            IntermittentMagicSprayer.ThrowMagicPuffUp(pawn3.Position.ToVector3(), pawn3.MapHeld);
                        }

                        Hediff h = HediffMaker.MakeHediff(rThought, pawn3, null);

                        Hediff hf = pm.replacement.health.hediffSet.hediffs.Find(x => x.def == HediffDef.Named("2xMergedHuman"));

                        if (hf != null)
                        {
                            h.Severity = hf.Severity;
                        }

                        pawn3.health.AddHediff(h);

                        pawn4 = (Pawn)GenSpawn.Spawn(pm.origin2, pm.replacement.PositionHeld, pm.replacement.MapHeld, 0);
                        pawn4.apparel.DestroyAll();
                        pawn4.equipment.DestroyAllEquipment();
                        for (int i = 0; i < 10; i++)
                        {
                            IntermittentMagicSprayer.ThrowMagicPuffDown(pawn4.Position.ToVector3(), pawn4.MapHeld);
                            IntermittentMagicSprayer.ThrowMagicPuffUp(pawn4.Position.ToVector3(), pawn4.MapHeld);
                        }

                        h = HediffMaker.MakeHediff(rThought, pawn4, null);

                        if (hf != null)
                        {
                            h.Severity = hf.Severity;
                        }

                        pawn4.health.AddHediff(h);

                        if (rThought == HediffDef.Named("WasMerged"))
                        {
                            pawn3.relations.AddDirectRelation(MergeMate, pawn4);
                        }
                        else if (rThought == HediffDef.Named("WasMergedRelieved"))
                        {
                            pawn3.relations.AddDirectRelation(ExMerge, pawn4);
                        }
                        pawn3.SetFaction(Faction.OfPlayer);
                        pawn4.SetFaction(Faction.OfPlayer);
                        pm.replacement.DeSpawn(0);
                        loader.removePawnMerged(pm);
                    }
                    else
                    {
                        List<Pawn> spawned = new List<Pawn>();

                        for (int i = 0; i < 2; i++)
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
                            for (int ii = 0; ii < 10; ii++)
                            {
                                IntermittentMagicSprayer.ThrowMagicPuffDown(pawn3.Position.ToVector3(), pawn3.MapHeld);
                                IntermittentMagicSprayer.ThrowMagicPuffUp(pawn3.Position.ToVector3(), pawn3.MapHeld);
                            }

                            Hediff h = HediffMaker.MakeHediff(rThought, pawn, null);

                            Hediff hf = pawn.health.hediffSet.hediffs.Find(x => x.def == HediffDef.Named("2xMergedHuman"));

                            if (hf != null)
                            {
                                h.Severity = hf.Severity;
                            }
                            spawned.Add(pawn3);
                            pawn3.health.AddHediff(h);
                            List<Hediff> hS2 = new List<Hediff>(pawn3.health.hediffSet.hediffs);

                            foreach (Hediff hediffx in hS2)
                            {
                                if (hediffx.def.hediffClass == typeof(Hediff_AddedMutation))
                                {
                                    pawn3.health.RemoveHediff(hediffx);
                                }
                                if (hediffx.def.hediffClass == typeof(HediffGiver_TFRandom))
                                {
                                    pawn3.health.RemoveHediff(hediffx);
                                }
                                if (hediffx.def.hediffClass == typeof(HediffGiver_TF))
                                {
                                    pawn3.health.RemoveHediff(hediffx);
                                }
                            }

                        }
                        pawn.DeSpawn(0);
                        if (rThought == HediffDef.Named("WasMerged"))
                        {
                            spawned[0].relations.AddDirectRelation(MergeMate, spawned[1]);
                        }
                        else if (rThought == HediffDef.Named("WasMergedRelieved"))
                        {
                            spawned[0].relations.AddDirectRelation(ExMerge, spawned[1]);
                        }
                    }
                }
            }
            else if (pawn.health.hediffSet.HasHediff(HediffDef.Named("TransformedHuman")))
            {
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
                        loader.removePawn(pm);
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

                    List<Hediff> hS2 = new List<Hediff>(pawn3.health.hediffSet.hediffs);

                    foreach (Hediff hediffx in hS2)
                    {
                        if (hediffx.def.hediffClass == typeof(Hediff_AddedMutation))
                        {
                            pawn3.health.RemoveHediff(hediffx);
                        }
                        if (hediffx.def.hediffClass == typeof(HediffGiver_TFRandom))
                        {
                            pawn3.health.RemoveHediff(hediffx);
                        }
                        if (hediffx.def.hediffClass == typeof(HediffGiver_TF))
                        {
                            pawn3.health.RemoveHediff(hediffx);
                        }
                    }

                }
            }
            else
            {
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
                    if (hediffx.def.hediffClass == typeof(Hediff_ProductionThought))
                    {
                        pawn.health.RemoveHediff(hediffx);
                    }
                }
            }
        }
    }
}
