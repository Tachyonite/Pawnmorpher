// DebugLogUtils.FoodOptimizations.cs created by Iron Wolf for Pawnmorph on //2020 
// last updated 03/06/2020  4:48 PM

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using HarmonyLib;
using JetBrains.Annotations;
using LudeonTK;
using Pawnmorph.Utilities;
using RimWorld;
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


		[DebugOutput(FOOD_OP_CATEGORY)]
		static void GetRacialFoodStats()
		{
			StringBuilder builder = new StringBuilder();
			foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.MakeSafe())
			{
				var race = thingDef.race;
				if (race == null) continue;

				builder.AppendLine($"{thingDef.defName},{race.baseBodySize},{race.baseHungerRate}");

			}

			Log.Message(builder.ToString());
		}

		[DebugOutput(FOOD_OP_CATEGORY, true)]
		static void CheckIfPawnsEatPlants()
		{
			Map map = Find.CurrentMap;
			if (map == null) return;
			var pawns = map.mapPawns.FreeColonistsSpawned.Where(p => p.RaceProps.Humanlike
																			 && (p.RaceProps.foodType
																			   & (FoodTypeFlags.Plant | FoodTypeFlags.Tree))
																			 != 0);
			var plants = map.listerThings.AllThings.OfType<Plant>().Where(p => p.def.ingestible != null).Take(20).ToList();
			StringBuilder builder = new StringBuilder();
			List<string> entries = new List<string>();
			foreach (Pawn pawn in pawns)
			{
				entries.Clear();
				foreach (Plant plant in plants)
				{
					bool isIngestible = plant.def.IsNutritionGivingIngestible;
					bool canEatNow = plant.IngestibleNow;
					entries.Add($"{{{plant.Label},{nameof(isIngestible)}:{isIngestible},{nameof(canEatNow)}:{canEatNow}}}");
				}

				builder.AppendLine($"{pawn.Name}:[{entries.Join(s => s)}]");
			}

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

		private const int ITERATIONS = 1;
		static void TestFoodFunction([NotNull] FoodFinderFunc func, [NotNull] List<Pawn> testers, [NotNull] List<float> results, StringBuilder messageBuilder)
		{

			List<(Plant plant, ThingDef eatingDef, Pawn eater)> plantsFound = new List<(Plant plant, ThingDef eatingDef, Pawn eater)>();
			for (int i = 0; i < ITERATIONS; i++)
			{
				Stopwatch sWatch = Stopwatch.StartNew();
				foreach (Pawn p in testers)
				{
					if (p == null || p.Dead || p.Destroyed || p.Map == null) continue;

					try
					{
						ThingDef tDef;
						var t = TestFoodFunction(func, p, out tDef);
						if (t is Plant plant && p.IsHumanlike())
						{
							plantsFound.Add((plant, tDef, p));
						}
					}
					catch (Exception e)
					{
						Log.Message($"caught {e.GetType().Name} while testing {p.Name}\n{e}");
					}
				}




				var dur = sWatch.ElapsedMilliseconds;
				sWatch.Stop();
				results.Add(dur);
				if (plantsFound.Count > 0)
				{


					var lStr = plantsFound.Join(tup => $"{tup.plant.def.defName}, {tup.eatingDef.defName}, {tup.eater.Name}",
												"\n");
					messageBuilder.AppendLine(lStr);

				}


			}
		}


		static Thing TestFoodFunction([NotNull] FoodFinderFunc func, [NotNull] Pawn tstPawn, out ThingDef oDef)
		{
			bool desperate = tstPawn.needs.food.CurCategory == HungerCategory.Starving;
			func(tstPawn, tstPawn, desperate, out Thing thing, out oDef);
			return thing;
		}

	}
}