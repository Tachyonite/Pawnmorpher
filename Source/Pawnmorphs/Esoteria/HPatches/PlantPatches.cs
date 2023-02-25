// PlantPatches.cs modified by Iron Wolf for Pawnmorph on 12/31/2019 3:25 PM
// last updated 12/31/2019  3:25 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.Plants;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.HPatches
{
	static class PlantPatches
	{

		[HarmonyPatch(typeof(WorkGiver_GrowerSow)), HarmonyPatch(nameof(WorkGiver_GrowerSow.JobOnCell))]
		static class SowerJobOnCellPatch
		{
			[HarmonyPrefix]
			static bool RespectRestrictionsPatch([NotNull] Pawn pawn, IntVec3 c, bool forced, ref Job __result)
			{
				if (pawn.Map == null) return true;

				var plant = WorkGiver_Grower.CalculateWantedPlantDef(c, pawn.Map);
				if (plant != null)
				{
					if (!plant.IsValidFor(pawn))
					{
						__result = null;
						return false;
					}
				}

				return true;
			}

			[HarmonyPostfix]
			static void SubstituteJob([NotNull] Pawn pawn, IntVec3 c, bool forced, ref Job __result)
			{

				if (__result?.def == JobDefOf.Sow)
				{
					var plant = __result.plantDefToSow;

					if (plant?.IsMutantPlant() == true)
					{

						__result = JobMaker.MakeJob(PMJobDefOf.PM_MutagenicSow, __result.targetA);
						__result.playerForced = forced;
						__result.plantDefToSow = plant;

					}
				}
			}
		}


		[HarmonyPatch(typeof(WorkGiver_PlantSeed))]
		static class WorkGiverPlantSeedPatches
		{
			[HarmonyPatch(nameof(WorkGiver_GrowerSow.JobOnThing))]
			[HarmonyPostfix]
			static void SubstituteJob([NotNull] Pawn pawn, Thing t, bool forced, ref Job __result)
			{

				if (__result?.def == JobDefOf.PlantSeed)
				{
					CompPlantable compPlantable = t.TryGetComp<CompPlantable>();
					var plant = compPlantable?.Props?.plantDefToSpawn;
					if (plant == null) return;
					if (plant?.IsMutantPlant() == true)
					{
						__result = JobMaker.MakeJob(PMJobDefOf.PM_MutagenicSow, __result.targetA, __result.targetB);
						__result.playerForced = forced;
						__result.plantDefToSow = compPlantable.Props.plantDefToSpawn;
						__result.count = 1;
					}
				}
			}
		}


		[HarmonyPatch(typeof(WorkGiver_GrowerSow)), HarmonyPatch("ExtraRequirements")]
		static class ExtraRequirementsPatch
		{
			[HarmonyPostfix]
			static void RespectRequirementsPatch([NotNull] IPlantToGrowSettable settable, [NotNull] Pawn pawn, ref bool __result)
			{
				if (!__result) return;
				IntVec3 c;
				if (settable is Zone_Growing growingZone)
				{
					c = growingZone.Cells[0];
				}
				else
				{
					c = ((Thing)settable).Position;
				}

				var plant = WorkGiver_Grower.CalculateWantedPlantDef(c, pawn.Map);
				if (plant == null) return;
				__result = plant.IsValidFor(pawn);

				if (__result && plant.IsMutantPlant())
				{
					//check ideo 
					__result = PMHistoryEventDefOf.SowMutagenicPlants.DoerWillingToDo(pawn);
				}
			}
		}


		[HarmonyPatch]
		public static class PlantHarvestTPatch
		{
			public static MethodInfo match = typeof(Plant).GetMethod("YieldNow");
			public static MethodInfo replaceWith = typeof(PlantHarvestTPatch).GetMethod("YieldNowPatch");
			//Note, this is being called implicitly by harmony with reflection, 
			public static MethodBase TargetMethod()
			{
				Type mainType = typeof(JobDriver_PlantWork);


				BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
				try
				{
					Type iteratorType = mainType.GetNestedTypes(bindingFlags).First(t => t.FullName.Contains("c__DisplayClass"));




					var method = iteratorType?.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
											  .First(m => m.ReturnType == typeof(void));


					return method;
				}
				catch (InvalidOperationException iO)
				{
					var names = string.Join(",", mainType.GetNestedTypes(bindingFlags).Select(t => t.FullName));

					throw new InvalidOperationException($"unable to find type with \"c__DisplayClass\" among \"{names}\"", iO);
				}
				catch (Exception e)
				{
					Log.Error($"unable to preform plant patch! caught {e}");
					throw;
				}

			}
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
			{
				foreach (CodeInstruction i in instr)
				{
					if ((i.operand as MethodInfo) == match)
					{
						Log.Message("Instruction insertion complete!");
						yield return new CodeInstruction(OpCodes.Ldloc_0);
						yield return new CodeInstruction(OpCodes.Call, replaceWith);
					}
					else
					{
						yield return i;
					}
				}
			}
			public static int YieldNowPatch(Plant p, Pawn actor)
			{


				if (p is SpecialHarvestFailPlant sPlant)
				{
					return sPlant.GetYieldNow(actor);
				}

				return p.YieldNow(); // Whatever you want to do here
			}
		}
	}
}