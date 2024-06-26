// PawnmorphPatches.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on //2019 
// last updated 08/11/2019  9:54 AM

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using AlienRace;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.DefExtensions;
using Pawnmorph.FormerHumans;
using Pawnmorph.HPatches;
using Pawnmorph.Hybrids;
using Pawnmorph.Thoughts;
using Pawnmorph.Utilities;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace Pawnmorph
{
	[StaticConstructorOnStartup]
	internal static class PawnmorphPatches
	{
		private static readonly Type patchType = typeof(PawnmorphPatches);
		[NotNull]
		private static readonly MethodInfo _animalTabWorkerMethod;

		private static Harmony _harmonyInstance;

		private static readonly ILookup<string, string> _modPatches = new KeyValuePair<string, string>[]
		{
			new ("jecrell.doorsexpanded", "DoorsExpanded.Building_DoorExpanded:PawnCanOpen"),
			new ("kentington.saveourship2", "SaveOurShip2.Building_ShipAirlock:PawnCanOpen"),

			new ("petetimessix.simplesidearms", "PeteTimesSix.SimpleSidearms.Extensions:IsValidSidearmsCarrier"),
			new ("petetimessix.simplesidearms", "SimpleSidearms.rimworld.CompSidearmMemory:GetMemoryCompForPawn"),
		}.ToLookup(x => x.Key, x => x.Value);

		static PawnmorphPatches()
		{
#if DEBUG
			Stopwatch stopwatch = Stopwatch.StartNew();
#endif


			_animalTabWorkerMethod =
				typeof(PawnmorphPatches).GetMethod(nameof(AnimalTabWorkerMethod), BindingFlags.Static | BindingFlags.NonPublic);



			var harmonyInstance = new Harmony("com.pawnmorpher.mod"); //shouldn't this be different? 
			_harmonyInstance = harmonyInstance;
			harmonyInstance.Patch(
								  AccessTools.Method(typeof(FoodUtility),
													 nameof(FoodUtility.ThoughtsFromIngesting)),
								  null,
								  new HarmonyMethod(patchType, nameof(ThoughtsFromIngestingPostfix))
								 );

#if true
			// Job patches.
			harmonyInstance.Patch(AccessTools.Method(typeof(JobDriver_Ingest),
													  "PrepareToIngestToils"),
								   new HarmonyMethod(patchType, nameof(PrepareToIngestToilsPrefix))
								  );
#endif

			try
			{
				PatchCaravanUI(harmonyInstance);
			}
			catch (Exception e)
			{
				Log.Error($"Pawnmorpher: encountered {e.GetType().Name} while patching caravan UI delegates\n{e}");
			}

			try
			{
				MassPatchFormerHumanChecks(harmonyInstance);
			}
			catch (Exception e)
			{
				Log.Error($"Pawnmorpher: encountered {e.GetType().Name} while mass patching former human functions\n{e}");

			}

			try
			{
				PawnObserverPatches.PreformPatches(harmonyInstance);
			}
			catch (Exception e)
			{
				Log.Error($"Pawnmorpher: unable to patch pawn observer:\n{e}");
			}

			try
			{
				DoAnimalPatches(harmonyInstance);
			}
			catch (Exception e)
			{
				Log.Error($"Pawnmorpher: encountered {e.GetType().Name} while patching animal tab worker\n{e}");
			}


			try
			{
				PatchMods(harmonyInstance);
			}
			catch (Exception e)
			{
				Log.Error($"PM: caught error {e.GetType()} while patching mods! \n{e}");
			}

			try
			{

				harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
			}
			catch (Exception e)
			{
				Log.Error($"Pawnmorpher cannot perform harmony patches! caught {e.GetType().Name}\n{e}");
			}

			try
			{
				DebugPatches.DoDebugPatches(harmonyInstance);
			}
			catch (Exception e)
			{
				Log.Error($"Pawnmorpher cannot perform debugging patches! caught {e.GetType().Name}\n{e}");
			}

			ConversionUtilityPatches.PreformPatches(harmonyInstance);
			ThoughtWorkerPatches.DoPatches(harmonyInstance);
			InteractionPatches.PatchDelegateMethods(harmonyInstance);

#if DEBUG
			stopwatch.Stop();
			Log.Message($"[{DateTime.Now.TimeOfDay}][Pawnmorpher]: Harmony patching finished in {stopwatch.ElapsedMilliseconds}ms");
#endif
		}

		private static void PatchMods([NotNull] Harmony harmonyInstance)
		{
			if (LoadedModManager.RunningMods.Any(m => m.PackageId == "roolo.giddyupcore"))
			{
				GiddyUpPatch.PatchGiddyUp(harmonyInstance);
			}
		}


		/// <summary>
		/// substitutes all instances of RaceProps Humanlike, Animal, and Tooluser with their equivalent in FormerHumanUtilities
		/// </summary>
		/// <param name="instructions">The code instructions.</param>
		/// <exception cref="System.ArgumentNullException">codeInstructions</exception>
		public static IEnumerable<CodeInstruction> SubstituteFormerHumanMethodsPatch([NotNull] IEnumerable<CodeInstruction> instructions)
		{
			if (instructions == null) throw new ArgumentNullException(nameof(instructions));
			List<CodeInstruction> codeInstructions = instructions.ToList();
			for (var i = 0; i < codeInstructions.Count - 1; i++)
			{
				int j = i + 1;
				CodeInstruction opI = codeInstructions[i];
				CodeInstruction opJ = codeInstructions[j];
				if (opI == null || opJ == null) continue;
				//the segment we're interested in always start with pawn.get_RaceProps() (ie pawn.RaceProps) 
				if (opI.opcode == OpCodes.Callvirt && (MethodInfo)opI.operand == PatchUtilities.RimworldGetRaceMethod)
				{
					//signatures we care about always have a second callVirt 
					if (opJ.opcode != OpCodes.Callvirt) continue;

					var jMethod = opJ.operand as MethodInfo;
					bool patched;
					//figure out which method, if any, we're going to be replacing 
					if (jMethod == PatchUtilities.RimworldIsAnimalMethod)
					{
						patched = true;
						opI.operand = _animalTabWorkerMethod;
					}
					else
					{
						patched = false;
					}

					if (patched)
					{
						//now clean up if we did any patching 

						opI.opcode = OpCodes.Call; //always uses call 

						//replace opJ with nop (no operation) so we don't fuck up the stack 
						opJ.opcode = OpCodes.Nop;
						opJ.operand = null;
					}
				}
			}

			return codeInstructions;
		}



		static bool AnimalTabWorkerMethod(Pawn pawn)
		{
			return pawn.RaceProps.Animal || pawn.GetIntelligence() == Intelligence.Animal;
		}

		static void DoAnimalPatches([NotNull] Harmony harmonyInstance)
		{
			var staticFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
			var instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;


			var tpMethod = typeof(PawnmorphPatches).GetMethod(nameof(SubstituteFormerHumanMethodsPatch), staticFlags);
			//animal tabs 
			var methods = typeof(MainTabWindow_Animals).GetNestedTypes(staticFlags | instanceFlags)//looking for delegates used by the animal tab 
													   .Where(t => t.IsCompilerGenerated())
													   .SelectMany(t => t.GetMethods(instanceFlags).Where(m => m.HasSignature(typeof(Pawn))));

			foreach (MethodInfo methodInfo in methods)
			{
				harmonyInstance.Patch(methodInfo, transpiler: new HarmonyMethod(tpMethod));
			}

		}

		struct MethodInfoSt
		{
			public MethodInfo methodInfo;
			public bool debug;

			public static implicit operator MethodInfoSt(MethodInfo info)
			{
				return new MethodInfoSt { methodInfo = info, debug = false };
			}
		}

		const BindingFlags INSTANCE_FLAGS = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
		const BindingFlags STATIC_FLAGS = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
		private const BindingFlags ALL = INSTANCE_FLAGS | STATIC_FLAGS;
		private static void MassPatchFormerHumanChecks([NotNull] Harmony harmonyInstance)
		{
			var staticFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
			var instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;




			List<MethodInfoSt> methodsToPatch = new List<MethodInfoSt>();

			StringBuilder modPatchingLog = new StringBuilder();
			foreach (string mod in _modPatches.Select(x => x.Key))
			{
				if (LoadedModManager.RunningMods.Any(x => x.PackageId == mod))
				{
					foreach (var methodIdentity in _modPatches[mod])
					{
						MethodInfo method = AccessTools.Method(methodIdentity);

						if (method != null)
							methodsToPatch.Add(method);
						else
							modPatchingLog.AppendLine("Unable to patch " + _modPatches[mod]);
					}
				}
			}

			if (modPatchingLog.Length > 0)
				Log.Warning("[Pawnmorpher] Failed patching mods:" + Environment.NewLine + modPatchingLog.ToString());

			//bed stuff 
			var bedUtilType = typeof(RestUtility);
			var canUseBedMethod = bedUtilType.GetMethod(nameof(RestUtility.CanUseBedEver), staticFlags);
			methodsToPatch.Add(canUseBedMethod);

			//wildman
			AddWildmanMethods(methodsToPatch);

			//door 
			methodsToPatch.Add(new MethodInfoSt() { methodInfo = typeof(Building_Door).GetMethod(nameof(Building_Door.PawnCanOpen), instanceFlags) });


			//assign comp 
			methodsToPatch.Add(typeof(CompAssignableToPawn).GetMethod(nameof(CompAssignableToPawn.DrawGUIOverlay), instanceFlags));

			//map pawns 
			var methods = typeof(MapPawns).GetMethods(instanceFlags).Where(m => m.HasSignature(typeof(Faction)) || m.HasSignature(typeof(Faction), typeof(bool)));
			methodsToPatch.AddRange(methods.Select(m => new MethodInfoSt() { methodInfo = m }));
			methodsToPatch.Add(typeof(MapPawns).GetProperty(nameof(MapPawns.SpawnedColonyAnimals), instanceFlags).GetMethod);

			methodsToPatch.Add(AccessTools.Method(typeof(Pawn_NeedsTracker), "ShouldHaveNeed"));
			
			//jobs and toils 
			methodsToPatch.Add(typeof(JobDriver_Ingest).GetMethod("PrepareToIngestToils", instanceFlags));
			methodsToPatch.Add(typeof(LordJob_Joinable_MarriageCeremony).GetMethod("IsGuest", instanceFlags));

			//down/death thoughts 
			methodsToPatch.Add(typeof(PawnDiedOrDownedThoughtsUtility).GetMethod(nameof(PawnDiedOrDownedThoughtsUtility.GetThoughts), staticFlags));

			//incidents 
			PatchIncidents(methodsToPatch);

			//interaction patch
			methodsToPatch.Add(typeof(HealthCardUtility).GetMethod("CreateSurgeryBill", STATIC_FLAGS));
			methodsToPatch.Add(AccessTools.Method(typeof(RimWorld.FloatMenuMakerMap), nameof(RimWorld.FloatMenuMakerMap.ChoicesAtFor)));
			PatchRescueMessage(methodsToPatch);

			methodsToPatch.Add(typeof(InteractionUtility).GetMethod(nameof(InteractionUtility.CanReceiveRandomInteraction), STATIC_FLAGS));
			methodsToPatch.Add(typeof(InteractionUtility).GetMethod(nameof(InteractionUtility.CanInitiateRandomInteraction), STATIC_FLAGS));
			methodsToPatch.Add(typeof(Pawn_InteractionsTracker).GetMethod(nameof(Pawn_InteractionsTracker.SocialFightPossible), INSTANCE_FLAGS));
			methodsToPatch.Add(typeof(Pawn_InteractionsTracker).GetMethod("TryInteractWith", INSTANCE_FLAGS));

			//relations
			methodsToPatch.Add(typeof(Pawn_RelationsTracker).GetMethod(nameof(Pawn_RelationsTracker.OpinionOf), INSTANCE_FLAGS));
			methodsToPatch.Add(typeof(Pawn_RelationsTracker).GetMethod(nameof(Pawn_RelationsTracker.OpinionExplanation), INSTANCE_FLAGS));
			methodsToPatch.Add(typeof(Pawn_RelationsTracker).GetMethod("Tick_CheckStartMarriageCeremony", INSTANCE_FLAGS));
			methodsToPatch.Add(typeof(Pawn_RelationsTracker).GetMethod("CheckAppendBondedAnimalDiedInfo", INSTANCE_FLAGS));
			methodsToPatch.Add(typeof(Pawn_RelationsTracker).GetMethod(nameof(Pawn_RelationsTracker.Notify_RescuedBy), INSTANCE_FLAGS));

			// Social tab
			methodsToPatch.Add(typeof(SocialCardUtility).GetMethod("DrawMyOpinion", STATIC_FLAGS));
			methodsToPatch.Add(typeof(SocialCardUtility).GetMethod("DrawHisOpinion", STATIC_FLAGS));
			methodsToPatch.Add(typeof(SocialCardUtility).GetMethod("Recache", STATIC_FLAGS));
			methodsToPatch.Add(typeof(SocialCardUtility).GetMethod("GetPawnRowTooltip", STATIC_FLAGS));

			// Food restrictions tracker
			methodsToPatch.Add(AccessTools.PropertyGetter(typeof(Pawn_FoodRestrictionTracker), nameof(Pawn_FoodRestrictionTracker.Configurable)));


			methodsToPatch.Add(typeof(Dialog_InfoCard).GetMethod(nameof(Dialog_InfoCard.DoWindowContents), INSTANCE_FLAGS));

			AddJobGiverMethods(methodsToPatch);


			AddDesignatorMethods(methodsToPatch);
			AddThoughtWorkerPatches(methodsToPatch);
			PatchSpawnedMasteredPawns(methodsToPatch);

			AddRitualPatches(methodsToPatch);

			methodsToPatch.Add(typeof(RitualRoleAssignments).GetMethod(nameof(RitualRoleAssignments.PawnNotAssignableReason), staticFlags));

			//socialization 
			methodsToPatch.Add(typeof(SocialProperness).GetMethod(nameof(SocialProperness.IsSociallyProper), new Type[] { typeof(Thing), typeof(Pawn), typeof(bool), typeof(bool) }));

			//roamer patches 
			methodsToPatch.Add(typeof(MentalStateWorker_Roaming).GetMethod(nameof(MentalStateWorker_Roaming.CanRoamNow), staticFlags));
			methodsToPatch.Add(typeof(MentalState_Manhunter).GetMethod(nameof(MentalState_Manhunter.ForceHostileTo), INSTANCE_FLAGS, null, new[] { typeof(Thing) }, null));
			methodsToPatch.Add(typeof(Pawn).GetMethod(nameof(Pawn.ThreatDisabledBecauseNonAggressiveRoamer), instanceFlags));
			methodsToPatch.Add(AccessTools.PropertyGetter(typeof(Pawn), nameof(Pawn.IsColonist)));

			//caravan and inventory patch 
			methodsToPatch.Add(typeof(MassUtility).GetMethod(nameof(MassUtility.CanEverCarryAnything), staticFlags)); 


			//pathfinding 
            methodsToPatch.Add(typeof(Pawn_PathFollower).GetMethod("TryEnterNextPathCell", instanceFlags));
			methodsToPatch.Add(typeof(Building_Trap).GetMethod(nameof(Building_Trap.KnowsOfTrap), instanceFlags));

			//now patch them 
			foreach (var methodInfo in methodsToPatch)
			{
				if (methodInfo.methodInfo == null)
				{
#if DEBUG
					// Only show configuration errors in debug mode.
					Log.Warning($"encountered null in {nameof(MassPatchFormerHumanChecks)}!");
#endif

					continue;
				}

				harmonyInstance.ILPatchCommonMethods(methodInfo.methodInfo, methodInfo.debug);
			}

			StringBuilder builder = new StringBuilder();
			builder.AppendLine("Patched:");
			foreach (var methodInfo in methodsToPatch)
			{
				if (methodInfo.methodInfo == null) continue;
				builder.AppendLine($"{methodInfo.methodInfo.DeclaringType.FullName + "." + methodInfo.methodInfo.Name}");
			}
			Log.Message(builder.ToString());
			DebugLogUtils.LogMsg(LogLevel.Messages, builder.ToString());
		}



		internal static void PatchHumanoidRace(WorkGiverDef wgd)
		{
			if (wgd != null)
			{
				_harmonyInstance.Patch(AccessTools.Method(wgd.giverClass, "JobOnThing"), null, new HarmonyMethod(patchType, "GenericJobOnThingPostfix"));
				MethodInfo methodInfo2 = AccessTools.Method(wgd.giverClass, "HasJobOnThing");
				if ((object)methodInfo2 != null && methodInfo2.IsDeclaredMember())
				{
					_harmonyInstance.Patch(methodInfo2, null, new HarmonyMethod(patchType, "GenericHasJobOnThingPostfix"));
				}
			}
		}


		private static void PatchIncidents([NotNull] List<MethodInfoSt> methodsToPatch)
		{
			methodsToPatch.Add(typeof(CompAnimalInsanityPulser).GetMethod("DoAnimalInsanityPulse", INSTANCE_FLAGS));
			IEnumerable<Type> tps = typeof(IncidentWorker_AnimalInsanitySingle)
								   .GetNestedTypes(ALL)
								   .Where(t => t.IsCompilerGenerated());
			IEnumerable<MethodInfoSt> methods = tps.SelectMany(t => t.GetMethods(INSTANCE_FLAGS))
												   .Where(m => m.HasSignature(typeof(Pawn)))
												   .Select(m => (MethodInfoSt)m);
			methodsToPatch.AddRange(methods);
		}

		private static void PatchRescueMessage([NotNull] List<MethodInfoSt> methodsToPatch)
		{
			IEnumerable<Type> tps = typeof(FloatMenuMakerMap)
								   .GetNestedTypes(ALL)
								   .Where(t => t.IsCompilerGenerated() && t.GetFields().Any(x => x.Name == "victim" && x.FieldType == typeof(Pawn)));

			IEnumerable<MethodInfoSt> methods = tps.SelectMany(t => t.GetMethods(INSTANCE_FLAGS))
												   .Where(m => m.HasSignature() && m.Name.Contains("<AddHumanlikeOrders>"))
												   .Select(m => (MethodInfoSt)m);
			methodsToPatch.AddRange(methods);
		}

		private static void AddRitualPatches([NotNull] List<MethodInfoSt> methodsToPatch)
		{
			var tp = typeof(Precept_Ritual);
			var methods = tp.GetNestedTypes(ALL)
							.Where(t => t.IsCompilerGenerated())
							.SelectMany(t => t.GetMethods(INSTANCE_FLAGS))
							.Where(m => m.HasSignature(typeof(Pawn), typeof(bool), typeof(bool)))
							.ToList();


			//ritual outcomes 
			methodsToPatch.Add(typeof(RitualOutcomeComp_ParticipantCount).GetMethod("Counts", INSTANCE_FLAGS));


			methodsToPatch.Add(AccessTools.Method(typeof(JobDriver_Scarify), nameof(JobDriver_Scarify.AvailableOnNow)));
			//methodsToPatch.Add(typeof(JobDriver_Scarify).GetMethod(nameof(JobDriver_Scarify.AvailableOnNow), STATIC_FLAGS));
			methodsToPatch.AddRange(methods.Select(m => (MethodInfoSt)m));
			methodsToPatch.Add(typeof(RitualRoleAssignments).GetMethod(nameof(RitualRoleAssignments.CanEverSpectate), INSTANCE_FLAGS));

			AddRolePatches(methodsToPatch);
		}

		private static void AddRolePatches([NotNull] List<MethodInfoSt> methodsToPatch)
		{

			/*var types = new Type[]
            {
                typeof(RitualRoleColonist),
                typeof(RitualRoleWarden),
                typeof(RitualRoleConvertee),
                typeof(RitualRoleBlindingTarget),
                typeof(RitualRoleScarificationTarget)
            };*/
			var types = new List<Type>();
			types = typeof(RitualRole).AllSubclassesNonAbstract();


			/*var methods = types.Select(t => t.GetMethod(nameof(RitualRole.AppliesToPawn), INSTANCE_FLAGS))
                               .Where(m => !m.IsAbstract)
                               .Select(m => (MethodInfoSt) m);*/
			var methods = types.Select(t => AccessTools.DeclaredMethod(t, nameof(RitualRole.AppliesToPawn)))
							   .Where(m => m != null)
							   .Select(m => (MethodInfoSt)m);
			methodsToPatch.AddRange(methods);

		}

		// PawnUtility.SpawnedMasteredPawns
		private static void PatchSpawnedMasteredPawns([NotNull] List<MethodInfoSt> methodsToPatch)
		{
			IEnumerable<Type> tps = typeof(PawnUtility)
								   .GetNestedTypes(ALL)
								   .Where(t => t.IsCompilerGenerated() && t.Name.Contains("<SpawnedMasteredPawns>"));

			IEnumerable<MethodInfoSt> methods = tps.SelectMany(t => t.GetMethods(INSTANCE_FLAGS))
												   .Where(m => m.HasSignature() && m.Name.Contains("MoveNext"))
												   .Select(m => (MethodInfoSt)m);
			methodsToPatch.AddRange(methods);
		}



		private static void AddThoughtWorkerPatches([NotNull] List<MethodInfoSt> methodsToPatch)
		{
			methodsToPatch.Add(typeof(ThoughtWorker_Precept_IdeoDiversity_Uniform).GetMethod("ShouldHaveThought", INSTANCE_FLAGS));
		}

		private static void AddWildmanMethods([NotNull] List<MethodInfoSt> methodsToPatch)
		{
			var methods = typeof(WildManUtility).GetMethods(STATIC_FLAGS).Where(m => m.ReturnType == typeof(bool));
			methodsToPatch.AddRange(methods.Select(m => new MethodInfoSt() { methodInfo = m }));
		}

		private static void AddDesignatorMethods([NotNull] List<MethodInfoSt> methodsToPatch)
		{
			methodsToPatch.Add(typeof(Designator_ReleaseAnimalToWild).GetMethod(nameof(Designator.CanDesignateThing), BindingFlags.Instance | BindingFlags.Public));
		}

		private static void AddJobGiverMethods([NotNull] List<MethodInfoSt> methodsToPatch)
		{
			methodsToPatch.Add(typeof(WorkGiver_ReleaseAnimalsToWild).GetMethod(nameof(WorkGiver_Scanner.HasJobOnThing), INSTANCE_FLAGS));
		}


		private static void PatchCaravanUI([NotNull] Harmony harmInstance)
		{

			var cUIType = typeof(CaravanUIUtility);
			var flg = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;


			bool CorrectSignature(MethodInfo methodInfo)
			{
				var parameters = methodInfo.GetParameters();
				if (parameters.Length != 1) return false;
				return parameters[0].ParameterType == typeof(TransferableOneWay) || parameters[0].ParameterType == typeof(Pawn);
			}

			IEnumerable<Type> allInnerTypes = cUIType.GetNestedTypes(flg).Where(IsCompilerGenerated);
			//add in the delegates from LordToil_PrepareCaravan_GatherAnimals
#pragma warning disable 612
			allInnerTypes = allInnerTypes.Concat(typeof(LordToil_PrepareCaravan_GatherAnimals)
#pragma warning restore 612
												.GetNestedTypes(flg)
												.Where(IsCompilerGenerated));

			var allMethods = allInnerTypes.SelectMany(t => t.GetMethods(flg).Where(CorrectSignature));
			var tsMethodInfo = typeof(PawnmorphPatches).GetMethod(nameof(CaravanDelegatePatch), flg);
			foreach (MethodInfo methodInfo in allMethods)
			{
				var hMethod = new HarmonyMethod(tsMethodInfo);
				harmInstance.Patch(methodInfo, transpiler: hMethod);
			}

		}



		static IEnumerable<CodeInstruction> CaravanDelegatePatch(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> ilList = instructions.ToList();
			MethodInfo methodSig1 = typeof(Pawn).GetProperty(nameof(Pawn.RaceProps)).GetGetMethod();
			MethodInfo methodSig2 = typeof(RaceProperties).GetProperty(nameof(RaceProperties.Animal)).GetGetMethod();
			MethodInfo replaceMethod = typeof(FormerHumanUtilities).GetMethod(nameof(FormerHumanUtilities.IsAnimal));
			for (var i = 0; i < ilList.Count - 1; i++)
			{
				int j = i + 1;
				CodeInstruction opI = ilList[i];
				CodeInstruction opJ = ilList[j];

				// looking for 
				// pawn.RaceProps.Animal 
				// which is 
				// race =  pawn.get_RaceProps()
				// race.get_Animal()
				if (opI.opcode == OpCodes.Callvirt
				 && (MethodInfo)opI.operand == methodSig1
				 && opJ.opcode == OpCodes.Callvirt
				 && (MethodInfo)opJ.operand == methodSig2)
				{
					//now replace with one call to FormerHumanUtilities.IsAnimal(Pawn)
					opI.opcode = OpCodes.Call;
					opI.operand = replaceMethod;

					//replace race.get_Animal() op with nop (no operation) so we don't fuck up the stack 
					opJ.opcode = OpCodes.Nop;
					opJ.operand = null;
				}
			}

			return ilList;
		}

		static bool IsCompilerGenerated(Type type)
		{
			if (type.GetCustomAttribute<CompilerGeneratedAttribute>() != null) return true;
			if (type.Name.Contains(">") || type.Name.Contains("<")) return true;
			return false;
		}



		[NotNull]
		private static SortedSet<IFoodThoughtModifier> _modifiersCache = new SortedSet<IFoodThoughtModifier>(ModComparer);

		class FoodModifierComparer : IComparer<IFoodThoughtModifier>
		{
			/// <summary>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</summary>
			/// <returns>Value Condition Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.</returns>
			/// <param name="x">The first object to compare.</param>
			/// <param name="y">The second object to compare.</param>
			public int Compare(IFoodThoughtModifier x, IFoodThoughtModifier y)
			{
				if (ReferenceEquals(x, y)) return 0;
				if (x == null) return -1;
				if (y == null) return 1;
				return x.Priority.CompareTo(y.Priority);
			}
		}

		[NotNull] private static FoodModifierComparer ModComparer { get; } = new FoodModifierComparer();

		public static void ThoughtsFromIngestingPostfix(Pawn ingester, Thing foodSource, ref List<FoodUtility.ThoughtFromIngesting> __result)
		{
			ApplyMorphFoodThoughts(ingester, foodSource, __result);

			//now apply any modifiers if present 

			//first clear the cache 
			_modifiersCache.Clear();

			List<Hediff> hediffs = ingester.health?.hediffSet?.hediffs;
			if (hediffs != null)
			{
				for (int i = hediffs.Count - 1; i >= 0; i--)
				{
					Hediff item = hediffs[i];

					if (item is IFoodThoughtModifier modifier)
						_modifiersCache.Add(modifier);

					HediffStage stage = item.CurStage;
					if (stage != null && stage is IFoodThoughtModifier modifierStage)
						_modifiersCache.Add(modifierStage);
				}
			}

			AspectTracker tracker = ingester.GetAspectTracker();
			if (tracker?.AspectCount > 0)
			{
				foreach (Aspect aspect in tracker.Aspects)
				{
					if (aspect is IFoodThoughtModifier modifier)
						_modifiersCache.Add(modifier);
				}
			}

			if (_modifiersCache.Count > 0)
			{
				foreach (IFoodThoughtModifier foodThoughtModifier in _modifiersCache)
				{
					foodThoughtModifier.ModifyThoughtsFromFood(foodSource, __result);
				}
			}
		}

		private static void ApplyMorphFoodThoughts(Pawn ingester, Thing foodSource, List<FoodUtility.ThoughtFromIngesting> foodThoughts)
		{
			var meatSource = FoodUtility.GetMeatSourceCategory(foodSource.def);
			//TODO figure out precept 
			if (RaceGenerator.TryGetMorphOfRace(ingester.def, out MorphDef morphDef))
			{
				AteThought cannibalThought = morphDef.raceSettings?.thoughtSettings?.ateAnimalThought;
				if (cannibalThought == null) return;
				if (ingester?.story?.traits == null) return;
				bool cannibal = ingester.story.traits.HasTrait(PMTraitDefOf.Cannibal);

				if (cannibal == false)
				{
					if (foodSource.def == morphDef.race.race.meatDef)
					{
						TryAddIngestThought(ingester, cannibalThought.thought, null, foodThoughts, foodSource.def, meatSource);
						return;
					}

					ThingDef comp = foodSource.TryGetComp<CompIngredients>()?.ingredients?.FirstOrDefault(def => def == morphDef.race.race.meatDef);
					if (comp != null)
					{
						TryAddIngestThought(ingester, cannibalThought.ingredientThought, null, foodThoughts, foodSource.def, meatSource);
					}
				}
			}
			else
			{
				var fHStatus = ingester.GetQuantizedSapienceLevel();
				if (fHStatus == null || fHStatus == SapienceLevel.PermanentlyFeral) return;

				ThoughtDef thought = GetCannibalThought(ingester, foodSource);
				if (thought != null)
				{
					TryAddIngestThought(ingester, thought, null, foodThoughts, foodSource.def, meatSource);

				}
			}

		}

		private static void TryAddIngestThought(Pawn ingester, ThoughtDef def, Precept fromPrecept, List<FoodUtility.ThoughtFromIngesting> ingestThoughts, ThingDef foodDef, MeatSourceCategory meatSourceCategory)
		{
			FoodUtility.ThoughtFromIngesting thoughtFromIngesting = new FoodUtility.ThoughtFromIngesting
			{
				thought = def,
				fromPrecept = fromPrecept
			};
			FoodUtility.ThoughtFromIngesting item = thoughtFromIngesting;
			if (ingester?.story?.traits != null)
			{
				if (!ingester.story.traits.IsThoughtFromIngestionDisallowed(def, foodDef, meatSourceCategory))
				{
					ingestThoughts.Add(item);
				}
			}
			else
			{
				ingestThoughts.Add(item);
			}
		}

		private static ThoughtDef GetCannibalThought(Pawn ingester, Thing foodSource, ThingDef raceDef = null)
		{
			raceDef = raceDef ?? ingester.def;
			CannibalThoughtStatus cannibalStatus = PMFoodUtilities.GetStatusOfFoodForPawn(raceDef, foodSource);

			if (cannibalStatus == CannibalThoughtStatus.None)
				return null;

			var ext = ingester.def.GetModExtension<FormerHumanSettings>();
			FoodThoughtSettings foodSettings = ext?.foodThoughtSettings;

			bool isCannibal = ingester.story?.traits?.HasTrait(PMTraitDefOf.Cannibal) ?? false;


			//assign the correct cannibal thoughts, one for pawns without the cannibal trait the other for pawns with it 
			if (cannibalStatus == CannibalThoughtStatus.Direct)
			{
				if (isCannibal)
					return foodSettings?.cannibalThoughtGood ?? PMThoughtDefOf.FHDefaultCannibalGoodThought_Direct;

				return foodSettings?.cannibalThought ?? PMThoughtDefOf.FHDefaultCannibalThought_Direct;
			}
			else if (cannibalStatus == CannibalThoughtStatus.Ingredient)
			{
				if (isCannibal)
					return foodSettings?.cannibalThoughtIngredientGood ?? PMThoughtDefOf.FHDefaultCannibalGoodThought_Ingredient;

				return foodSettings?.cannibalThoughtIngredient ?? PMThoughtDefOf.FHDefaultCannibalThought_Ingredient;
			}

			return null;
		}

		private static bool PrepareToIngestToilsPrefix(JobDriver __instance, ref IEnumerable<Toil> __result) //TODO figure out how to turn this into a transpiler patch 
		{
			Thing thing = __instance.job.targetA.Thing;
			if (RaceGenerator.IsMorphRace(__instance.pawn.def))
			{
				if (thing.def.ingestible == null) 
					return true;

				FoodTypeFlags flg = thing.def.ingestible.foodType & (FoodTypeFlags.Tree | FoodTypeFlags.Plant);
				if (flg != 0)
				{
					__result = new[]
					{
						ReserveFoodIfWillIngestWholeStack(__instance),
						Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch)
					};

					return false;
				}
			}

			return true;
		}

		private static Toil ReserveFoodIfWillIngestWholeStack(JobDriver driver)
		{
			return new Toil
			{
				initAction = delegate
				{
					if (driver.pawn.Faction == null) return;

					Thing thing = driver.job.GetTarget(TargetIndex.A).Thing;
					if (driver.pawn.carryTracker.CarriedThing == thing) return;

					int num = FoodUtility.WillIngestStackCountOf(driver.pawn, thing.def,
																 thing.GetStatValue(StatDefOf.Nutrition));
					if (num >= thing.stackCount)
					{
						if (!thing.Spawned)
						{
							driver.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
							return;
						}

						driver.pawn.Reserve(thing, driver.job);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant,
				atomicWithPrevious = true
			};
		}
	}
}
