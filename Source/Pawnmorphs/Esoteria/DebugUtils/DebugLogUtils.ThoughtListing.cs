// DebugLogUtils.ThoughtListing.cs created by Iron Wolf for Pawnmorph on //2020 
// last updated 02/29/2020  1:42 PM

using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using JetBrains.Annotations;
using LudeonTK;
using Pawnmorph.Hediffs;
using Pawnmorph.Thoughts;
using RimWorld;
using Verse;

#pragma warning disable 1591
namespace Pawnmorph.DebugUtils
{
	public static partial class DebugLogUtils
	{
		private static bool IsTODOThought(ThoughtDef thoughtDef)
		{
			if (thoughtDef?.stages == null) return true;
			if (thoughtDef.stages.Any(s => string.IsNullOrEmpty(s?.label)
										|| s.label.StartsWith("TODO")
										|| s.label.StartsWith("!!!")))
				return true;
			if (thoughtDef.stages.Any(s => string.IsNullOrEmpty(s?.description)
										|| s.description.StartsWith("TODO")
										|| s.description.StartsWith("!!!")))
				return true;
			return false;
		}

		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		public static void FindAllTODOThoughts()
		{
			var builder = new StringBuilder();
			var defNames = new List<string>();
			foreach (ThoughtDef thoughtDef in DefDatabase<ThoughtDef>.AllDefs)
			{
				var addedHeader = false;
				for (var index = 0; index < (thoughtDef?.stages?.Count ?? 0); index++)
				{
					ThoughtStage stage = thoughtDef?.stages?[index];
					if (stage == null) continue;
					if (string.IsNullOrEmpty(stage.label) || string.IsNullOrEmpty(stage.description)) continue;
					if (stage.label == "TODO"
					 || stage.description == "TODO"
					 || stage.description.StartsWith("!!!")
					 || stage.label.StartsWith("!!!"))
					{
						if (!addedHeader)
						{
							builder.AppendLine($"In {thoughtDef.defName}:");
							addedHeader = true;
						}

						defNames.Add(thoughtDef.defName);
						builder.AppendLine($"{index}) label:{stage.label} description:\"{stage.description}\"".Indented());
					}
				}
			}

			builder.AppendLine(defNames.Distinct().Join(d => d, "\n"));

			Log.Message(builder.ToString());
		}

		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		public static void GetThoughtlessMutations()
		{
			var missingThought = DefDatabase<MutationDef>.AllDefs.Where(m => m.mutationMemory == null).Join(m => m.defName, "\n");

			Log.Message(missingThought);
		}

		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		static void ListMutationOutlookThoughts()
		{
			StringBuilder builder = new StringBuilder();
			foreach (ThoughtDef thoughtDef in DefDatabase<ThoughtDef>.AllDefs)
			{
				if (!typeof(MutationMemory).IsAssignableFrom(thoughtDef.thoughtClass)) continue;
				ListOutlookMemory(thoughtDef, builder);
			}

			Log.Message(builder.ToString());
		}


		static void ListOutlookMemory([NotNull] ThoughtDef thought, [NotNull] StringBuilder builder)
		{
			if (thought.stages == null) return;
			builder.AppendLine($"\n====={thought}=====");
			for (int i = 0; i < thought.stages.Count; i++)
			{
				string label;
				if (i >= Outlooks.Length)
					label = "out of bonds";
				else
					label = Outlooks[i].ToString();
				builder.AppendLine($"---stage {i}:{label}---");
				var tStage = thought.stages[i];
				ListThoughtStageInfo(tStage, builder);
			}


		}

		static void ListThoughtStageInfo([NotNull] ThoughtStage stage, [NotNull] StringBuilder builder)
		{
			builder.AppendLine($"{nameof(stage.label)}:{stage.label}\t{nameof(stage.labelSocial)}:{stage.labelSocial}");
			builder.AppendLine($"{nameof(stage.visible)}:{stage.visible}\t{nameof(stage.baseMoodEffect)}:{stage.baseMoodEffect}\t{nameof(stage.baseOpinionOffset)}:{stage.baseOpinionOffset}");
			builder.AppendLine($"description:\t{stage.description}");
		}

	}
}