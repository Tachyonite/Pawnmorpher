// DebugLogUtils.MutationLogging.cs created by Iron Wolf for Pawnmorph on //2020 
// last updated 03/01/2020  6:57 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using JetBrains.Annotations;
using LudeonTK;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.DebugUtils
{
#pragma warning disable 1591

	public static partial class DebugLogUtils
	{
		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		private static void LogAllMutationInfo()
		{
			var builder = new StringBuilder();
			foreach (MutationDef allMutation in MutationDef.AllMutations)
			{
				builder.AppendLine(allMutation.ToStringFull());
			}

			Log.Message(builder.ToString());
		}

		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		static void GetRaceMaxInfluence()
		{
			StringBuilder builder = new StringBuilder();
			foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(t => t.race?.body != null))
			{
				builder.AppendLine($"{thingDef.defName}/{thingDef.label}={MorphUtilities.GetMaxInfluenceOfRace(thingDef)}");
			}

			Log.Message(builder.ToString());
		}

		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		static void CheckGenomes()
		{
			StringBuilder builder = new StringBuilder();
			foreach (MutationCategoryDef mCat in DefDatabase<MutationCategoryDef>.AllDefs)
			{
				if (mCat.GenomeDef != null)
				{
					builder.AppendLine($"{mCat.defName}:{mCat.GenomeDef.defName}");
				}
			}

			if (builder.Length == 0)
			{
				Log.Message("no genome defs");
			}
			else
				Log.Message(builder.ToString());
		}

		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		static void GetStatsPerMutation()
		{
			var allStages = MutationDef.AllMutations.SelectMany(m => m.stages.MakeSafe());
			List<StatDef> offsets = new List<StatDef>();
			List<StatDef> factors = new List<StatDef>();

			foreach (HediffStage hediffStage in allStages)
			{
				if (hediffStage == null) continue;
				foreach (StatModifier statModifier in hediffStage.statOffsets.MakeSafe())
				{
					if (!offsets.Contains(statModifier.stat)) offsets.Add(statModifier.stat);

				}

				foreach (StatModifier statModifier in hediffStage.statFactors.MakeSafe())
				{
					if (!factors.Contains(statModifier.stat)) factors.Add(statModifier.stat);
				}
			}

			StringBuilder builder = new StringBuilder();

			builder.AppendLine("Mutations," + offsets.Join());

			foreach (MutationDef allMutation in MutationDef.AllMutations)
			{
				if (allMutation.stages == null) continue;
				for (int i = 0; i < allMutation.stages.Count; i++)
				{
					var stage = allMutation.stages[i];
					if (stage?.statOffsets == null) continue;

					string eStr = allMutation.defName + i;

					List<string> entry = new List<string>() { eStr };

					foreach (StatDef statDef in offsets)
					{
						var stat = stage.statOffsets.FirstOrDefault(f => f.stat == statDef);

						var val = stat?.value ?? 0;

						entry.Add(val.ToString());
					}

					builder.AppendLine(entry.Join());
				}
			}

			Log.Message(builder.ToString());

			builder.Clear();
			builder.AppendLine("Mutations," + factors.Join());

			foreach (MutationDef allMutation in MutationDef.AllMutations)
			{
				if (allMutation.stages == null) continue;
				for (int i = 0; i < allMutation.stages.Count; i++)
				{
					var stage = allMutation.stages[i];
					if (stage?.statFactors == null) continue;

					string eStr = allMutation.defName + i;

					List<string> entry = new List<string>() { eStr };

					foreach (StatDef statDef in factors)
					{
						var stat = stage.statFactors.FirstOrDefault(f => f.stat == statDef);

						var val = stat?.value ?? 0;

						entry.Add(val.ToString());
					}

					builder.AppendLine(entry.Join());
				}
			}

			Log.Message(builder.ToString());

			builder.Clear();

			builder.AppendLine("Mutation,part efficiency, pain offset, pain factor");
			foreach (MutationDef allMutation in MutationDef.AllMutations)
			{
				if (allMutation.stages == null) continue;
				List<string> lst = new List<string>();

				for (var index = 0; index < allMutation.stages.Count; index++)
				{

					HediffStage allMutationStage = allMutation.stages[index];
					if (allMutationStage == null) continue;
					lst.Add(allMutation.defName + index);
					lst.Add(allMutationStage.partEfficiencyOffset.ToString());
					lst.Add(allMutationStage.painOffset.ToString());
					lst.Add(allMutationStage.painFactor.ToString());
					builder.AppendLine(lst.Join());
					lst.Clear();

				}

			}

			Log.Message(builder.ToString());

			List<PawnCapacityDef> caps = MutationDef.AllMutations.SelectMany(m => m.stages.MakeSafe())
													.SelectMany(st => st.capMods.MakeSafe().Select(s => s.capacity))
													.Where(c => c != null)
													.Distinct()
													.ToList();

			builder.Clear();
			List<string> tmpLst = new List<string>();
			tmpLst.Add("Mutation");
			tmpLst.AddRange(caps.Select(c => c.defName));

			builder.AppendLine(tmpLst.Join());
			tmpLst.Clear();


			foreach (MutationDef mut in MutationDef.AllMutations)
			{
				if (mut.stages == null) continue;
				for (int index = 0; index < mut.stages.Count; index++)
				{
					var stage = mut.stages[index];
					if (stage?.capMods == null) continue;
					tmpLst.Clear();
					tmpLst.Add(mut.defName + index);
					foreach (PawnCapacityDef pawnCapacityDef in caps)
					{
						var capMod = stage.capMods.FirstOrDefault(c => c.capacity == pawnCapacityDef);
						var val = capMod?.offset ?? 0;
						tmpLst.Add(val.ToString());
					}

					builder.AppendLine(tmpLst.Join());
				}
			}

			Log.Message(builder.ToString());

		}

		//debug output for visualizing all mutation classification info
		//writes to a 

		[DebugOutput(MAIN_CATEGORY_NAME)]
		static void PrintGpzPartClassificationInfo()
		{
			var root = AnimalClassDefOf.Animal;

			var mClassRoot = new MClassificationInfo(root);
			foreach (MutationDef mut in MutationDef.AllMutations)
			{
				if (mut.ClassInfluences.Contains(root)) mClassRoot.mutations.Add(mut);

			}

			int i = 0;
			foreach (AnimalClassBase child in root.Children)
			{
				mClassRoot.children.Add(BuildClassInfo(child, mClassRoot, i));
				i++;
			}

			StringBuilder builder = new StringBuilder();
			builder.AppendLine("graph G {");
			builder.AppendLine(mClassRoot.ConvertToStringRecursive());
			builder.AppendLine("}");
			Log.Message(builder.ToString());

		}

		static MClassificationInfo BuildClassInfo([NotNull] AnimalClassBase cBase, [NotNull] MClassificationInfo parent, int counter)
		{
			MClassificationInfo retVal = new MClassificationInfo(cBase)
			{
				hCounter = counter,
				vCounter = parent.vCounter + 1
			};

			foreach (MutationDef allMutation in MutationDef.AllMutations)
			{
				if (allMutation.ClassInfluences.Contains(cBase))
				{
					retVal.mutations.Add(allMutation);
				}
			}

			int i = 0;
			foreach (AnimalClassBase child in cBase.Children)
			{

				retVal.children.Add(BuildClassInfo(child, retVal, i));
				i++;
			}

			return retVal;

		}



		class MClassificationInfo
		{
			private static int cntr = 0;
			public MClassificationInfo([NotNull] AnimalClassBase mClass)
			{
				this.mClass = mClass;
				internalCount = cntr;
				cntr++;
			}
			public int hCounter;
			public int vCounter;
			private readonly int internalCount;

			[NotNull]
			public readonly AnimalClassBase mClass;
			[NotNull]
			public readonly List<MutationDef> mutations = new List<MutationDef>();
			[NotNull]
			public readonly List<MClassificationInfo> children = new List<MClassificationInfo>();
			private const string HEADER = "subgraph cluster";
			public string ConvertToStringRecursive()
			{
				StringBuilder builder = new StringBuilder();
				var header = HEADER + hCounter + "_" + vCounter + "_" + internalCount;
				builder.AppendLine(header);
				builder.AppendLine("{");
				var label = "label = \"" + mClass.defName + "\"";
				builder.AppendLine(label);
				builder.AppendLine(String.Join(",", mutations.Select(m => m.defName)));

				foreach (MClassificationInfo child in children.MakeSafe())
				{
					builder.AppendLine(child.ConvertToStringRecursive());
				}

				builder.AppendLine("}");
				return builder.ToString();

			}
		}



		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		static void CheckForSeverityPerDay()
		{
			StringBuilder builder = new StringBuilder();



			foreach (MutationDef mutation in MutationDef.AllMutations)
			{
				if (!mutation.HasComp(typeof(Comp_MutationSeverityAdjust)))
				{
					builder.AppendLine($"{mutation.defName} does not have a {nameof(Comp_MutationSeverityAdjust)} comp!");
				}
			}

			if (builder.Length == 0) Log.Message("All mutations have a severity per day comp");
			else Log.Message(builder.ToString());
		}


		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		static void GetMissingSlotsPerMorph()
		{
			var allPossibleSites = DefDatabase<MutationDef>
								  .AllDefs.SelectMany(m => m.GetAllMutationSites(BodyDefOf.Human))
								  .Distinct()
								  .ToList();

			List<MutationSite> scratchList = new List<MutationSite>();
			List<MutationSite> missingScratch = new List<MutationSite>();
			StringBuilder builder = new StringBuilder();

			foreach (MorphDef morphDef in MorphDef.AllDefs)
			{
				var allMorphSites = morphDef.AllAssociatedMutations.SelectMany(m => m.GetAllMutationSites(BodyDefOf.Human))
											.Distinct();

				scratchList.Clear();
				scratchList.AddRange(allMorphSites);
				missingScratch.Clear();
				var missing = allPossibleSites.Where(m => !scratchList.Contains(m));
				missingScratch.AddRange(missing);

				if (missingScratch.Count == 0)
				{
					builder.AppendLine($"{morphDef.defName} has all possible mutations");
				}
				else
				{
					float maxInfluence = ((float)scratchList.Count) / MorphUtilities.GetMaxInfluenceOfRace(ThingDefOf.Human);
					builder.AppendLine($"{morphDef.defName}: {nameof(maxInfluence)}={maxInfluence.ToStringByStyle(ToStringStyle.PercentOne)}");
					var grouping = missingScratch.GroupBy(g => g.Layer, g => g.Record);
					foreach (var group in grouping)
					{
						builder.AppendLine($"-\t{group.Key}: [{group.Select(s => s.def).Distinct().Join(s => s?.defName ?? "NULL")}]");
					}
				}
			}
			Log.Message(builder.ToString());

		}

		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		static void LogMutationsPerSlot()
		{
			List<MutationDef> allMutations = DefDatabase<MutationDef>.AllDefs.ToList();
			List<BodyPartDef> allValidParts = allMutations.SelectMany(m => m.parts).Distinct().ToList();
			StringBuilder builder = new StringBuilder();

			builder.AppendLine("Mutations by body part:");
			foreach (BodyPartDef part in allValidParts)
			{
				builder.AppendLine($"--{part}--");
				foreach (MutationLayer layer in Enum.GetValues(typeof(MutationLayer)))
				{
					List<MutationDef> mutationsOnLayer = allMutations.Where(m => m.parts.Contains(part) && m.RemoveComp.layer == layer).ToList();
					if (mutationsOnLayer.Count > 0) builder.AppendLine($"{layer} ({mutationsOnLayer.Count}): {string.Join(", ", mutationsOnLayer)}");
				}
			}
			Log.Message(builder.ToString());
		}

		[DebugOutput(category = MAIN_CATEGORY_NAME, onlyWhenPlaying = true)]
		static void LogMutationSensitivity()
		{
			StringBuilder builder = new StringBuilder();

			foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonistsAndPrisoners)
			{
				var mStat = pawn.GetStatValue(PMStatDefOf.MutagenSensitivity);
				var mFStat = pawn.GetMutagenicBuildupMultiplier();

				builder.AppendLine($"{pawn.Name} = {{{PMStatDefOf.MutagenSensitivity}:{mStat}, MutagenicBuildupMultiplier:{mFStat} }}");

			}

			Log.Message(builder.ToString());

		}

		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		static void ListMutationsPerCategory()
		{
			StringBuilder builder = new StringBuilder();
			foreach (MutationCategoryDef def in DefDatabase<MutationCategoryDef>.AllDefsListForReading)
			{
				builder.AppendLine($"{def.defName}:[{string.Join(",", def.AllMutations.Select(m => m.defName))}]");
			}

			Log.Message(builder.ToString());
		}

		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		private static void LogMutationValue()
		{
			var builder = new StringBuilder();
			foreach (MutationDef mutation in DefDatabase<MutationDef>.AllDefs)
				builder.AppendLine($"{mutation.defName},{mutation.value}");

			Log.Message(builder.ToString());
		}

	}
}