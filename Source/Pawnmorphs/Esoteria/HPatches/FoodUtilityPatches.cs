// FoodUtilityPatches.cs modified by Iron Wolf for Pawnmorph on 01/19/2020 4:33 PM
// last updated 01/19/2020  4:33 PM

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
	static class FoodUtilityPatches
	{

		[HarmonyPatch(typeof(FoodUtility)), HarmonyPatch(nameof(FoodUtility.AddFoodPoisoningHediff))]
		static class FoodPoisoningIgnoreChance
		{
			static bool Prefix(Pawn pawn, FoodPoisonCause cause)
			{
				if (cause == FoodPoisonCause.DangerousFoodType)
				{
					var ignoreChance = 1 - pawn.GetStatValue(PMStatDefOf.DangerousFoodSensitivity);
					if (Rand.Value < ignoreChance) return false; //do not add food poisoning 
				}
				else if (cause == FoodPoisonCause.Rotten)
				{
					var ignoreChance = 1 - pawn.GetStatValue(PMStatDefOf.RottenFoodSensitivity);
					if (Rand.Value < ignoreChance) return false; //do not add food poisoning 
				}

				return true;
			}
		}


		[HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.BestFoodSourceOnMap))]
		static class FixBestFoodSourceForFormerHumans
		{
			[NotNull]
			private static readonly MethodInfo _isToolUser = typeof(FormerHumanUtilities).GetMethod(nameof(FormerHumanUtilities.IsToolUser));

			[NotNull]
			private static readonly MethodInfo _isHumanlike =
				typeof(FormerHumanUtilities).GetMethod(nameof(FormerHumanUtilities.IsHumanlike), new[] { typeof(Pawn) });




			[NotNull]
			private static readonly MethodInfo _targetMethodSig =
				typeof(Pawn).GetProperty(nameof(Pawn.RaceProps)).GetGetMethod();

			[NotNull]
			private static readonly MethodInfo _tUTarget =
				typeof(RaceProperties).GetProperty(nameof(RaceProperties.ToolUser)).GetGetMethod();


			[NotNull]
			private static readonly MethodInfo _humanlikeTarget =
				typeof(RaceProperties).GetProperty(nameof(RaceProperties.Humanlike)).GetGetMethod();

			[HarmonyPrefix]
			static bool Prefix(
				ref Thing __result,
				[NotNull] Pawn getter,
				[NotNull] Pawn eater,
				bool desperate,
				ref ThingDef foodDef,
				FoodPreferability maxPref = FoodPreferability.MealLavish,
				bool allowPlant = true,
				bool allowDrug = true,
				bool allowCorpse = true,
				bool allowDispenserFull = true,
				bool allowDispenserEmpty = true,
				bool allowForbidden = false,
				bool allowSociallyImproper = false,
				bool allowHarvest = false,
				bool forceScanWholeMap = false,
				bool ignoreReservations = false,
				FoodPreferability minPrefOverride = FoodPreferability.Undefined)
			{


				if (ShouldUseOptimizedCode(eater))
				{
					__result = PMFoodUtilities.BestFoodSourceOnMapOptimized(getter, eater, desperate, out foodDef, maxPref,
																			allowPlant, allowDrug, allowCorpse,
																			allowDispenserFull,
																			allowDispenserEmpty, allowForbidden,
																			allowSociallyImproper, allowHarvest,
																			forceScanWholeMap, ignoreReservations,
																			minPrefOverride);
					return false;
				}
				
				return true;
			}

			private static bool ShouldUseOptimizedCode(Pawn eater)
			{
				return eater.IsHumanlike() && (eater.RaceProps.foodType & (FoodTypeFlags.Plant | FoodTypeFlags.Tree)) != 0;
			}

			[HarmonyTranspiler]
			static
				IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var codes = instructions.ToList(); //convert the code instructions to a list so we can do 2 at a time 

				for (var i = 0; i < codes.Count - 1; i++)
				{
					int j = i + 1;
					CodeInstruction instI = codes[i];
					//need to be more specific because the patched method is longer, don't want to patch stuff we don't intend to 
					CodeInstruction instJ = codes[j];
					if (instI.opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == _targetMethodSig && instJ.opcode == OpCodes.Callvirt)
					{
						var jOperand = instJ.operand as MethodInfo;
						if (jOperand == null) continue;
						MethodInfo rpOp;

						if (jOperand == _humanlikeTarget) //check which method we're going to replace
						{
							rpOp = _isHumanlike;
						}
						else if (jOperand == _tUTarget)
						{
							rpOp = _isToolUser;
						}
						else
						{
							rpOp = null;
						}

						if (rpOp == null) continue; //if it's not either just ignore it

						instI.opcode =
							OpCodes.Call; //replace the callVirt to get_RaceProps with call to FormerHumanUtilities.IsToolUser 
						instI.operand = rpOp; //set the method that the call op is going to call 
						instJ.opcode = OpCodes.Nop; //replace the second  callVirt to a No op so we don't fuck up the stack 
					}
				}

				return codes;
			}
		}



	}
}