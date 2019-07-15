using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace EtherGun
{
    //What this is SUPPOSED to do is overide the CompExplosive's Detonate() method with our own custom method and call the base method at the end.
    public class CompEtherExplosive : CompExplosive
    {
        public new CompProperties_EtherExplosive Props
        {
            get
            {
                return (CompProperties_EtherExplosive)this.props;
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
    }
}
