using Verse;
using RimWorld;

namespace Pawnmorph
{
    public class HediffGiver_Esoteric_Diet : HediffGiver
    {
        public float mtbDays;
        public PawnKindDef pawnTFKind;
        public string newFood;
        private bool triggered = false;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (Rand.MTBEventOccurs(mtbDays, 60000f, 60f) && !triggered)
            {
                IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);

                Pawn pawnTF = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnTFKind, Faction.OfPlayer));
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
}
