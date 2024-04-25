// DebugLogUtils.cs created by Iron Wolf for Pawnmorph on 09/23/2019 7:54 AM
// last updated 09/27/2019  8:00 AM

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using JetBrains.Annotations;
using LudeonTK;
using Pawnmorph.Chambers;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

#pragma warning disable 1591
namespace Pawnmorph.DebugUtils
{
	[StaticConstructorOnStartup]
	public static partial class DebugLogUtils
	{
		[NotNull]
		private static MutationOutlook[] Outlooks { get; }

		static DebugLogUtils()
		{
			Outlooks = Enum.GetValues(typeof(MutationOutlook)).OfType<MutationOutlook>().ToArray();
		}


		public const string MAIN_CATEGORY_NAME = "Pawnmorpher";


		private const string STORAGE_SPACE_HEADER = "DefName,Storage Space Required, Value";


		[DebuggerHidden]
		public static void LogFail(this LogFailMode mode, string message)
		{
			switch (mode)
			{
				case LogFailMode.Silent:
					break;
				case LogFailMode.Log:
					Log.Message(message);
					break;
				case LogFailMode.Warning:
					Log.Warning(message);
					break;
				case LogFailMode.Error:
					Log.Error(message);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}
		}

		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		static void GetAllDistinctPMTraderTags()
		{
			var tTags = DefDatabase<ThingDef>.AllDefs.SelectMany(td => td.tradeTags.MakeSafe()).Distinct();
			Log.Message(string.Join(",", tTags));


		}

		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		public static void LogStorageSpaceRequirementRange()
		{
			StringBuilder builder = new StringBuilder();

			builder.AppendLine(STORAGE_SPACE_HEADER);

			foreach (MutationDef mutationDef in DefDatabase<MutationDef>.AllDefs.Where(d => !d.IsRestricted))
			{
				builder.AppendLine(mutationDef.defName + "," + mutationDef.GetRequiredStorage() + "," + mutationDef.value);
			}

			Log.Message(builder.ToString());
			builder.Clear();
			builder.AppendLine(STORAGE_SPACE_HEADER);

			foreach (PawnKindDef pawnKindDef in DefDatabase<PawnKindDef>.AllDefs.Where(d => d?.race?.IsValidAnimal() == true))
			{
				builder.AppendLine(pawnKindDef.defName
								 + ","
								 + pawnKindDef.GetRequiredStorage()
								 + ","
								 + pawnKindDef.race.BaseMarketValue);
			}

			Log.Message(builder.ToString());

		}

		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		static void LogSapienceWillBonuses()
		{
			StringBuilder builder = new StringBuilder();
			for (float f = 0; f <= 1; f += 0.05f)
			{
				builder.AppendLine($"{f}[{FormerHumanUtilities.GetQuantizedSapienceLevel(f)}]:{FormerHumanUtilities.GetSapienceWillDebuff(f)}");
			}

			Log.Message(builder.ToString());
		}

		[DebugOutput(category = MAIN_CATEGORY_NAME, onlyWhenPlaying = true)]
		public static void LogDatabaseInfo()
		{
			var db = Find.World.GetComponent<ChamberDatabase>();
			Log.Message($"total storage:{db.TotalStorage}, used storage:{db.UsedStorage}, free:{db.FreeStorage}");

		}

		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		public static void FindMissingMorphDescriptions()
		{
			List<MorphDef> morphs = DefDatabase<MorphDef>
								   .AllDefs.Where(def => string.IsNullOrEmpty(def.description)
													  || def.description.StartsWith("!!!"))
								   .ToList();
			if (morphs.Count == 0)
			{
				Log.Message("all morphs have descriptions c:");
			}
			else
			{
				string str = string.Join("\n", morphs.Select(def => def.defName).ToArray());
				Log.Message($"Morphs Missing descriptions:\n{str}");
			}
		}

		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		static void ListMorphInfo()
		{

			StringBuilder builder = new StringBuilder();
			var morphsByMod = MorphDef.AllDefs.GroupBy(m => m.modContentPack);
			List<ThingDef> tmpLst = new List<ThingDef>();
			foreach (IGrouping<ModContentPack, MorphDef> morphByMod in morphsByMod)
			{
				const string mainSep = "----------";
				const string lineSep = "\t-";
				builder.AppendLine($"{mainSep}{morphByMod.Key.Name}{mainSep}");

				foreach (MorphDef morph in morphByMod)
				{
					builder.AppendLine($"{morph}:{morph.race.label}");
					tmpLst.Clear();
					tmpLst.AddRange(morph.AllAssociatedAnimals.Where(r => r != morph.race));
					if (tmpLst.Count > 0)
						builder.AppendLine(lineSep + string.Join(",", tmpLst.Select(a => a.label)));

				}
			}

			Log.Message(builder.ToString());
		}


		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		public static void FindMissingMutationDescriptions()
		{
			bool SelectionFunc(HediffDef def)
			{
				if (!typeof(Hediff_AddedMutation).IsAssignableFrom(def.hediffClass))
					return false; //must be mutation hediff 
				return string.IsNullOrEmpty(def.description) || def.description.StartsWith("!!!");
			}

			IEnumerable<HediffDef> mutations = DefDatabase<HediffDef>.AllDefs.Where(SelectionFunc);

			string str = string.Join("\n\t", mutations.Select(m => m.defName).ToArray());

			Log.Message(string.IsNullOrEmpty(str) ? "no parts with missing description" : str);
		}




		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		public static void GetSpreadingMutationStats()
		{
			FieldInfo isSkinCoveredF =
				typeof(BodyPartDef)
				   .GetField("skinCovered", //have to get isSkinCovered field by reflection because it's not public 
							 BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

			List<BodyPartRecord> spreadableParts = BodyDefOf
												  .Human.GetAllMutableParts()
												  .Where(r => (bool)(isSkinCoveredF?.GetValue(r.def) ?? false))
												  .ToList();

			int spreadablePartsCount = spreadableParts.Count;


			var allSpreadableMutations = MutationDef.AllMutations.Select(mut => new
			{
				mut, //make an anonymous object to keep track of both the mutation and comp
				comp = mut.CompProps<SpreadingMutationCompProperties>()
			})
													.Where(o => o.comp != null) //keep only those with a spreading property 
													.ToList();

			var builder = new StringBuilder();

			builder.AppendLine($"{spreadablePartsCount} parts that spreadable parts can spread onto");
			foreach (var sMutation in allSpreadableMutations)
			{
				if (sMutation.mut.stages == null) continue;

				builder.AppendLine($"----{sMutation.mut.defName}-----");

				//print some stuff about the comp 
				builder.AppendLine($"mtb:{sMutation.comp.mtb}");
				builder.AppendLine($"searchDepth:{sMutation.comp.maxTreeSearchDepth}");


				for (var i = 0; i < sMutation.mut.stages.Count; i++)
				{
					HediffStage stage = sMutation.mut.stages[i];
					builder.AppendLine($"\tstage:{stage.label},{i}");


					foreach (PawnCapacityModifier capMod in stage.capMods ?? Enumerable.Empty<PawnCapacityModifier>())
					{
						float postFactor =
							Mathf.Pow(capMod.postFactor,
									  spreadablePartsCount); //use pow because the post factors are multiplied together, not added 
						builder.AppendLine($"\t\t{capMod.capacity.defName}:[offset={capMod.offset * spreadablePartsCount}, postFactor={capMod.postFactor = postFactor}]");
					}

					foreach (StatModifier statOffset in stage.statOffsets ?? Enumerable.Empty<StatModifier>())
						builder.AppendLine($"\t\t{statOffset.stat.defName}:{statOffset.value * spreadablePartsCount}");
				}

				builder.AppendLine(""); //make an empty line between mutations 
			}


			Log.Message(builder.ToString());
		}


		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		public static void ListHybridStateOffset()
		{
			IEnumerable<MorphDef> morphs = DefDatabase<MorphDef>.AllDefs;
			ThingDef human = ThingDefOf.Human;

			Dictionary<StatDef, float> lookup = human.statBases.ToDictionary(s => s.stat, s => s.value);

			var builder = new StringBuilder();

			foreach (MorphDef morphDef in morphs)
			{
				builder.AppendLine($"{morphDef.label}:");

				foreach (StatModifier statModifier in morphDef.hybridRaceDef.statBases ?? Enumerable.Empty<StatModifier>())
				{
					float humanVal = lookup.TryGetValue(statModifier.stat);
					float diff = statModifier.value - humanVal;
					string sym = diff > 0 ? "+" : "";
					string str = $"{statModifier.stat.label}:{sym}{diff}";
					builder.AppendLine($"\t\t{str}");
				}
			}

			Log.Message($"{builder}");
		}

		private static void BuildGraphvizTree([NotNull] AnimalClassBase aBase, StringBuilder builder)
		{
			foreach (AnimalClassBase child in aBase.Children)
			{
				builder.AppendLine($"{aBase.defName}->{child.defName};");
				BuildGraphvizTree(child, builder);
			}
		}

		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		private static void FindMissingMorphReactions()
		{
			var missingMorphs = new List<MorphDef>();


			foreach (MorphDef morphDef in MorphDef.AllDefs)
			{
				MorphDef.TransformSettings tfSettings = morphDef.transformSettings;
				if (IsTODOThought(tfSettings?.transformationMemory) || IsTODOThought(tfSettings?.revertedMemory))
					missingMorphs.Add(morphDef);
			}

			string msgText = missingMorphs.Select(m => m.defName).Join(delimiter: "\n");
			Log.Message(msgText);
		}



		[DebugOutput(category = MAIN_CATEGORY_NAME, onlyWhenPlaying = true)]
		private static void GetPawnsNewInfluence()
		{
			var wDict = new Dictionary<AnimalClassBase, float>();
			var mutations = new List<Hediff_AddedMutation>();
			var strings = new List<string>();

			foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonists)
			{
				mutations.Clear();
				mutations.AddRange(pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>());
				float maxInf = MorphUtilities.GetMaxInfluenceOfRace(pawn.def);
				AnimalClassUtilities.FillInfluenceDict(mutations, wDict);
				if (wDict.Count == 0) continue;
				//now build the log message entry 
				var builder = new StringBuilder();
				builder.AppendLine($"{pawn.Name}:");
				foreach (KeyValuePair<AnimalClassBase, float> kvp in wDict)
				{
					var def = (Def)kvp.Key;
					builder.AppendLine($"\t{def.label}:{(kvp.Value / maxInf).ToStringPercent()}");
				}

				strings.Add(builder.ToString());
			}

			string msg = strings.Join(delimiter: "\n----------------------------\n");
			Log.Message(msg);
		}



		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		private static void ListAllMutationsPerMorph()
		{
			var builder = new StringBuilder();

			foreach (MorphDef morphDef in MorphDef.AllDefs)
			{
				builder.AppendLine(morphDef.defName);

				builder.AppendLine(morphDef.AllAssociatedMutations.Select(m => m.defName).Join(delimiter: ","));
			}

			Log.Message(builder.ToString());
		}

		[DebugOutput(category = MAIN_CATEGORY_NAME, onlyWhenPlaying = true)]
		private static void ListAllSpreadableParts()
		{
			BodyDef bDef = BodyDefOf.Human;
			var sBuilder = new StringBuilder();

			foreach (BodyPartDef allMutablePart in bDef.GetAllMutableParts().Select(b => b.def).Distinct())
				if (allMutablePart.IsSkinCoveredInDefinition_Debug)
					sBuilder.AppendLine($"<li>{allMutablePart.defName}</li>");

			Log.Message(sBuilder.ToString());
		}

		[DebugOutput(MAIN_CATEGORY_NAME, true)]
		private static void ListMutationsInMorphs()
		{
			var builder = new StringBuilder();

			foreach (MorphDef morphDef in DefDatabase<MorphDef>.AllDefsListForReading)
			{
				string outStr = morphDef.AllAssociatedMutations.Select(m => m.defName).Join(delimiter: ",");
				builder.AppendLine($"{morphDef.defName}:{outStr}");
			}

			Log.Message(builder.ToString());
		}





		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		private static void MaximumMutationPointsForHumans()
		{
			Log.Message(MorphUtilities.GetMaxInfluenceOfRace(ThingDefOf.Human).ToString());
		}

		[DebugOutput(MAIN_CATEGORY_NAME)]
		[UsedImplicitly]
		private static void PrintGraphVizAnimalTree()
		{
			var builder = new StringBuilder();
			builder.AppendLine("digraph animalClassTree\n{\n\trankdir=LR");

			foreach (AnimalClassDef aClass in DefDatabase<AnimalClassDef>.AllDefs)
				builder.AppendLine($"\t{aClass.defName}[shape=square]");

			BuildGraphvizTree(AnimalClassDefOf.Animal, builder);
			builder.AppendLine("}");
			Log.Message(builder.ToString());
		}

		private struct HediffStageInfo
		{
			public string defName;
			public int stageIndex;
			public float minSeverity;
			public float severityPerDay;

			public static string Header => "defName,stageIndex,minSeverity,severityPerDay";

			/// <summary>Returns the fully qualified type name of this instance.</summary>
			/// <returns>A <see cref="T:System.String" /> containing a fully qualified type name.</returns>
			public override string ToString()
			{
				return $"{defName},{stageIndex},{minSeverity},{severityPerDay}";
			}
		}

		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		static void LogAllBackstoryInfo()
		{
			StringBuilder builder = new StringBuilder();
			foreach (var backstory in DefDatabase<BackstoryDef>.AllDefs)
			{
				var ext = backstory.GetModExtension<MorphPawnKindExtension>();
				if (ext == null) continue;
				builder.AppendLine($"---{backstory.defName}---");
				builder.AppendLine(ext.ToStringFull());

			}

			Log.Message(builder.ToString());
		}

		[DebugOutput(category = MAIN_CATEGORY_NAME, onlyWhenPlaying = true)]
		static void ListDesignatedFormerHumans()
		{
			var map = Find.CurrentMap;
			if (map == null) return;
			var designation =
				map.designationManager.AllDesignations.Where(d => d.def == PMDesignationDefOf.RecruitSapientFormerHuman).ToList();
			if (designation.Count == 0)
			{
				Log.Message($"No {nameof(PMDesignationDefOf.RecruitSapientFormerHuman)} on map");
				return;
			}

			var str = designation.Join(s => s.target.Label, "\n");
			Log.Message(str);



		}

		/// <summary>Lists all tags and their associated morphs to the console.</summary>
		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		public static void ListMorphsByTags()
		{
			StringBuilder builder = new StringBuilder();
			Dictionary<MorphCategoryDef, IEnumerable<MorphDef>> mutationDefsByInfluence =
				DefDatabase<MorphCategoryDef>.AllDefs
				.Select(k => new { k, v = DefDatabase<MorphDef>.AllDefs.Where(m => m.categories.Contains(k)) })
				.ToDictionary(x => x.k, x => x.v);
			foreach (KeyValuePair<MorphCategoryDef, IEnumerable<MorphDef>> entry in mutationDefsByInfluence)
			{
				builder.AppendLine($"{entry.Key.defName}:");
				foreach (MorphDef value in entry.Value)
				{
					builder.AppendLine($"    {value.LabelCap}");
				}
			}
			Log.Message(builder.ToString());
		}

		///<summary>Lists all MutationDefs in the console, sorted by influence.</summary>
		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		public static void ListMutationsByInfluence()
		{
			StringBuilder builder = new StringBuilder();
			// Build a dictionary of all defs, sorted by influence.
			Dictionary<AnimalClassBase, IEnumerable<MutationDef>> mutationDefsByInfluence =
				DefDatabase<AnimalClassBase>.AllDefs
				.Select(k => new { k, v = DefDatabase<MutationDef>.AllDefs.Where(m => m.ClassInfluences.Contains(k)) })
				.ToDictionary(x => x.k, x => x.v);
			foreach (KeyValuePair<AnimalClassBase, IEnumerable<MutationDef>> entry in mutationDefsByInfluence)
			{
				builder.AppendLine($"{entry.Key.defName}:");
				foreach (MutationDef value in entry.Value)
				{
					builder.AppendLine($"    {value.defName} - {value.description}");
				}
			}
			Log.Message(builder.ToString());
		}


		[DebugOutput(category = MAIN_CATEGORY_NAME, onlyWhenPlaying = true)]
		static void GetHistoryEventInfo()
		{
			StringBuilder builder = new StringBuilder();

			foreach (HistoryEventDef historyEventDef in HistoryEventUtilities.AllCustomEvents)
			{
				var count = Find.HistoryEventsManager.GetRecentCountWithinTicks(historyEventDef, int.MaxValue);
				builder.AppendLine($"{historyEventDef.defName}:{count}");
			}

			Log.Message(builder.ToString());
		}


		/// <summary>Prints out all MutationDef's labels and descriptions (Including stages).</summary>
		[DebugOutput(category = MAIN_CATEGORY_NAME)]
		public static void LogAllMutationLabelsAndDescriptions()
		{
			StringBuilder builder = new StringBuilder();
			foreach (MutationDef mutation in DefDatabase<MutationDef>.AllDefs.Where(m => !m.parts.NullOrEmpty()).OrderBy(n => n.parts.First().defName))
			{
				int i = 0;
				builder.AppendLine($"Mutation: {mutation.defName}");
				builder.AppendLine($"Label: {mutation.label}");
				builder.AppendLine($"Description: {mutation.description}");
				if (!mutation.stages.NullOrEmpty())
				{
					foreach (HediffStage stage in mutation.stages)
					{
						if (stage.label != null)
						{
							builder.AppendLine($"    Stage {i} Label: {stage.label}");
						}
						if (stage.GetType() == typeof(MutationStage))
						{
							MutationStage mutationStage = (MutationStage)stage;
							if (mutationStage.labelOverride != null)
							{
								builder.AppendLine($"    Stage {i} Override: {mutationStage.labelOverride}");
							}
							if (mutationStage.description != null)
							{
								builder.AppendLine($"    Stage {i} Description: {mutationStage.description}");
							}
						}
						else
						{
							builder.AppendLine($"    Stage {i} is not a MutationStage");
						}
						builder.AppendLine();
						i++;
					}
				}
				else
				{
					builder.AppendLine("Mutation has no stages");
					builder.AppendLine();
				}
			}
			Log.Message(builder.ToString());
		}
	}
}
