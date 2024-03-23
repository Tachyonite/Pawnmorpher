// DebugActions.cs created by Iron Wolf for Pawnmorph on 03/18/2020 1:42 PM
// last updated 03/18/2020  1:42 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienRace;
using HarmonyLib;
using JetBrains.Annotations;
using LudeonTK;
using Pawnmorph.Chambers;
using Pawnmorph.FormerHumans;
using Pawnmorph.Genebank.Model;
using Pawnmorph.GraphicSys;
using Pawnmorph.Hediffs;
using Pawnmorph.HPatches;
using Pawnmorph.Hybrids;
using Pawnmorph.TfSys;
using Pawnmorph.ThingComps;
using Pawnmorph.UserInterface;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Pawnmorph.DebugUtils
{
	static class PMDebugActions
	{
		private const string PM_CATEGORY = "Pawnmorpher";


		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.Action)]
		static void TagAllMutations()
		{
			var cd = Find.World.GetComponent<ChamberDatabase>();

			var mutations = DefDatabase<MutationDef>.AllDefs.Distinct();
			foreach (MutationDef mutationDef in mutations)
			{
				cd.TryAddToDatabase(new MutationGenebankEntry(mutationDef));
			}
		}

		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.Action)]
		static void TagAllGenomeMutations()
		{
			var cd = Find.World.GetComponent<ChamberDatabase>();

			var mutations = DefDatabase<MutationCategoryDef>.AllDefs.Where(d => d.genomeProvider)
															.SelectMany(d => d.AllMutations)
															.Distinct();
			foreach (MutationDef mutationDef in mutations)
			{
				cd.TryAddToDatabase(new MutationGenebankEntry(mutationDef));
			}
		}

		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.Action)]
		static void MassChaoTfPawns()
		{
			var curMap = Find.CurrentMap;
			if (curMap == null) return;
			var hediffDef = MorphTransformationDefOf.FullRandomTFAnyOutcome;
			var mutagen = hediffDef.GetMutagenDef();
			List<Pawn> lst = new List<Pawn>();
			foreach (Pawn pawn in curMap.mapPawns.AllPawns)
			{
				if (!mutagen.CanTransform(pawn)) continue;
				var health = pawn.health?.hediffSet;
				if (health == null) continue;
				lst.Add(pawn);
				if (health.HasHediff(hediffDef))
				{
					continue;
				}

				health.AddDirect(HediffMaker.MakeHediff(hediffDef, pawn));
			}

			foreach (Pawn pawn in lst)
			{
				ForceTransformation(pawn);
			}
		}

		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
		private static void TransformPawnIntoRandomAnimal(Pawn p)
		{
			MutagenDef mutagen = MutagenDefOf.defaultMutagen;
			if (p == null || !mutagen.CanTransform(p)) return;

			PawnKindDef rPK = FormerHumanUtilities.AllRegularFormerHumanPawnkindDefs.RandomElement();
			var newRequest = new TransformationRequest(rPK, p, SapienceLevel.Sapient);
			var tfPawn = mutagen.MutagenCached.Transform(newRequest);
			if (tfPawn == null) return;
			var wComp = Find.World.GetComponent<PawnmorphGameComp>();
			wComp.AddTransformedPawn(tfPawn);
		}

		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
		static void EnableMutationTrackerLogging(Pawn p)
		{
			var comp = p?.GetMutationTracker();
			if (comp != null)
			{

				comp.debug = !comp.debug;

				Log.Message($"logging {(comp.debug ? "enabled" : "disabled")} for {p.Label}");
			}
		}

		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.Action)]
		static void GiveBuildupToAllPawns()
		{
			var map = Find.CurrentMap;
			StringBuilder builder = new StringBuilder();
			foreach (Pawn pawn in PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer).MakeSafe())
			{
				if (pawn == null) continue;
				builder.AppendLine(TryGiveMutagenBuildupToPawn(pawn));
			}

			Log.Message(builder.ToString());
		}

		static string TryGiveMutagenBuildupToPawn(Pawn pawn)
		{
			var buildup = MutagenicBuildupUtilities.AdjustMutagenicBuildup(null, pawn, 0.1f);
			if (buildup > 0)
			{
				return $"gave {buildup} buildup to {pawn.Name}";
			}
			else return $"could not give buildup to {pawn.Name}";
		}


		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
		static void AdaptAllMutations(Pawn p)
		{
			var mTracker = p?.GetMutationTracker();
			if (mTracker == null) return;
			foreach (Hediff_AddedMutation mutation in mTracker.AllMutations)
			{
				var sevAdj = mutation.SeverityAdjust;
				if (sevAdj == null) continue;
				mutation.Severity = sevAdj.NaturalSeverityLimit;
			}

			mTracker.RecalculateMutationInfluences();
			p.health?.capacities?.Notify_CapacityLevelsDirty();
		}

		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.Action)]
		static void TagAllAnimals()
		{
			var gComp = Find.World.GetComponent<PawnmorphGameComp>();
			var database = Find.World.GetComponent<ChamberDatabase>();

			StringBuilder sBuilder = new StringBuilder();
			foreach (var kindDef in DefDatabase<PawnKindDef>.AllDefs)
			{
				var thingDef = kindDef.race;
				if (thingDef.race?.Animal != true) continue;

				if (!database.TryAddToDatabase(new AnimalGenebankEntry(kindDef), out string reason))
				{
					sBuilder.AppendLine($"unable to store {kindDef.label} because {reason}");
				}
				else
				{
					sBuilder.AppendLine($"added {kindDef.label} to the database");
				}
			}

			Log.Message(sBuilder.ToString());

		}

		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
		static void GetInfluenceDebugInfo(Pawn pawn)
		{
			var mutTracker = pawn?.GetMutationTracker();
			if (mutTracker == null)
			{
				Log.Message("no mutation tracker");
				return;
			}

			Log.Message(AnimalClassUtilities.GenerateDebugInfo(mutTracker.AllMutations));

		}

		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
		static void TryExitSapienceState(Pawn pawn)
		{
			var sapienceT = pawn?.GetSapienceTracker();
			if (sapienceT?.CurrentState == null) return;
			var stateName = sapienceT.CurrentState.StateDef.defName;
			try
			{
				sapienceT.ExitState();
			}
			catch (Exception e)
			{
				Log.Error($"caught {e.GetType().Name} while trying to exit sapience state {stateName}!\n{e.ToString().Indented("|\t")}");
			}
		}

		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
		static void AddMutation(Pawn pawn)
		{
			if (pawn == null) return;
			Find.WindowStack.Add(new DebugMenu_AddMutations(pawn));
		}


		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
		static void TagAnimal()
		{

			var db = Find.World.GetComponent<ChamberDatabase>();
			var options = GetTaggableAnimalActions(db).ToList();
			if (options.Count == 0) return;
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(GetTaggableAnimalActions(db)));

		}

		static IEnumerable<DebugMenuOption> GetTaggableAnimalActions([NotNull] ChamberDatabase db)
		{
			foreach (PawnKindDef pawnKindDef in DefDatabase<PawnKindDef>.AllDefs)
			{
				if (!pawnKindDef.race.IsValidAnimal() || db.TaggedAnimals.Contains(pawnKindDef)) continue;
				var tmpPk = pawnKindDef;
				yield return new DebugMenuOption(pawnKindDef.label, DebugMenuOptionMode.Action,
												 () => db.TryAddToDatabase(new AnimalGenebankEntry(tmpPk)));
			}
		}




		static IEnumerable<DebugMenuOption> GetAddMutationOptions([NotNull] Pawn pawn)
		{

			bool CanAddMutationToPawn(MutationDef mDef)
			{
				if (mDef.parts == null)
				{
					return pawn.health.hediffSet.GetFirstHediffOfDef(mDef) == null;
				}

				foreach (BodyPartDef bodyPartDef in mDef.parts)
				{
					foreach (BodyPartRecord record in pawn.GetAllNonMissingParts().Where(p => p.def == bodyPartDef))
					{
						if (!pawn.health.hediffSet.HasHediff(mDef, record)) return true;
					}
				}

				return false;
			}

			void CreateMutationDialog(MutationDef mDef)
			{
				var nMenu = new DebugMenu_AddMutation(mDef, pawn);
				Find.WindowStack.Add(nMenu);
			}


			foreach (MutationDef mutation in MutationDef.AllMutations.Where(CanAddMutationToPawn))
			{
				var tMu = mutation;
				yield return new DebugMenuOption(mutation.label, DebugMenuOptionMode.Action, () => CreateMutationDialog(tMu));
			}

		}

		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
		static void RecruitFormerHuman(Pawn pawn)
		{
			var sapienceState = pawn?.GetSapienceState();
			if (sapienceState?.IsFormerHuman == true)
			{
				InteractionWorker_RecruitAttempt.DoRecruit(pawn.Map.mapPawns.FreeColonists.FirstOrDefault(), pawn);
				DebugActionsUtility.DustPuffFrom(pawn);
			}
		}

		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
		static void ReduceSapience(Pawn pawn)
		{
			var sTracker = pawn?.GetSapienceTracker();
			if (sTracker == null) return;

			sTracker.SetSapience(Mathf.Max(0, sTracker.Sapience - 0.2f));
		}

		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
		static void IncreaseSapience(Pawn pawn)
		{
			var sTracker = pawn?.GetSapienceTracker();
			if (sTracker == null) return;

			sTracker.SetSapience(sTracker.Sapience + 0.2f);

		}

		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
		static void MakeAnimalSapientFormerHuman(Pawn pawn)
		{
			if (pawn == null) return;
			if (pawn.GetSapienceState() != null) return;
			if (!pawn.RaceProps.Animal) return;

			FormerHumanUtilities.MakeAnimalSapient(pawn);

		}
		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
		static void MakeAnimalFormerHuman(Pawn pawn)
		{
			if (pawn == null) return;
			if (pawn.GetSapienceState() != null) return;
			if (!pawn.RaceProps.Animal) return;

			FormerHumanUtilities.MakeAnimalSapient(pawn, Rand.Range(0.1f, 1f));

		}

		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
		static void MakeAnimalRelatedFormerHuman(Pawn pawn)
		{
			if (pawn == null) return;
			if (pawn.GetSapienceState() != null) return;
			if (!pawn.RaceProps.Animal) return;

			var fhRequest = new FHGenerationSettings
			{
				ColonistRelationChanceFactor = 99999,
			};

			var oPawn = FormerHumanPawnGenerator.GenerateRandomHumanForm(pawn, fhRequest); //sloppy but good enough for testing 
			FormerHumanUtilities.MakeAnimalSapient(oPawn, pawn, 0.2f);
			var inst = new TransformedPawnSingle
			{
				original = oPawn,
				animal = pawn,
				mutagenDef = MutagenDefOf.defaultMutagen
			};

			var gameComp = Find.World.GetComponent<PawnmorphGameComp>();
			gameComp.AddTransformedPawn(inst);
			if (pawn.Faction == null && pawn.GetCorrectMap() != null)
			{
				RelatedFormerHumanUtilities.OfferJoinColonyIfRelated(pawn);
			}
		}

		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns)]
		static void TryRevertTransformedPawn(Pawn pawn)
		{
			if (pawn == null) return;
			var gComp = Find.World.GetComponent<PawnmorphGameComp>();
			(TransformedPawn pawn, TransformedStatus status)? tfPawn = gComp?.GetTransformedPawnContaining(pawn);

			TransformedPawn transformedPawn = tfPawn?.pawn;

			if (transformedPawn == null || tfPawn?.status != TransformedStatus.Transformed) return;

			MutagenDef mut = null;
			if (transformedPawn is MergedPawns)
				mut = MutagenDefOf.MergeMutagen;
			else
				mut = MutagenDefOf.defaultMutagen;

			mut.MutagenCached.TryRevert(transformedPawn);
		}


		[DebugAction("General", "Explosion (mutagenic small)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		static void SmallExplosionMutagenic()
		{
			GenExplosion.DoExplosion(UI.MouseCell(), Find.CurrentMap, 10, PMDamageDefOf.MutagenCloud, null);
		}

		[DebugAction("General", "Explosion (mutagenic large)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		static void ExplosionMutagenic()
		{
			GenExplosion.DoExplosion(UI.MouseCell(), Find.CurrentMap, 10, PMDamageDefOf.MutagenCloud_Large, null);

		}

		[DebugAction("General", "Explosion (mutagenic large dirty)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		static void ExplosionMutagenicDirty()
		{
			GenExplosion.DoExplosion(UI.MouseCell(), Find.CurrentMap, 10, PMDamageDefOf.MutagenCloud_Large, null, postExplosionSpawnThingDef: PMThingDefOf.PM_Filth_Slurry, postExplosionSpawnChance: 0.35f, postExplosionSpawnThingCount: 2);

		}

		[DebugAction(category = PM_CATEGORY, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void OpenPartPickerMenu(Pawn pawn)
		{
			if (pawn == null) return;
			Find.WindowStack.Add(new Dialog_PartPicker(pawn, true));
		}

		private static List<DebugMenuOption> GetRaceChangeOptions()
		{
			//var races = RaceGenerator.ImplicitRaces;
			var lst = new List<DebugMenuOption>();
			foreach (MorphDef morph in DefDatabase<MorphDef>.AllDefs)
			{
				MorphDef local = morph;

				lst.Add(new DebugMenuOption(local.label, DebugMenuOptionMode.Tool, () =>
				{
					Pawn pawn = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).OfType<Pawn>().FirstOrDefault();
					if (pawn != null && pawn.RaceProps.intelligence == Intelligence.Humanlike)
						RaceShiftUtilities.ChangePawnToMorph(pawn, local);
				}));
			}

			return lst;
		}

		[DebugAction("Pawnmorpher", "Shift race", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void ShiftRace()
		{
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(GetRaceChangeOptions()));
		}


		[DebugAction("Pawnmorpher", "Make pawn alien.", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void MakePawnAlien(Pawn pawn)
		{
			if (pawn.def is ThingDef_AlienRace == false)
				return;

			List<DebugMenuOption> options = new List<DebugMenuOption>();

			foreach (var alienRace in DefDatabase<ThingDef>.AllDefs.OfType<ThingDef_AlienRace>().Except(RaceGenerator.ImplicitRaces))
			{
				options.Add(new DebugMenuOption(alienRace.LabelCap, DebugMenuOptionMode.Action, () => RaceShiftUtilities.ChangePawnRace(pawn, alienRace)));
			}

			Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
		}


		private static void MakePawnMorph([CanBeNull] MorphDef morph)
		{
			Pawn pawn = Find.CurrentMap.thingGrid
							.ThingsAt(UI.MouseCell())
							.OfType<Pawn>()
							.FirstOrDefault();
			if (pawn == null) return;


			IEnumerable<MutationDef> mutations = morph?.AllAssociatedMutations;
			if (mutations == null)
				return;


			var mutList = mutations.ToList();
			if (mutList.Count == 0)
				return;

			foreach (MutationDef mutation in mutations)
				MutationUtilities.AddMutation(pawn, mutation);

			AdaptAllMutations(pawn);
			RaceShiftUtilities.ChangePawnToMorph(pawn, morph);

			StatWorkerPatches.GetValueUnfinalizedPatch.Invalidate(pawn);
		}

		private static void GivePawnRandomMutations([CanBeNull] MorphDef morph)
		{
			Pawn pawn = Find.CurrentMap.thingGrid
							.ThingsAt(UI.MouseCell())
							.OfType<Pawn>()
							.FirstOrDefault();
			if (pawn == null) return;


			IEnumerable<MutationDef> mutations = morph?.AllAssociatedMutations;
			if (mutations == null)
				mutations = DefDatabase<MutationDef>.AllDefs;



			var mutList = mutations.ToList();
			if (mutList.Count == 0) return;

			int num = Rand.Range(1, Mathf.Min(10, mutList.Count));

			var i = 0;
			List<Hediff_AddedMutation> givenList = new List<Hediff_AddedMutation>();
			List<MutationDef> triedGive = new List<MutationDef>();
			while (i < num && mutList.Count > 0)
			{
				var giver = mutList.RandElement();
				mutList.Remove(giver);
				triedGive.Add(giver);
				var res = MutationUtilities.AddMutation(pawn, giver);
				givenList.AddRange(res);
				i++;
			}

			if (givenList.Count > 0)
			{
				Log.Message($"gave {pawn.Name} [{givenList.Join(m => m.Label)}] from [{triedGive.Join(m => m.defName)}]");
			}
			else
			{
				Log.Message($"could not give {pawn.Name} any from [{triedGive.Join(m => m.defName)}]");
			}
		}

		[DebugAction("Pawnmorpher", "Give random mutations", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void GetRandomMutationsOptions()
		{
			var options = new List<DebugMenuOption>
				{new DebugMenuOption("none", DebugMenuOptionMode.Tool, () => GivePawnRandomMutations(null))};


			foreach (MorphDef morphDef in MorphDef.AllDefs)
			{
				var option = new DebugMenuOption(morphDef.label, DebugMenuOptionMode.Tool,
												 () => GivePawnRandomMutations(morphDef));
				options.Add(option);
			}

			Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
		}


		[DebugAction("Pawnmorpher", "Make morph", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void MakeMorph()
		{
			var options = new List<DebugMenuOption>
				{new DebugMenuOption("none", DebugMenuOptionMode.Tool, () => GivePawnRandomMutations(null))};


			foreach (MorphDef morphDef in MorphDef.AllDefs)
			{
				var option = new DebugMenuOption(morphDef.label, DebugMenuOptionMode.Tool,
												 () => MakePawnMorph(morphDef));
				options.Add(option);
			}

			Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
		}

		[DebugAction("Pawnmorpher", "Recalculate mutation influence", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void RecalculateInfluence(Pawn pawn)
		{
			pawn?.GetComp<MutationTracker>()?.RecalculateMutationInfluences();
		}

		[DebugAction("Pawnmorpher", "Recalculate all colonist mutation influence", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void AllRecalculateInfluence()
		{
			foreach (Pawn colonist in PawnsFinder.AllMaps_FreeColonists)
			{
				RecalculateInfluence(colonist);
			}
		}

		private static void RaceCheck(Pawn pawn)
		{
			if (pawn == null) return;
			if (pawn.IsAnimalOrMerged()) return;
			var oldRace = pawn.def;
			pawn.CheckRace();
			if (pawn.def == oldRace)
			{
				DebugLogUtils.LogMsg(LogLevel.Messages, $"no change in {pawn.Name}");
			}
			else
			{
				DebugLogUtils.LogMsg(LogLevel.Messages, $"{pawn.Name} was {oldRace.defName} and is now {pawn.def.defName}");
			}
		}

		[DebugAction("Pawnmorpher", "Run race check", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void RunRaceCheck(Pawn pawn)
		{
			RaceCheck(pawn);
		}

		[DebugAction("Pawnmorpher", "Run race check on all pawn", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void RunRaceCheck()
		{
			StringBuilder builder = new StringBuilder();
			foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonistsAndPrisonersSpawned)
			{
				RaceCheck(pawn);
			}
		}

		private static void MutationInfo(Pawn pawn)
		{
			var tracker = pawn?.GetMutationTracker();
			if (tracker == null)
			{
				Log.Message($"no tracker on {pawn?.Name?.ToStringFull ?? "NULL"}");
				return;
			}

			StringBuilder builder = new StringBuilder();
			builder.AppendLine($"---{pawn.Name}---");
			builder.AppendLine("---Raw Influence---");
			foreach (KeyValuePair<AnimalClassBase, float> kvp in tracker)
			{
				builder.AppendLine($"{kvp.Key.Label}:{kvp.Value}");
			}

			builder.AppendLine($"---Total={tracker.TotalInfluence} N:{tracker.TotalNormalizedInfluence} NN:{tracker.TotalInfluence / MorphUtilities.GetMaxInfluenceOfRace(pawn.def)}---");


			builder.AppendLine($"---HighestInfluence={tracker.HighestInfluence?.Label ?? "NULL"}---");



			Log.Message(builder.ToString());

		}

		[DebugAction("Pawnmorpher", "Get mutation info", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void GetMutationInfo(Pawn pawn)
		{
			MutationInfo(pawn);
		}


		[DebugAction("Pawnmorpher", "Get mutation info for all pawns", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void GetAllMutationInfo()
		{
			foreach (Pawn pawn in PawnsFinder.AllCaravansAndTravelingTransportPods_Alive)
			{
				MutationInfo(pawn);
			}
		}

		[DebugAction("Pawnmorpher", "Force full transformation", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ForceTransformation(Pawn pawn)
		{
			if (pawn == null)
				return;

			var allHediffs = pawn.health.hediffSet.hediffs;
			if (allHediffs == null)
				return;

			MutagenDef mutagen = MutagenDefOf.defaultMutagen;
			if (!mutagen.CanTransform(pawn))
				return;

			Hediff_MutagenicBase transformHediff = null;
			HediffStage_Transformation transformStage = null;
			foreach (Hediff hediff in allHediffs)
			{
				if (hediff is Hediff_MutagenicBase mutagenic)
				{
					transformStage = mutagenic.def.stages.FirstOrDefault(x => x is HediffStage_Transformation) as HediffStage_Transformation;

					if (transformStage != null)
					{
						transformHediff = mutagenic;
						break;
					}
				}
			}

			if (transformHediff == null)
				return;

			PawnKindDef pawnDef = transformStage.tfTypes.GetTF(transformHediff);

			var newRequest = new TransformationRequest(pawnDef, pawn, SapienceLevel.Sapient);
			var tfPawn = mutagen.MutagenCached.Transform(newRequest);
			if (tfPawn == null)
				return;

			var wComp = Find.World.GetComponent<PawnmorphGameComp>();
			wComp.AddTransformedPawn(tfPawn);
		}

		[DebugAction("Pawnmorpher", "Get initial graphics", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ListPawnInitialGraphics(Pawn pawn)
		{
			var initialComp = pawn.GetComp<InitialGraphicsComp>();
			if (initialComp == null) return;

			Log.Message(initialComp.GetDebugStr());
		}

		[DebugAction("Pawnmorpher", "Get pawn stats cache", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void GetPawnCachedStats(Pawn pawn)
		{
			Log.Message(StatsUtility.GetPawnDebugString(pawn));
		}

		private static void AddAspectAtStage(AspectDef def, Pawn p, int i)
		{
			p.GetAspectTracker()?.Add(def, i);
		}

		private static void AddBackstoryToPawn(Pawn pawn, BackstoryDef def)
		{
			pawn.story.Adulthood = def;
		}

		private static List<DebugMenuOption> GetGiveBackstoriesOptions(Pawn pawn)
		{
			List<DebugMenuOption> options = new List<DebugMenuOption>();
			foreach (BackstoryDef backstoryDef in DefDatabase<BackstoryDef>.AllDefs)
			{
				var def = backstoryDef;
				options.Add(new DebugMenuOption(def.defName, DebugMenuOptionMode.Action, () => AddBackstoryToPawn(pawn, def)));
			}

			return options;
		}

		private static List<DebugMenuOption> GetAddAspectOptions(AspectDef def, Pawn p)
		{
			var outLst = new List<DebugMenuOption>();
			for (var i = 0; i < def.stages.Count; i++)
			{
				AspectStage stage = def.stages[i];
				int i1 = i; //need to make a copy 
				var label = string.IsNullOrEmpty(stage.label) ? def.label : stage.label;
				outLst.Add(new DebugMenuOption($"{i}) {label}", DebugMenuOptionMode.Action,
											   () => AddAspectAtStage(def, p, i1)));
			}

			return outLst;
		}

		private static List<DebugMenuOption> GetAddAspectOptions(Pawn pawn)
		{
			var outLst = new List<DebugMenuOption>();
			AspectTracker tracker = pawn.GetAspectTracker();

			foreach (AspectDef aspectDef in DefDatabase<AspectDef>.AllDefs.Where(d => !tracker.Contains(d))
			) //don't allow aspects to be added more then once 
			{
				AspectDef tmpDef = aspectDef;

				outLst.Add(new DebugMenuOption($"{aspectDef.defName}", DebugMenuOptionMode.Action,
											   () =>
												   Find.WindowStack
													   .Add(new Dialog_DebugOptionListLister(GetAddAspectOptions(tmpDef,
																												 pawn)))));
			}

			return outLst;
		}

		[DebugAction("Pawnmorpher", "Add Aspect", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DoAddAspectToPawn(Pawn p)
		{
			var options = GetAddAspectOptions(p);
			if (options.Count == 0) return;
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
		}

		private static List<DebugMenuOption> GetRemoveAspectOptions(Pawn p)
		{
			var outLst = new List<DebugMenuOption>();


			AspectTracker aspectTracker = p.GetAspectTracker();
			if (aspectTracker == null) return outLst;
			foreach (Aspect aspect in aspectTracker.Aspects.ToList())
			{
				Aspect tmpAspect = aspect;
				outLst.Add(new DebugMenuOption($"{aspect.Label}", DebugMenuOptionMode.Action,
											   () => aspectTracker.Remove(tmpAspect)));
			}

			return outLst;
		}

		[DebugAction("Pawnmorpher", "Remove Aspect", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DoRemoveAspectsOption(Pawn p)
		{
			var options = GetRemoveAspectOptions(p);
			if (options.Count == 0) return;
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
		}

		[DebugAction("Pawnmorpher", "Add Backstory to Sapient Animal", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void DoAddBackstoryToPawn(Pawn pawn)
		{
			if (!pawn.IsFormerHuman()) return;

			Find.WindowStack.Add(new Dialog_DebugOptionListLister(GetGiveBackstoriesOptions(pawn)));

		}

		[DebugAction("Pawnmorpher", "Try Random Hunt", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void TryStartRandomHunt(Pawn pawn)
		{
			if (!pawn.RaceProps.predator) return;
			var prey = FormerHumanUtilities.FindRandomPreyFor(pawn);
			if (prey == null) return;
			var job = new Job(JobDefOf.PredatorHunt, prey)
			{
				killIncappedTarget = true
			};

			pawn.jobs?.StartJob(job, JobCondition.InterruptForced);
		}

		[DebugAction("Pawnmorpher", "Make pawn permanently feral", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void MakePawnPermanentlyFeral(Pawn obj)
		{
			if (obj?.IsFormerHuman() != true) return;

			FormerHumanUtilities.MakePermanentlyFeral(obj);
		}

		[DebugAction("Pawnmorpher", "Restart all mutation progression", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ResetMutationProgression(Pawn pawn)
		{
			var allHediffs = pawn?.health?.hediffSet?.hediffs;
			if (allHediffs == null) return;

			foreach (Hediff_AddedMutation mutation in allHediffs.OfType<Hediff_AddedMutation>())
			{
				mutation.SeverityAdjust?.Restart();
			}
		}

#if DEBUG
		[DebugAction("Pawnmorpher", "List all comps", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void Listcomps(Pawn pawn)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Comps for pawn " + pawn.LabelCap);
			foreach (var item in pawn.AllComps)
			{
				stringBuilder.AppendLine(item.GetType().Name);
			}
			Log.Message(stringBuilder.ToString());
		}
#endif

		[DebugAction("Pawnmorpher", "Reload graphics", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void ResetMutationProgression()
		{
			IEnumerable<Thing> things = Find.CurrentMap.thingGrid
							.ThingsAt(UI.MouseCell())
							.OfType<Thing>();

			foreach (Thing thing in things)
			{
				var data = thing.def.graphicData;
				thing.Graphic.Init(new GraphicRequest(data.graphicClass, data.texPath, data.shaderType.Shader, data.drawSize, data.color, data.colorTwo, data, 0, data.shaderParameters, data.maskPath));
			}
		}

		[DebugAction(category: PM_CATEGORY, name: "Invalidate intelligence.", actionType = DebugActionType.ToolMapForPawns)]
		private static void InvalidateIntelligence(Pawn pawn)
		{
			Intelligence oldInt = pawn.GetIntelligence();
			pawn.InvalidateIntelligence();
			Intelligence newInt = pawn.GetIntelligence();


			Log.Message($"Recalculated intelligence for {pawn.LabelCap}: from {oldInt} to {newInt}");
			HPatches.PawnPatches.QueuePostTickAction(pawn, () =>
			{
				PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn);
				pawn?.needs?.AddOrRemoveNeedsAsAppropriate();

				if (pawn.IsColonist)
					Find.ColonistBar?.MarkColonistsDirty();
			});
		}
	}
}