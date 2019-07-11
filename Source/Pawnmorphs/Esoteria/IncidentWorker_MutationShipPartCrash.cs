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

    public class CompProperties_MutagenicRadius : CompProperties
    {
        public SimpleCurve radiusPerDayCurve;
        public HediffDef hediff;

        public float harmFrequencyPerArea = 1f;

        public CompProperties_MutagenicRadius()
        {
            this.compClass = typeof(CompMutagenicRadius);
        }
    }

    public class CompMutagenicRadius : ThingComp
    {
        private int plantHarmAge;

        private int ticksToPlantHarm;

        private float LeaflessPlantKillChance = 0.09f;

        protected CompProperties_MutagenicRadius PropsPlantHarmRadius
        {
            get
            {
                return (CompProperties_MutagenicRadius)this.props;
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look<int>(ref this.plantHarmAge, "plantHarmAge", 0, false);
            Scribe_Values.Look<int>(ref this.ticksToPlantHarm, "ticksToPlantHarm", 0, false);
        }

        public override void CompTick()
        {
            if (this.parent.IsHashIntervalTick(60))
            {
                if (!this.parent.Spawned)
                {
                    return;
                }
                this.plantHarmAge++;
                this.ticksToPlantHarm--;
                if (this.ticksToPlantHarm <= 0)
                {
                    float x = (float)this.plantHarmAge / 60000f;
                    float num = this.PropsPlantHarmRadius.radiusPerDayCurve.Evaluate(x);
                    float num2 = 3.14159274f * num * num;
                    float num3 = num2 * this.PropsPlantHarmRadius.harmFrequencyPerArea;
                    float num4 = 60f / num3;
                    int num5;
                    if (num4 >= 1f)
                    {
                        this.ticksToPlantHarm = GenMath.RoundRandom(num4);
                        num5 = 1;
                    }
                    else
                    {
                        this.ticksToPlantHarm = 1;
                        num5 = GenMath.RoundRandom(1f / num4);
                    }
                    for (int i = 0; i < num5; i++)
                    {
                        this.MutateInRadius(num, this.PropsPlantHarmRadius.hediff);
                    }
                }
            }
        }

        private void MutateInRadius(float radius, HediffDef hediff)
        {
            IntVec3 c = this.parent.Position + (Rand.InsideUnitCircleVec3 * radius).ToIntVec3();
            if (!c.InBounds(this.parent.Map))
            {
                return;
            }
            Pawn pawn = c.GetFirstPawn(this.parent.Map);
            if (pawn != null)
            {
                if (!pawn.health.hediffSet.HasHediff(hediff))
                {
                    if (Rand.Value < this.LeaflessPlantKillChance)
                    {
                        Hediff applyHediff = HediffMaker.MakeHediff(hediff, pawn, null);
                        applyHediff.Severity = 1f;
                        pawn.health.AddHediff(applyHediff, null, null, null);
                    }
                }
            }
            Plant plant = c.GetPlant(this.parent.Map);
            if (plant != null)
            {
                if (!plant.LeaflessNow)
                {
                    plant.MakeLeafless(Plant.LeaflessCause.Poison);
                }
            }
            IntVec3 cc = this.parent.Position + (Rand.InsideUnitCircleVec3 * radius).ToIntVec3();
            SnowUtility.AddSnowRadial(this.parent.Position, this.parent.Map, radius, 0.01f);
        }
    }
}
