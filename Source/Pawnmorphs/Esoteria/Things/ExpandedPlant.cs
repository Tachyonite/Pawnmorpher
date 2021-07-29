// ExpandedPlant.cs created by Iron Wolf for Pawnmorph on 07/25/2021 6:39 PM
// last updated 07/25/2021  6:39 PM

using System.Collections.Generic;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Things
{
    /// <summary>
    ///     class for more flexible plants
    /// </summary>
    /// <seealso cref="RimWorld.Plant" />
    public class ExpandedPlant : Plant
    {
        private AdditionalPlantInfo _info;

        /// <summary>
        ///     Gets the growth rate.
        /// </summary>
        /// <value>
        ///     The growth rate.
        /// </value>
        public override float GrowthRate
        {
            get
            {
                if (Blighted) return 0f;
                if (Spawned && !PlantUtility.GrowthSeasonNow(Position, Map)) return 0f;
                return GrowthRateFactor_Fertility * DynGrowthRateFactor_Temperature;
            }
        }

        /// <summary>
        ///     Gets the leafless temperature thresh.
        /// </summary>
        /// <value>
        ///     The leafless temperature thresh.
        /// </value>
        protected override float LeaflessTemperatureThresh
        {
            get
            {
                var num = 8f;
                return this.HashOffset() * 0.01f % num - num + DynMaxLeaflessTemperature;
            }
        }


        [CanBeNull]
        private AdditionalPlantInfo Info
        {
            get
            {
                if (_info == null) _info = def.GetModExtension<AdditionalPlantInfo>();

                return _info;
            }
        }

        private float DynMaxOptimalGrowthTemperature => Info?.maxOptimalGrowthTemperature ?? MaxOptimalGrowthTemperature;

        private float DynMinOptimalGrowthTemperature => Info?.minOptimalGrowthTemperature ?? MinOptimalGrowthTemperature;

        private float DynMaxGrowthTemperature => Info?.maxGrowthTemperature ?? MaxGrowthTemperature;


        private float DynMinGrowthTemperature => Info?.minGrowthTemperature ?? MinGrowthForAnimalIngestion;

        private float DynMaxLeaflessTemperature => Info?.maxLeaflessTemperature ?? MaxLeaflessTemperature;

        private float DynGrowthRateFactor_Temperature
        {
            get
            {
                if (!GenTemperature.TryGetTemperatureForCell(Position, Map, out float tempResult)) return 1f;
                return GrowthRateFactorFor_Temperature(tempResult);
            }
        }

        private List<ThingComp> _newComps;

        List<ThingComp> NewComps
        {
            get
            {
                if (_newComps == null)
                {
                    //need to do this reflection horribleness because of hard coded growth suppression in winter 
                    var fld = typeof(ThingWithComps).GetField("comps", BindingFlags.Instance | BindingFlags.NonPublic);
                    _newComps = (List<ThingComp>) fld.GetValue(this); 
                }

                return _newComps;
            }
        }

        /// <summary>
        /// performs a long tick.
        /// </summary>
        public override void TickLong() //need this copy-paste nonsense because of hardcoded growth suppression in winter 
        {
            CheckTemperatureMakeLeafless();
            if (Destroyed) return;

            if (NewComps != null)
            {
                int i = 0;
                for (int count = NewComps.Count; i < count; i++)
                {
                    NewComps[i].CompTickLong();
                }
            }

            float num = growthInt;
            bool num2 = LifeStage == PlantLifeStage.Mature;
            growthInt += GrowthPerTick * 2000f;
            if (growthInt > 1f) growthInt = 1f;
            if ((!num2 && LifeStage == PlantLifeStage.Mature || (int) (num * 10f) != (int) (growthInt * 10f))
             && CurrentlyCultivated()) Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things);


            unlitTicks = 0;
            ageInt += 2000;
            if (Dying)
            {
                Map map = Map;
                bool isCrop = IsCrop;
                bool harvestableNow = HarvestableNow;
                bool dyingBecauseExposedToLight = DyingBecauseExposedToLight;
                int num3 = Mathf.CeilToInt(CurrentDyingDamagePerTick * 2000f);
                TakeDamage(new DamageInfo(DamageDefOf.Rotting, num3));
                if (Destroyed)
                {
                    if (isCrop
                     && def.plant.Harvestable
                     && MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfRot-" + def.defName, 240f))
                    {
                        string key = harvestableNow ? "MessagePlantDiedOfRot_LeftUnharvested" :
                                     !dyingBecauseExposedToLight ? "MessagePlantDiedOfRot" :
                                     "MessagePlantDiedOfRot_ExposedToLight";
                        Messages.Message(key.Translate(GetCustomLabelNoCount(false)), new TargetInfo(Position, map),
                                         MessageTypeDefOf.NegativeEvent);
                    }

                    return;
                }
            }

            cachedLabelMouseover = null;
            if (def.plant.dropLeaves)
            {
                var moteLeaf = MoteMaker.MakeStaticMote(Vector3.zero, Map, ThingDefOf.Mote_Leaf) as MoteLeaf;
                if (moteLeaf != null)
                {
                    float num4 = def.plant.visualSizeRange.LerpThroughRange(growthInt);
                    float treeHeight = def.graphicData.drawSize.x * num4;
                    Vector3 vector = Rand.InsideUnitCircleVec3 * LeafSpawnRadius;
                    moteLeaf.Initialize(Position.ToVector3Shifted() + Vector3.up * Rand.Range(LeafSpawnYMin, LeafSpawnYMax) + vector + Vector3.forward * def.graphicData.shadowData.offset.z,
                                        Rand.Value * 2000.TicksToSeconds(), vector.z > 0f, treeHeight);
                }
            }
        }

        private string cachedLabelMouseover;
        private static float LeafSpawnYMin = 0.3f;
        private static float LeafSpawnYMax = 1f;
        private static float LeafSpawnRadius = 0.4f;
        /// <summary>
        /// Gets the  mouseover label.
        /// </summary>
        /// <value>
        /// The label mouseover.
        /// </value>
        public override string LabelMouseover  //need this copy pate nonsense because of the nonsense in LongTick 
        {
            get
            {
                if (cachedLabelMouseover == null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(def.LabelCap);
                    stringBuilder.Append(" (" + "PercentGrowth".Translate(GrowthPercentString));
                    if (Dying)
                    {
                        stringBuilder.Append(", " + "DyingLower".Translate());
                    }
                    stringBuilder.Append(")");
                    cachedLabelMouseover = stringBuilder.ToString();
                }
                return cachedLabelMouseover;
            }
        }

        /// <summary>
        ///     Gets the inspect string.
        /// </summary>
        /// <returns></returns>
        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            if (def.plant.showGrowthInInspectPane)
            {
                if (LifeStage == PlantLifeStage.Growing)
                {
                    stringBuilder.AppendLine("PercentGrowth".Translate(GrowthPercentString));
                    stringBuilder.AppendLine("GrowthRate".Translate() + ": " + GrowthRate.ToStringPercent());
                    if (!Blighted)
                    {
                        if (Resting) stringBuilder.AppendLine("PlantResting".Translate());
                        float growthRateFactor_Temperature = DynGrowthRateFactor_Temperature;
                        if (growthRateFactor_Temperature < 0.99f)
                        {
                            if (Mathf.Approximately(growthRateFactor_Temperature, 0f)
                             || !PlantUtility.GrowthSeasonNow(Position, Map))
                                stringBuilder.AppendLine("OutOfIdealTemperatureRangeNotGrowing".Translate());
                            else
                                stringBuilder.AppendLine("OutOfIdealTemperatureRange".Translate(Mathf
                                                                                               .Max(1,
                                                                                                    Mathf
                                                                                                       .RoundToInt(growthRateFactor_Temperature
                                                                                                                 * 100f))
                                                                                               .ToString()));
                        }
                    }
                }
                else if (LifeStage == PlantLifeStage.Mature)
                {
                    if (HarvestableNow)
                        stringBuilder.AppendLine("ReadyToHarvest".Translate());
                    else
                        stringBuilder.AppendLine("Mature".Translate());
                }

                if (DyingBecauseExposedToLight) stringBuilder.AppendLine("DyingBecauseExposedToLight".Translate());
                if (Blighted) stringBuilder.AppendLine("Blighted".Translate() + " (" + Blight.Severity.ToStringPercent() + ")");
            }

            string text = InspectStringPartsFromComps();
            if (!text.NullOrEmpty()) stringBuilder.Append(text);
            return stringBuilder.ToString().TrimEndNewlines();
        }

        /// <summary>
        ///     gets the growth rate factor for this plant .
        /// </summary>
        /// <param name="cellTemp">The cell temporary.</param>
        /// <returns></returns>
        public float GrowthRateFactorFor_Temperature(float cellTemp)
        {
            if (cellTemp < DynMinOptimalGrowthTemperature)
                return Mathf.InverseLerp(DynMinGrowthTemperature, DynMinOptimalGrowthTemperature, cellTemp);
            if (cellTemp > DynMaxOptimalGrowthTemperature)
                return Mathf.InverseLerp(DynMaxGrowthTemperature, DynMaxOptimalGrowthTemperature, cellTemp);
            return 1f;
        }
    }
}