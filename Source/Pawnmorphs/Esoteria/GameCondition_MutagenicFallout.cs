using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Multiplayer.API;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
using UnityEngine;

namespace Pawnmorph
{
    public class GameCondition_MutagenicFallout : GameCondition
    {
        private const int LerpTicks = 5000;

        private const float MaxSkyLerpFactor = 0.5f;

        private const float SkyGlow = 0.85f;

        private SkyColorSet MutagenicFalloutColors = new SkyColorSet(new ColorInt(216, 255, 0).ToColor, new ColorInt(234, 200, 255).ToColor, new Color(0.5f, 0.8f, 0.4f), 0.85f);

        private List<SkyOverlay> overlays = new List<SkyOverlay>
        {
            new WeatherOverlay_Mutagen()
        };

        private const int CheckInterval = 3451;

        private const float ToxicPerDay = 0.5f;

        public override void GameConditionTick()
        {
            List<Map> affectedMaps = base.AffectedMaps;
            if (Find.TickManager.TicksGame % 3451 == 0)
            {
                for (int i = 0; i < affectedMaps.Count; i++)
                {
                    DoPawnsMutagenicDamage(affectedMaps[i]);
                }
            }
            for (int j = 0; j < overlays.Count; j++)
            {
                for (int k = 0; k < affectedMaps.Count; k++)
                {
                    overlays[j].TickOverlay(affectedMaps[k]);
                }
            }
        }

        private void DoPawnsMutagenicDamage(Map map)
        {
            List<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
            var mutagen = MutagenDefOf.defaultMutagen;
            for (int i = 0; i < allPawnsSpawned.Count; i++)
            {
                Pawn pawn = allPawnsSpawned[i];

                if (!pawn.Position.Roofed(map) && mutagen.CanInfect(pawn))
                {
                    float num = 0.028758334f;
                    num *= pawn.GetStatValue(StatDefOf.ToxicSensitivity);
                    if (num != 0f)
                    {
                        float num2 = Mathf.Lerp(0.85f, 1.15f, Rand.ValueSeeded(pawn.thingIDNumber ^ 0x46EDC5D)); //should be ok
                        num *= num2;                                                //what's the magic number? 
                        HealthUtility.AdjustSeverity(pawn, Hediffs.MorphTransformationDefOf.MutagenicBuildup, num);
                    }
                }
            }
        }

        public override void GameConditionDraw(Map map)
        {
            for (int i = 0; i < overlays.Count; i++)
            {
                overlays[i].DrawOverlay(map);
            }
        }

        public override float SkyTargetLerpFactor(Map map)
        {
            return GameConditionUtility.LerpInOutValue(this, 5000f, 0.5f);
        }

        public override SkyTarget? SkyTarget(Map map)
        {
            return new SkyTarget(0.85f, MutagenicFalloutColors, 1f, 1f);
        }

        private const double PLANT_SUBSTITUTION_CHANCE = 0.0250000013411045;

        public bool IsMutagenicPlant(Plant plant)
        {
            return plant.def.In(PMThingDefOf.Plant_ChaoBulb, PMThingDefOf.Plant_GnarledTree);
        }

        public override void DoCellSteadyEffects(IntVec3 c, Map map)
        {
            base.DoCellSteadyEffects(c, map);

            if (c.Roofed(map)) return;

            var thingList = c.GetThingList(map);

            RandUtilities.PushState(); // MP compatibility check for rand usage.

            for (var index = thingList.Count - 1; index > 0; index--)
            {
                Thing thing = thingList[index];
                if (!(thing is Plant plant)) continue;
                if (IsMutagenicPlant(plant)) continue;

                if (Rand.Value < PLANT_SUBSTITUTION_CHANCE)
                {
                    SubstitutePlant(plant);
                }
            }

            RandUtilities.PopState();
        }

        [SyncMethod]
        private void SubstitutePlant(Plant plant)
        {
            var plantType = plant.def.plant.purpose;
            var plantProps = plant.def.plant;
            ThingDef plantDef;

            if (plantProps.IsTree)
            {
                plantDef = PMThingDefOf.Plant_GnarledTree;
            }
            else if (plantProps.harvestedThingDef != null)
            {
                plantDef = PMThingDefOf.Plant_ChaoBulb;
            }
            else
            {
                plantDef = null;
            }

            var pos = plant.Position;
            var map = plant.Map;

            if (plantDef != null) //spawn a new plant 
            {
                var newPlant = (Plant)GenSpawn.Spawn(plantDef, pos, map);
                newPlant.Growth = plant.Growth * 1.3f; // Make the new plant a little more mature then the one that was substituted.
            }

            plant.Kill();
        }

        public override float AnimalDensityFactor(Map map)
        {
            return 0.5f;
        }

        public override float PlantDensityFactor(Map map)
        {
            return 0.5f;
        }

        public override bool AllowEnjoyableOutsideNow(Map map)
        {
            return false;
        }

        public override List<SkyOverlay> SkyOverlays(Map map)
        {
            return overlays;
        }
    }
}
