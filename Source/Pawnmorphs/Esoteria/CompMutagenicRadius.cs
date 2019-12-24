using Verse;
using RimWorld;

namespace Pawnmorph
{
    /// <summary>
    /// comp for mutating things within a radius 
    /// </summary>
    public class CompMutagenicRadius : ThingComp
    {
        private const float LEAFLESS_PLANT_KILL_CHANCE = 0.09f;
        private const float MUTATE_IN_RADIUS_CHANCE = 0.50f;

        private int plantHarmAge;
        private int ticksToPlantHarm;
        
        CompProperties_MutagenicRadius PropsPlantHarmRadius
        {
            get
            {
                return (CompProperties_MutagenicRadius)props;
            }
        }
        /// <summary>
        /// call to save/load data 
        /// </summary>
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref plantHarmAge, "plantHarmAge", 0, false);
            Scribe_Values.Look(ref ticksToPlantHarm, "ticksToPlantHarm", 0, false);
        }


        /// <summary>
        /// called every tick after it's parent updates 
        /// </summary>
        public override void CompTick()
        {
            if (parent.IsHashIntervalTick(60))
            {
                if (!parent.Spawned)
                {
                    return;
                }
                plantHarmAge++;
                ticksToPlantHarm--;
                if (ticksToPlantHarm <= 0)
                {
                    float x = plantHarmAge / 60000f;
                    float num = PropsPlantHarmRadius.radiusPerDayCurve.Evaluate(x);
                    float num2 = 3.14159274f * num * num;
                    float num3 = num2 * PropsPlantHarmRadius.harmFrequencyPerArea;
                    float num4 = 60f / num3;
                    int num5;

                    if (num4 >= 1f)
                    {
                        ticksToPlantHarm = GenMath.RoundRandom(num4);
                        num5 = 1;
                    }
                    else
                    {
                        ticksToPlantHarm = 1;
                        num5 = GenMath.RoundRandom(1f / num4);
                    }

                    for (int i = 0; i < num5; i++)
                    {
                        MutateInRadius(num, PropsPlantHarmRadius.hediff);
                    }
                }
            }
        }

        private void MutateInRadius(float radius, HediffDef hediff)
        {
            IntVec3 c = parent.Position + (Rand.InsideUnitCircleVec3 * radius).ToIntVec3();
            if (!c.InBounds(parent.Map))
            {
                return;
            }

            Pawn pawn = c.GetFirstPawn(parent.Map);
            if (pawn != null)
            {
                if (!pawn.health.hediffSet.HasHediff(hediff))
                {
                    if (Rand.Value < MUTATE_IN_RADIUS_CHANCE)
                    {
                        Hediff applyHediff = HediffMaker.MakeHediff(hediff, pawn, null);
                        applyHediff.Severity = 1f;
                        pawn.health.AddHediff(applyHediff, null, null, null);
                    }
                }
            }

            Plant plant = c.GetPlant(parent.Map);

            if (plant != null && !plant.def.IsMutantPlant()) //don't harm mutant plants 
            {
                if (!plant.LeaflessNow)
                {
                    plant.MakeLeafless(Plant.LeaflessCause.Poison);
                }
                else if(Rand.Value < 0.3f) //30% chance
                {
                    PMPlantUtilities.TryMutatePlant(plant); 
                }
            }

            
        }
    }
}
