// SheepChef.cs created by Iron Wolf for Pawnmorph on 01/05/2021 4:30 PM
// last updated 01/05/2021  4:30 PM

using RimWorld;
using Verse;

namespace Pawnmorph.IncidentWorkers
{
    /// <summary>
    /// </summary>
    /// <seealso cref="RimWorld.IncidentWorker" />
    public class SheepChef : IncidentWorker
    {
        /// <summary>
        ///     Determines whether this instance with the specified parms [can fire now sub]
        /// </summary>
        /// <param name="parms">The parms.</param>
        /// <returns>
        ///     <c>true</c> if this instance with the specified parms  [can fire now sub]  otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (parms?.forced == true) return true;

            bool fired = Find.World?.GetComponent<PawnmorphGameComp>()?.sheepChefEventFired ?? false;

            return !fired;
        }

        /// <summary>
        ///     Tries the execute worker.
        /// </summary>
        /// <param name="parms">The parms.</param>
        /// <returns></returns>
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map) parms.target;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 result, map, CellFinder.EdgeRoadChance_Animal)) return false;

            PawnKindDef kind = PMPawnKindDefOf.Sheep;


            IntVec3 loc = CellFinder.RandomClosewalkCellNear(result, map, 12);
            Pawn pawn = PawnGenerator.GeneratePawn(kind);
            GenSpawn.Spawn(pawn, loc, map, Rot4.Random);
            pawn.SetFaction(Faction.OfPlayer);
            FormerHumanUtilities.MakeAnimalSapient(pawn, backstoryOverride: PMBackstoryDefOf.PM_SheepChef);


            SendStandardLetter("PMSheepChefLetterLabel".Translate(kind.label).CapitalizeFirst(),
                               "PMSheepChefLetter".Translate(kind.label), LetterDefOf.PositiveEvent, parms,
                               new TargetInfo(result, map));
            var wComp = Find.World.GetComponent<PawnmorphGameComp>();
            wComp.sheepChefEventFired = true;

            return true;
        }
    }
}