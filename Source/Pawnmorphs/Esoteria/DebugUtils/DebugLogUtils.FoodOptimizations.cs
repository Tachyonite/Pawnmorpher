// DebugLogUtils.FoodOptimizations.cs created by Iron Wolf for Pawnmorph on //2020 
// last updated 03/06/2020  4:48 PM

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Pawnmorph.DebugUtils
{
    public static partial class DebugLogUtils
    {
        private const string FOOD_OP_CATEGORY = MAIN_CATEGORY_NAME + " - " + "Food Optimizations";

        //signature of the various food finder functions 
        delegate bool FoodFinderFunc(Pawn getter, 
                                      Pawn eater,
                                      bool desperate,
                                      out Thing foodSource,
                                      out ThingDef foodDef,
                                      bool canRefillDispenser = true,
                                      bool canUseInventory = true,
                                      bool allowForbidden = false,
                                      bool allowCorpse = true,
                                      bool allowSociallyImproper = false,
                                      bool allowHarvest = false,
                                      bool forceScanWholeMap = false,
                                      bool ignoreReservations = false,
                                      FoodPreferability minPrefOverride = FoodPreferability.Undefined); 

        [DebugOutput(category = FOOD_OP_CATEGORY, onlyWhenPlaying = true)]
        static void TestFoodOptimizations()
        {
            var mapPawns = Find.CurrentMap?.mapPawns?.AllPawns;
            if (mapPawns == null) return;
            //var validGetters = mapPawns.Where(p => p.IsToolUser()).ToList();  
            StringBuilder builder = new StringBuilder();
            List<float> resultList = new List<float>(); 
            builder.AppendLine("--------------Rimworld--------------");
            builder.AppendLine(Results.HEADER); 

            TestFoodFunction(FoodUtility.TryFindBestFoodSourceFor, mapPawns, resultList);

            var rmResults = new Results(resultList); 
            resultList.Clear();
            builder.AppendLine(rmResults.ToString());


            builder.AppendLine("--------------Pawnmorpher--------------");

            TestFoodFunction(PMFoodUtilities.TryFindBestFoodSourceForOptimized, mapPawns, resultList);
            builder.AppendLine(Results.HEADER);

            var pmResults = new Results(resultList); 
            resultList.Clear();
            builder.AppendLine(pmResults.ToString());
            Log.Message(builder.ToString()); 
        }

        class Results
        {
            public float average;
            public float min;
            public float max;
            public float stdDev;
            public bool isValid;

            /// <summary>Returns a string that represents the current object.</summary>
            /// <returns>A string that represents the current object.</returns>
            public override string ToString()
            {
                if (!isValid) return "INVALID";
                return $"{average}, {min}, {max}, {stdDev}";
            }

            public const string HEADER = nameof(average) + "," + nameof(min) + "," + nameof(max) + "," + nameof(stdDev); 

            public Results([NotNull] IReadOnlyList<float> entries)
            {
                if (entries == null) throw new ArgumentNullException(nameof(entries));
                isValid = entries.Count > 1;
                if (!isValid) return; 
                average = entries.Average();
                min = entries.Min();
                max = entries.Max();
                var n = entries.Count - 1;
                stdDev = 0;//calculate standard deviation 
                foreach (float entry in entries)
                {
                    stdDev += (entry - average) * (entry - average); 
                }

                stdDev /= n;
                stdDev = Mathf.Sqrt(stdDev); 
            }
        }
        

        static void TestFoodFunction([NotNull] FoodFinderFunc func, [NotNull] List<Pawn> testers, [NotNull] List<float> results)
        {
            foreach (Pawn p in testers)
            {
                if (p == null || p.Dead || p.Destroyed || p.Map == null) continue;

                try
                {
                    var dur = TestFoodFunction(FoodUtility.TryFindBestFoodSourceFor, p);
                    results.Add(dur);
                }
                catch (Exception e)
                {
                    Log.Message($"caught {e.GetType().Name} while testing {p.Name}\n{e}");
                }
            }
        }

        static float TestFoodFunction([NotNull] FoodFinderFunc func, [NotNull] Pawn tstPawn)
        {
            var sw = Stopwatch.StartNew();
            bool desperate = tstPawn.needs.food.CurCategory == HungerCategory.Starving;
            func(tstPawn, tstPawn, desperate, out Thing t, out ThingDef tt);
            sw.Stop();
            return sw.ElapsedMilliseconds; 

        }

    }
}