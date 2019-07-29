using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace EtherGun
{
    public class CompEtherExplosive : CompExplosive
    {
        public new CompProperties_EtherExplosive Props
        {
            get
            {
                return (CompProperties_EtherExplosive)props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (wickStarted && wickTicksLeft <= 1)
            {
                transformArea();
            }
        }

        public void transformArea()
        {
            List<Thing> thingList = GenRadial.RadialDistinctThingsAround(parent.PositionHeld, parent.Map, Props.explosiveRadius, true).ToList();
            List<Pawn> pawnsAffected = new List<Pawn>();
            HediffDef hediff = Props.HediffToAdd;
            float chance = Props.AddHediffChance;

            foreach (Pawn pawn in thingList.OfType<Pawn>())
            {
               
                if (!pawnsAffected.Contains(pawn) && Props.CanAddHediffToPawn(pawn))
                {
                    pawnsAffected.Add(pawn);
                }
            }

            TransformPawn.ApplyHediff(pawnsAffected, parent.Map, hediff, chance); //does the list need clearing? 
        }
    }
}
