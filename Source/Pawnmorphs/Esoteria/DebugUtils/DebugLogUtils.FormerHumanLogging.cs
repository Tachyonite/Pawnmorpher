// DebugLogUtils.FormerHumanLogging.cs created by Iron Wolf for Pawnmorph on //2020 
// last updated 03/01/2020  10:28 AM

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using JetBrains.Annotations;
using LudeonTK;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

#pragma warning disable 1591
namespace Pawnmorph.DebugUtils
{
	public static partial class DebugLogUtils
	{
		private const string FH_CATEGORY = MAIN_CATEGORY_NAME + "-Former Humans";

		[DebugOutput(category = FH_CATEGORY, onlyWhenPlaying = true)]
		static void LogFilthValues()
		{
			var map = Find.CurrentMap;
			if (map == null) return;
			var formerHumans = map.mapPawns?.AllPawns?.Where(m => m.IsFormerHuman() || m.GetSapienceState() != null);

			StringBuilder builder = new StringBuilder();

			foreach (Pawn pawn in formerHumans.MakeSafe())
			{
				var baseStat = pawn.GetStatValue(StatDefOf.FilthRate);
				var Adjusted = pawn.GetFilthStat();
				builder.AppendLine($"{pawn.Name}[{pawn.GetQuantizedSapienceLevel()}] baseFilth:{baseStat} adjusted:{Adjusted}");
			}

			Log.Message(builder.ToString());
		}


		[DebugOutput(category = FH_CATEGORY, onlyWhenPlaying = true)]
		static void GetFormerHumanResponsibility()
		{
			var mp = Find.CurrentMap;
			if (mp == null) return;
			var allFhs = FormerHumanUtilities.AllMaps_FormerHumans;
			var worldComp = Find.World.GetComponent<PawnmorphGameComp>();
			if (worldComp == null) return;
			StringBuilder sBuilder = new StringBuilder();
			foreach (Pawn formerHuman in allFhs)
			{
				var tf = worldComp.GetTransformedPawnContaining(formerHuman)?.pawn;
				if (tf == null)
				{
					sBuilder.AppendLine($"{formerHuman.Name}:Wild");
				}
				else
				{
					sBuilder.AppendLine($"{formerHuman.Name}:{tf.FactionResponsible?.Name ?? "Wild"}");
				}
			}
			Log.Message(sBuilder.ToString());
		}

		[DebugOutput(category = FH_CATEGORY)]
		static void PrintAnimalThinkTree()
		{
			var tTree = DefDatabase<ThinkTreeDef>.GetNamed("Animal");
			if (tTree == null) return;

			var outStr = TreeUtilities.PrettyPrintTree(tTree.thinkRoot, GetChildren, GetNodeLabel);

			Log.Message(outStr);

		}


		[DebugOutput(category = FH_CATEGORY, onlyWhenPlaying = true)]
		static void LogAnimalisticTrainingInfo()
		{
			var animalisticPawns =
				Find.CurrentMap.mapPawns.AllPawns.Where(p => p.GetSapienceState()?.StateDef == SapienceStateDefOf.Animalistic);

			StringBuilder builder = new StringBuilder();

			foreach (Pawn pawn in animalisticPawns)
			{
				var training = pawn.training;
				if (training == null) continue;

				builder.AppendLine($"{pawn.Name}:");


				builder.AppendLine($"{nameof(Pawn_TrainingTracker.NextTrainableToTrain)}: {training.NextTrainableToTrain()?.defName ?? "NULL"}");


			}

			Log.Message(builder.ToString());

		}

		[DebugOutput(category = FH_CATEGORY)]
		static void PrintHumanlikeThinkTree()
		{
			ThinkTreeDef tree = DefDatabase<ThinkTreeDef>.GetNamed("Humanlike");
			if (tree == null) return;
			string outStr = TreeUtilities.PrettyPrintTree(tree.thinkRoot, GetChildren, GetNodeLabel);
			Log.Message(outStr);
		}

		[DebugOutput(category = FH_CATEGORY, onlyWhenPlaying = true)]
		static void LogSapienceInfo()
		{
			StringBuilder builder = new StringBuilder();
			foreach (Pawn pawn in PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer))
			{
				var sapienceTracker = pawn.GetSapienceTracker();
				Need_Control need = sapienceTracker?.SapienceNeed;
				if (need == null || sapienceTracker.CurrentState == null) continue;

				builder.AppendLine($"{pawn.Name}[{pawn.ThingID}]: in state {sapienceTracker.CurrentState.StateDef.defName}");

				float curLevel = need.CurLevel;
				float curLevelPercent = need.CurLevelPercentage;
				float limit = need.Limit;
				var limitStat = pawn.GetStatValue(PMStatDefOf.SapienceLimit);
				float limitPercent = need.Limit / need.MaxLevel;

				builder.AppendLine($"|\t{nameof(curLevel)}:{curLevel}={sapienceTracker.SapienceLevel}");
				builder.AppendLine($"|\t{nameof(curLevelPercent)}:{curLevelPercent}");
				builder.AppendLine($"|\t{nameof(limitStat)}:{limitStat}");
				builder.AppendLine($"|\t{nameof(limit)}:{limit}");
				builder.AppendLine($"|\t{nameof(limitPercent)}:{limitPercent}");
				builder.AppendLine($"|\t{nameof(Pawn.IsColonist)}:{pawn.IsColonist}");
			}

			Log.Message(builder.ToString());

		}


		[DebugOutput(category = FH_CATEGORY, onlyWhenPlaying = true)]
		static void TestFormerHumanDoorPatch()
		{

			var doors = Find.CurrentMap.listerThings.AllThings.OfType<Building_Door>().ToList();
			var allFHs = Find.CurrentMap.listerThings.AllThings.OfType<Pawn>().Where(p => p.IsFormerHuman()).ToList();

			if (doors.Count == 0)
			{
				Log.Message("No doors on map?");
				return;
			}

			if (allFHs.Count == 0)
			{
				Log.Message("no former humans on map");
				return;
			}

			string BuildFHInfo(Pawn formerHuman)
			{
				return $"{formerHuman.Name}/{formerHuman.LabelShort}[{formerHuman.GetQuantizedSapienceLevel()}]";
			}

			string BuildDoorStr(Building_Door bDoor)
			{
				return $"{bDoor.ThingID} def=\"{bDoor.def.defName}\" faction={bDoor.Faction?.Name ?? "None"}";
			}

			StringBuilder builder = new StringBuilder();

			//patch info
			var methodInfo =
				typeof(Building_Door).GetMethod(nameof(Building_Door.PawnCanOpen), BindingFlags.Instance | BindingFlags.Public);

			Patches patchInfo = Harmony.GetPatchInfo(methodInfo);

			string GetPatchInfo(Patch patch)
			{
				return $"{patch.owner}:{{\"{nameof(Patch.index)}\":{patch.index},\"priority\":{patch.priority}}}";
			}


			if (patchInfo == null)
			{
				builder.AppendLine($"{nameof(Building_Door.PawnCanOpen)} is not patched!");
			}
			else
			{
				builder.AppendLine($"Patch info for {nameof(Building_Door.PawnCanOpen)}:");
				foreach (Patch postfix in patchInfo.Postfixes.MakeSafe())
				{
					builder.AppendLine(GetPatchInfo(postfix).Indented("|\t"));
				}

				builder.AppendLine("---------------Pawn Info------------------");
			}

			foreach (Building_Door door in doors)
			{
				builder.AppendLine($"testing door {BuildDoorStr(door)}:");

				foreach (Pawn formerHuman in allFHs)
				{
					var canPass = door.PawnCanOpen(formerHuman);
					var str = BuildFHInfo(formerHuman) + nameof(Building_Door.PawnCanOpen) + "=" + canPass;
					builder.AppendLine(str.Indented("|\t"));
				}
			}

			Log.Message(builder.ToString());

		}

		static IEnumerable<ThinkNode> GetChildren([NotNull] ThinkNode node)
		{
			if (node is ThinkNode_SubtreesByTag sNode)
			{
				var allDefs = DefDatabase<ThinkTreeDef>.AllDefs.Where(d => d.insertTag == sNode.insertTag && d.thinkRoot != null);
				return allDefs.Select(d => d.thinkRoot);

			}
			if (node is ThinkNode_Subtree) return Enumerable.Empty<ThinkNode>();
			return node.subNodes.MakeSafe();
		}

		static string GetNodeLabel([NotNull] ThinkNode node)
		{
			var tLabel = node.GetType().Name;
			if (node is ThinkNode_SubtreesByTag streeTag)
			{
				return tLabel + "-" + streeTag.insertTag;
			}

			if (node is ThinkNode_Subtree sTree)
			{
				var fInfo = typeof(ThinkNode_Subtree).GetField("treeDef", BindingFlags.Instance | BindingFlags.NonPublic);
				var def = (Def)fInfo.GetValue(sTree);

				return tLabel + "-" + def.defName;
			}

			return tLabel;
		}

	}
}