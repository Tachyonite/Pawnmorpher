using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;


namespace EtherGun
{
    public class DeathActionWorker_MutagenicExplosion : DeathActionWorker
    {
        public override RulePackDef DeathRules => RulePackDefOf.Transition_DiedExplosive;

        public override bool DangerousInMelee => true;

        public override void PawnDied(Corpse corpse)
        {
            GenExplosion.DoExplosion(radius: (corpse.InnerPawn.ageTracker.CurLifeStageIndex == 0) ? 2.9f : ((corpse.InnerPawn.ageTracker.CurLifeStageIndex != 1) ? 5.9f : 3.9f), center: corpse.Position, map: corpse.Map, damType: DamageDefOf.Flame, instigator: corpse.InnerPawn);
            List<Thing> thingList = GenRadial.RadialDistinctThingsAround(corpse.PositionHeld, corpse.Map, (corpse.InnerPawn.ageTracker.CurLifeStageIndex == 0) ? 2.9f : ((corpse.InnerPawn.ageTracker.CurLifeStageIndex != 1) ? 5.9f : 3.9f), true).ToList();
            List<Pawn> pawnsAffected = new List<Pawn>();
            HediffDef hediff = HediffDef.Named("FullRandomTF");
            float chance = 0.7f;

            foreach (Pawn pawn in thingList.OfType<Pawn>())
            {

                if (!pawnsAffected.Contains(pawn) && pawn.RaceProps.intelligence == Intelligence.Humanlike)
                {
                    pawnsAffected.Add(pawn);
                }
            }

            TransformPawn.ApplyHediff(pawnsAffected, corpse.InnerPawn.Map, hediff, chance); //does the list need clearing? 
        }
    }
}
