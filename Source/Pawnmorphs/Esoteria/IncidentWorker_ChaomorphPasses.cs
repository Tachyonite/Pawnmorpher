using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace Pawnmorph
{
    public class IncidentWorker_ChaomorphPasses : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            return !map.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout) && map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(ThingDefOf.Thrumbo) && TryFindEntryCell(map, out IntVec3 intVec);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!TryFindEntryCell(map, out IntVec3 intVec))
            {
                return false;
            }

            List<string> pawnkinds = new List<string>
            {
                "Chaofox",
                "Chaodino",
                "Chaocow"
            };

            PawnKindDef animal = PawnKindDef.Named(pawnkinds.RandomElement());
            float num = StorytellerUtility.DefaultThreatPointsNow(map);
            int num2 = GenMath.RoundRandom(num / animal.combatPower);
            int max = Rand.RangeInclusive(2, 4);
            num2 = Mathf.Clamp(num2, 1, max);
            int num3 = Rand.RangeInclusive(90000, 150000);
            IntVec3 invalid = IntVec3.Invalid;

            if (!RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(intVec, map, 10f, out invalid))
            {
                invalid = IntVec3.Invalid;
            }

            Pawn pawn = null;
            for (int i = 0; i < num2; i++)
            {
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(intVec, map, 10, null);
                pawn = PawnGenerator.GeneratePawn(animal, null);
                GenSpawn.Spawn(pawn, loc, map, Rot4.Random, WipeMode.Vanish, false);
                pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + num3;
                if (invalid.IsValid)
                {
                    pawn.mindState.forcedGotoPosition = CellFinder.RandomClosewalkCellNear(invalid, map, 10, null);
                }
            }

            Find.LetterStack.ReceiveLetter("LetterLabelChaomorphPasses".Translate(animal.label).CapitalizeFirst(), "LetterChaomorphPasses".Translate(animal.label), LetterDefOf.PositiveEvent, pawn, null, null);
            return true;
        }

        private bool TryFindEntryCell(Map map, out IntVec3 cell)
        {
            return RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal + 0.2f, false, null);
        }
    }
}
