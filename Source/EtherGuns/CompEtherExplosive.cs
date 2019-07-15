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
                return this.props as CompProperties_EtherExplosive;
            }
        }

        protected new void Detonate(Map map)
        {
            List<Thing> thingList = GenRadial.RadialDistinctThingsAround(parent.Position, map, Props.explosiveRadius, true).ToList();
            List<Pawn> pawnsAffected = new List<Pawn>();
            HediffDef hediff = Props.HediffToAdd;
            float chance = Props.AddHediffChance;

            for (int i = 0; i < thingList.Count; i++)
            {
                Pawn pawn = thingList[i] as Pawn;
                if (pawn != null && !pawnsAffected.Contains(pawn))
                {
                    pawnsAffected.Add(pawn);
                }
            }

            TransformPawn.ApplyHediff(pawnsAffected, map, hediff, chance);
            base.Detonate(map);
        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look<HediffDef>(ref HediffToAdd, "HediffToAdd");
            Scribe_Values.Look<float>(ref AddHediffChance, "AddHediffChance", 0.99f, false);
        }

        public HediffDef HediffToAdd;
        public float AddHediffChance;
    }
}
