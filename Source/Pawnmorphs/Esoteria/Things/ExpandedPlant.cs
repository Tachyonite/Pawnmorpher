// ExpandedPlant.cs created by Iron Wolf for Pawnmorph on 07/25/2021 6:39 PM
// last updated 07/25/2021  6:39 PM

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
                return GrowthRateFactor_Fertility * DynGrowthRateFactor_Temperature * GrowthRateFactor_Light;
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
                        if (!HasEnoughLightToGrow)
                            stringBuilder.AppendLine("PlantNeedsLightLevel".Translate()
                                                   + ": "
                                                   + def.plant.growMinGlow.ToStringPercent());
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