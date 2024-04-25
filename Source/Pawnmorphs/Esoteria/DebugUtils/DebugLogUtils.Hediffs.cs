using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
using Pawnmorph.Utilities;
using Verse;

#pragma warning disable 1591
namespace Pawnmorph.DebugUtils
{
	/// <summary>
	/// Debug log utils.
	/// </summary>
	public static partial class DebugLogUtils
	{
		private const string HEDIFF_CATEGORY_NAME = MAIN_CATEGORY_NAME + "-Hediffs";

		/// <summary>
		/// Increments the specific key in the dictionary, setting it to 1 if it wasn't present
		/// </summary>
		/// <param name="dict">Dictionary.</param>
		/// <param name="key">Key.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		private static void Increment<T>(this Dictionary<T, int> dict, T key)
		{
			int val = dict.TryGetValue(key, 0);
			dict[key] = val + 1;
		}

		/// <summary>
		/// Displays the count of all hediffs across all pawns in the world.
		/// </summary>
		[DebugOutput(category = HEDIFF_CATEGORY_NAME, onlyWhenPlaying = true)]
		public static void DisplayHediffCounts()
		{
			Dictionary<HediffDef, int> mapCount = new Dictionary<HediffDef, int>();
			Dictionary<HediffDef, int> totalCount = new Dictionary<HediffDef, int>();
			HashSet<HediffDef> hediffs = new HashSet<HediffDef>();

			foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned)
			{
				foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
				{
					hediffs.Add(hediff.def);
					mapCount.Increment(hediff.def);
					totalCount.Increment(hediff.def);
				}
			}

			foreach (Pawn pawn in Find.WorldPawns.AllPawnsAlive)
			{
				foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
				{
					hediffs.Add(hediff.def);
					totalCount.Increment(hediff.def);
				}

			}

			List<TableDataGetter<HediffDef>> tableGetters = new List<TableDataGetter<HediffDef>>
			{
				new TableDataGetter<HediffDef> ("defName", i => i.defName),
				new TableDataGetter<HediffDef> ("spawned #", i => mapCount.TryGetValue(i, 0)),
				new TableDataGetter<HediffDef> ("total #", i => totalCount.TryGetValue(i, 0)),
			};
			DebugTables.MakeTablesDialog(hediffs, tableGetters.ToArray());
		}

		[DebugAction(HEDIFF_CATEGORY_NAME, actionType = DebugActionType.ToolMapForPawns)]
		static void DisplayMutationCauses(Pawn p)
		{
			var h = p?.health?.hediffSet;
			if (h == null) return;
			StringBuilder builder = new StringBuilder();
			foreach (Hediff_AddedMutation hediff in h.hediffs.MakeSafe().OfType<Hediff_AddedMutation>())
			{
				builder.AppendLine($"{hediff.Label}:{hediff.Causes}");
			}
			Log.Message(builder.ToString());
		}

		/// <summary>
		/// Displays the count of all hediff classes across all pawns in the world.
		/// </summary>
		[DebugOutput(category = HEDIFF_CATEGORY_NAME, onlyWhenPlaying = true)]
		public static void DisplayHediffClassCounts()
		{
			Dictionary<string, int> mapCount = new Dictionary<string, int>();
			Dictionary<string, int> totalCount = new Dictionary<string, int>();
			HashSet<string> classes = new HashSet<string>();

			foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned)
			{
				foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
				{
					string classname = hediff.def.hediffClass.ToString();
					classes.Add(classname);
					mapCount.Increment(classname);
					totalCount.Increment(classname);
				}
			}

			foreach (Pawn pawn in Find.WorldPawns.AllPawnsAlive)
			{
				foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
				{
					string classname = hediff.def.hediffClass.ToString();
					classes.Add(classname);
					totalCount.Increment(classname);
				}

			}

			List<TableDataGetter<string>> tableGetters = new List<TableDataGetter<string>>
			{
				new TableDataGetter<string> ("className", i => i),
				new TableDataGetter<string> ("spawned #", i=> mapCount.TryGetValue(i, 0)),
				new TableDataGetter<string> ("total #", i => totalCount.TryGetValue(i, 0)),
			};
			DebugTables.MakeTablesDialog(classes, tableGetters.ToArray());
		}

		/// <summary>
		/// Displays all the hediff givers in use by all the hediffs
		/// </summary>
		[DebugOutput(category = HEDIFF_CATEGORY_NAME, onlyWhenPlaying = true)]
		public static void DisplayHediffGivers()
		{
			Dictionary<HediffDef, Dictionary<string, int>> dictdict = new Dictionary<HediffDef, Dictionary<string, int>>();
			HashSet<string> givers = new HashSet<string>();

			foreach (HediffDef def in DefDatabase<HediffDef>.AllDefs)
			{
				Dictionary<string, int> giverCounts = new Dictionary<string, int>();
				dictdict.Add(def, giverCounts);
				foreach (var giver in def.hediffGivers.MakeSafe())
				{
					var s = giver.GetType().ToString();
					givers.Add(s);
					giverCounts.Increment(s);
				}

				foreach (var stage in def.stages.MakeSafe())
				{
					foreach (var giver in stage.hediffGivers.MakeSafe())
					{
						var s = giver.GetType().ToString();
						givers.Add(s);
						giverCounts.Increment(s);
					}
				}
			}

			List<TableDataGetter<HediffDef>> tableGetters = new List<TableDataGetter<HediffDef>>
			{
				new TableDataGetter<HediffDef> ("defName", i => i.defName),
			};

			foreach (string giver in givers)
				tableGetters.Add(new TableDataGetter<HediffDef>(giver, i => dictdict.TryGetValue(i)?.TryGetValue(giver, 0)));

			DebugTables.MakeTablesDialog(DefDatabase<HediffDef>.AllDefs, tableGetters.ToArray());
		}
	}
}