// InteractionPatches.cs modified by Iron Wolf for Pawnmorph on 12/10/2019 6:31 PM
// last updated 12/10/2019  6:31 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.DefExtensions;
using Pawnmorph.Hybrids;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.HPatches
{
	[StaticConstructorOnStartup]
	internal static class InteractionPatches
	{
		private static readonly FieldInfo _pawnNeedField;

		static InteractionPatches()
		{
			_pawnNeedField = typeof(Need).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		internal static void PatchDelegateMethods([NotNull] Harmony harInstance)
		{
			if (harInstance == null) throw new ArgumentNullException(nameof(harInstance));

			try
			{
				Type mainType = typeof(Toils_Interpersonal);
				var nTypes = mainType
							.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)
							.Where(t => t.IsCompilerGenerated()
											  && t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
												  .Any(m => m.Name.Contains("TryTrain")));





				MethodInfo transpiler =
					typeof(InteractionPatches).GetMethod(nameof(TrainingTranspiler),
														 BindingFlags.Static | BindingFlags.NonPublic);


				IEnumerable<MethodInfo> methods = nTypes.SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
													   .Where(m => m.ReturnType == typeof(void) && !m.IsAbstract && !m.IsConstructor && m.Name.Contains("TryTrain")));
				foreach (MethodInfo method in methods)
				{
					harInstance.Patch(method, transpiler: new HarmonyMethod(transpiler));

				}

			}
			catch (Exception e)
			{
				Log.Error($"unable to patch interaction delegates!\n{e}");
			}
		}

		static float GetEffectiveTamingStatFor(Pawn initiator, Pawn recipient)
		{
			if (initiator == null)
			{
				Log.Error("unable to get initiator for effective stat patch!");
				return 1;
			}

			float sVal = initiator.GetStatValue(StatDefOf.TameAnimalChance);

			if (recipient == null)
			{
				Log.ErrorOnce($"unable to recipient for {initiator.Name} interaction!", initiator.GetHashCode());
				return sVal;
			}

			var morph = initiator.def.GetMorphOfRace();
			if (morph != null)
				sVal *= morph.GetAssociatedAnimalBonus(recipient.def, 2);
			return sVal;
		}

		static float GetEffectiveTrainingStatFor(Pawn initiator, Pawn recipient)
		{
			if (initiator == null)
			{
				Log.Error("unable to get initiator for effective stat patch!");
				return 1;
			}

			float sVal = initiator.GetStatValue(StatDefOf.TrainAnimalChance);

			if (recipient == null)
			{
				Log.ErrorOnce($"unable to recipient for {initiator.Name} training interaction!", initiator.GetHashCode());
				return sVal;
			}

			var morph = initiator.def.GetMorphOfRace();
			if (morph != null)
				sVal *= morph.GetAssociatedAnimalBonus(recipient.def, 2);
			return sVal;
		}


		static IEnumerable<CodeInstruction> TrainingTranspiler(IEnumerable<CodeInstruction> instructions)
		{

			FieldInfo statFld =
				typeof(StatDefOf).GetField(nameof(StatDefOf.TrainAnimalChance), BindingFlags.Static | BindingFlags.Public);
			MethodInfo extCallMethod =
				typeof(StatExtension).GetMethod(nameof(StatExtension.GetStatValue),
												BindingFlags.Static | BindingFlags.Public);

			var subMethod =
				typeof(InteractionPatches).GetMethod(nameof(GetEffectiveTrainingStatFor),
													 BindingFlags.Static | BindingFlags.NonPublic);


			var insArr = instructions.ToArray();
			const int len = 4;


			CodeInstruction[] subArr = new CodeInstruction[len];
			for (int i = 0; i < insArr.Length - len; i++)
			{
				for (int j = 0; j < len; j++)
				{
					subArr[j] = insArr[i + j];
				}

				if (subArr[0].opcode != OpCodes.Ldloc_0) continue;
				if (subArr[1].opcode != OpCodes.Ldsfld || (FieldInfo)subArr[1].operand != statFld) continue;
				if (subArr[2].opcode != OpCodes.Ldc_I4_1) continue;
				if (subArr[3].opcode != OpCodes.Call || (MethodInfo)subArr[3].operand != extCallMethod) continue;

				insArr[i + 1].opcode = OpCodes.Ldloc_1;
				insArr[i + 1].operand = null;
				insArr[i + 2].operand = null;
				insArr[i + 2].opcode = OpCodes.Nop;
				insArr[i + 3].operand = subMethod;
				break;
			}

			return insArr;
		}

		private static float GetEffectiveNeed(Need_Suppression need)
		{
			var p = (Pawn)_pawnNeedField.GetValue(need);
			float? sLevel = p.GetSapienceLevel();

			if (sLevel == null) return need.MaxLevel;
			return (FormerHumanUtilities.GetSapienceWillDebuff(sLevel.Value) + 1) * need.MaxLevel;
		}

		private static float GetEffectiveResistance(Pawn p, float mulVal)
		{
			float? qSapience = p.GetSapienceLevel();
			if (qSapience == null) return p.guest.resistance - mulVal;

			float s = FormerHumanUtilities.GetSapienceWillDebuff(qSapience.Value) + 1;

			return p.guest.resistance - mulVal * s;
		}

		[HarmonyPatch(typeof(InteractionWorker_RecruitAttempt), nameof(InteractionWorker_RecruitAttempt.Interacted))]
		static class TamePatch
		{
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instruction)
			{
				var instLst = instruction.ToList();

				FieldInfo statFld =
					typeof(StatDefOf).GetField(nameof(StatDefOf.TameAnimalChance), BindingFlags.Static | BindingFlags.Public);
				MethodInfo extCallMethod =
					typeof(StatExtension).GetMethod(nameof(StatExtension.GetStatValue),
													BindingFlags.Static | BindingFlags.Public);

				var subMethod =
					typeof(InteractionPatches).GetMethod(nameof(GetEffectiveTamingStatFor),
														 BindingFlags.Static | BindingFlags.NonPublic);

				const int len = 4;
				CodeInstruction[] subArr = new CodeInstruction[len];

				for (int i = 0; i < instLst.Count - len; i++)
				{
					for (int j = 0; j < len; j++)
					{
						subArr[j] = instLst[i + j];
					}

					if (subArr[0].opcode != OpCodes.Ldarg_1) continue;
					if (subArr[1].opcode != OpCodes.Ldsfld || (FieldInfo)subArr[1].operand != statFld) continue;
					if (subArr[2].opcode != OpCodes.Ldc_I4_1) continue;
					if (subArr[3].opcode != OpCodes.Call || (MethodInfo)subArr[3].operand != extCallMethod) continue;

					instLst[i + 1].opcode = OpCodes.Ldarg_2;
					instLst[i + 1].operand = null;
					instLst[i + 2].opcode = OpCodes.Call;
					instLst[i + 2].operand = subMethod;
					instLst[i + 3].opcode = OpCodes.Nop;
					instLst[i + 3].operand = null;
					break;

				}
				return instLst;
			}
		}

		[HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.CompatibilityWith))]
		private static class RelationshipPatches
		{
			[HarmonyPostfix]
			public static void CompatibilityWithPostfix(Pawn_RelationsTracker __instance, Pawn otherPawn, ref float __result,
														Pawn ___pawn)
			{
				if (__result > 0) return;
				if (___pawn.IsHumanlike() != otherPawn.IsHumanlike() || ___pawn == otherPawn)
				{
					__result = 0f;
					return;
				}

				float x = Mathf.Abs(___pawn.ageTracker.AgeBiologicalYearsFloat - otherPawn.ageTracker.AgeBiologicalYearsFloat);
				float num = GenMath.LerpDouble(0f, 20f, 0.45f, -0.45f, x);
				num = Mathf.Clamp(num, -0.45f, 0.45f);
				float num2 = __instance.ConstantPerPawnsPairCompatibilityOffset(otherPawn.thingIDNumber);
				__result = num + num2;
			}
		}

		[HarmonyPatch(typeof(Pawn_RelationsTracker), nameof(Pawn_RelationsTracker.SecondaryLovinChanceFactor))]
		private static class SecondaryLoveFactorPatch
		{
			[HarmonyPostfix]
			private static void SecondaryLovinChanceFactor(Pawn ___pawn, Pawn otherPawn, ref float __result)
			{
				if (__result > 0) return;

				if (___pawn.IsHumanlike() != otherPawn.IsHumanlike() || ___pawn == otherPawn)
				{
					__result = 0f;
					return;
				}
				if (___pawn.story != null && ___pawn.story.traits != null)
				{
					if (___pawn.story.traits.HasTrait(TraitDefOf.Asexual))
					{
						__result = 0;
						return;
					}
					if (!___pawn.story.traits.HasTrait(TraitDefOf.Bisexual))
					{
						if (___pawn.story.traits.HasTrait(TraitDefOf.Gay))
						{
							if (otherPawn.gender != ___pawn.gender)
							{
								__result = 0f;
								return;
							}
						}
						else if (otherPawn.gender == ___pawn.gender)
						{
							__result = 0f;
							return;
						}
					}
				}

				float ageBiologicalYearsFloat = ___pawn.ageTracker.AgeBiologicalYearsFloat;
				float ageBiologicalYearsFloat2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;
				if (ageBiologicalYearsFloat < 16f || ageBiologicalYearsFloat2 < 16f)
				{
					__result = 0f;
					return;
				}
				var num = 1f;
				if (___pawn.gender == Gender.Male)
				{
					float min = ageBiologicalYearsFloat - 30f;
					float lower = ageBiologicalYearsFloat - 10f;
					float upper = ageBiologicalYearsFloat + 3f;
					float max = ageBiologicalYearsFloat + 10f;
					num = GenMath.FlatHill(0.2f, min, lower, upper, max, 0.2f, ageBiologicalYearsFloat2);
				}
				else if (___pawn.gender == Gender.Female)
				{
					float min2 = ageBiologicalYearsFloat - 10f;
					float lower2 = ageBiologicalYearsFloat - 3f;
					float upper2 = ageBiologicalYearsFloat + 10f;
					float max2 = ageBiologicalYearsFloat + 30f;
					num = GenMath.FlatHill(0.2f, min2, lower2, upper2, max2, 0.2f, ageBiologicalYearsFloat2);
				}

				float num2 = Mathf.InverseLerp(16f, 18f, ageBiologicalYearsFloat);
				float num3 = Mathf.InverseLerp(16f, 18f, ageBiologicalYearsFloat2);
				var num4 = 0f;
				if (otherPawn.IsHumanlike()) num4 = otherPawn.GetStatValue(StatDefOf.PawnBeauty);
				var num5 = 1f;
				if (num4 < 0f)
					num5 = 0.3f;
				else if (num4 > 0f) num5 = 2.3f;
				__result = num * num2 * num3 * num5;
			}
		}

		[HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.TryInteractWith))]
		private static class TryInteractWithPatch
		{
			[HarmonyPostfix]
			private static void AddInteractionThoughts(Pawn ___pawn, [NotNull] Pawn recipient, [NotNull] InteractionDef intDef, bool __result)
			{


				if (!__result) return;
				if ((recipient.IsFormerHuman() || recipient.GetSapienceState()?.StateDef == SapienceStateDefOf.Animalistic)
				 && recipient.needs?.mood != null)
				{
					ThoughtDef
						memory = intDef.GetModExtension<InstinctEffector>()
									  ?.thought; //hacky, should come up with a better solution eventually 
					if (memory == null) return;

					if (DebugLogUtils.ShouldLog(LogLevel.Messages))
					{
						string msg = $"giving {recipient.Name} memory {memory.defName}";
						Log.Message(msg);
					}


					//social thoughts to? 
					recipient.TryGainMemory(memory);
				}


			}

			[HarmonyPrefix]
			private static bool SubstituteInteraction(Pawn recipient, ref InteractionDef intDef, Pawn ___pawn)
			{



				var ext = intDef.GetModExtension<InteractionGroupExtension>();
				InteractionDef alt = ext?.TryGetAlternativeFor(___pawn, recipient);
				if (alt != null)
				{
					intDef = alt;
				}

				return true;
			}
		}

		[HarmonyPatch(typeof(InteractionWorker_RecruitAttempt), "Interacted")]
		private static class ResistanceSapienceInfluenceP
		{
			[HarmonyTranspiler]
			private static IEnumerable<CodeInstruction> ResistanceSapienceInfluenceTranspilerPatch(
				IEnumerable<CodeInstruction> inst)
			{
				CodeInstruction[] instArr = inst.ToArray();

				const int len = 4;
				var sliceArr = new CodeInstruction[len];
				FieldInfo guestField = typeof(Pawn).GetField(nameof(Pawn.guest), BindingFlags.Public | BindingFlags.Instance);
				FieldInfo resField =
					typeof(Pawn_GuestTracker).GetField(nameof(Pawn_GuestTracker.resistance),
													   BindingFlags.Public | BindingFlags.Instance);
				MethodInfo subMethod =
					typeof(InteractionPatches).GetMethod("GetEffectiveResistance", BindingFlags.NonPublic | BindingFlags.Static);

				for (var i = 0; i < instArr.Length - len; i++)
				{
					for (var j = 0; j < len; j++) sliceArr[j] = instArr[i + j];


					if (sliceArr[0].opcode != OpCodes.Ldfld || (FieldInfo)sliceArr[0].operand != guestField) continue;
					if (sliceArr[1].opcode != OpCodes.Ldfld || (FieldInfo)sliceArr[1].operand != resField) continue;
					if (sliceArr[2].opcode != OpCodes.Ldloc_S) continue;
					if (sliceArr[3].opcode != OpCodes.Sub) continue;

					sliceArr[0].opcode = OpCodes.Nop;
					sliceArr[1].opcode = OpCodes.Nop;
					sliceArr[3].opcode = OpCodes.Call;
					sliceArr[3].operand = subMethod;
					break;
				}

				return instArr;
			}
		}

		[NotNull]
		private static readonly List<Pawn> _workingList = new List<Pawn>();

#if true



		[NotNull]
		private static readonly StringBuilder _dbgBuilder = new StringBuilder();
		[HarmonyPatch(typeof(Pawn_InteractionsTracker), "TryInteractRandomly")]
		static class DebugInteractionPatch
		{
			static void Postfix(Pawn_InteractionsTracker __instance, Pawn ___pawn, ref bool __result)
			{
				_workingList.Clear();
				_dbgBuilder.Clear();
				if (___pawn?.IsFormerHuman() == true)
				{
					if (InteractionUtility.CanInitiateRandomInteraction(___pawn) && !__instance.InteractedTooRecentlyToInteract())
					{
						List<Pawn> collection = ___pawn.Map?.mapPawns?.SpawnedPawnsInFaction(___pawn.Faction);
						if ((collection?.Count ?? 0) == 0)
						{
							return;
						}

						_workingList.AddRange(collection);
						List<InteractionDef> allDefsListForReading = DefDatabase<InteractionDef>.AllDefsListForReading;
						for (int i = 0; i < _workingList.Count; i++)
						{
							Pawn p = _workingList[i];
							if (p != ___pawn)
							{
								if (__instance.CanInteractNowWith(p))
								{
									if (InteractionUtility.CanReceiveRandomInteraction(p))
									{
										if (!___pawn.HostileTo(p))
										{
											var tups = allDefsListForReading
													  .Select((InteractionDef x) => (x, GetWeight(x, p)))
													  .Where(t => t.Item2 > 0);

											var elem = tups.TryRandomElementByWeight(t => t.Item2, out var selTup);

											if (elem)
											{
												__result = __instance.TryInteractWith(p, selTup.x);
												break;
											}

										}
										else { }
									}
									else
									{
									}
								}
								else
								{
								}
							}
						}

						float GetWeight(InteractionDef x, Pawn p)
						{
							return (!__instance.CanInteractNowWith(p, x)) ? 0f : x.Worker.RandomSelectionWeight(___pawn, p);
						}



						if (_dbgBuilder.Length > 0)
							Log.Message(_dbgBuilder.ToString());

					}

				}


			}
		}
#endif

		[HarmonyPatch(typeof(InteractionWorker_Suppress), "Interacted")]
		private static class ResistanceSapienceInfluenceSuppression
		{
			[HarmonyTranspiler]
			private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> inst)
			{
				CodeInstruction[] codeArr = inst.ToArray();
				const int len = 5;
				var sliceArr = new CodeInstruction[len];

				MethodInfo callVirtMInfo = typeof(Need)
										  .GetProperty(nameof(Need.MaxLevel), BindingFlags.Public | BindingFlags.Instance)
										  .GetMethod;
				MethodInfo replaceMethod =
					typeof(InteractionPatches).GetMethod("GetEffectiveNeed", BindingFlags.Static | BindingFlags.NonPublic);


				for (var i = 0; i < codeArr.Length - len; i++)
				{
					for (var j = 0; j < len; j++) sliceArr[j] = codeArr[i + j];

					if (sliceArr[0].opcode != OpCodes.Mul) continue;
					if (sliceArr[1].opcode != OpCodes.Ldloc_0) continue;
					if (sliceArr[2].opcode != OpCodes.Callvirt || (MethodInfo)sliceArr[2].operand != callVirtMInfo) continue;
					if (sliceArr[3].opcode != OpCodes.Mul) continue;


					sliceArr[2].opcode = OpCodes.Call;
					sliceArr[2].operand = replaceMethod;
				}

				return codeArr;
			}
		}
	}
}