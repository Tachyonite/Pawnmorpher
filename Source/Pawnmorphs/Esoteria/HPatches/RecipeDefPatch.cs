using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Pawnmorph.Chambers;
using Verse;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(RecipeDef))]
	internal class RecipeDefPatch
	{
		static PawnmorpherSettings _settings;
		static Dictionary<int, MorphDef> _injectorMorphMapping;

		public static void Prepare(MethodBase original)
		{
			if (original != null)
				return;

			_settings = LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>();
			List<ThingDef> _injectors = PMThingCategoryDefOf.Injector.SortedChildThingDefs;
			
			List<MorphDef> allMorphs = MorphDef.AllDefs.ToList();

			_injectorMorphMapping = new Dictionary<int, MorphDef>(_injectors.Count);
			foreach (var injector in _injectors)
			{
				IngestionOutcomeDoer_GiveHediff hediff = injector.ingestible?.outcomeDoers.OfType<IngestionOutcomeDoer_GiveHediff>().FirstOrDefault();
				if (hediff != null && hediff.hediffDef != null)
				{
					MorphDef morphDef = allMorphs.FirstOrDefault(m => m.fullTransformation == hediff.hediffDef);
					if (morphDef != null)
						_injectorMorphMapping[injector.index] = morphDef;
				}
			}
		}

		[HarmonyPatch(nameof(RecipeDef.AvailableNow), MethodType.Getter), HarmonyPostfix]
		public static bool AvailableNow(bool __result, RecipeDef __instance)
		{
			// If recipe is available (Has been researched)
			if (__result && _settings.injectorsRequireTagging && __instance.ProducedThingDef != null)
			{
				if (_injectorMorphMapping.TryGetValue(__instance.ProducedThingDef.index, out var morphDef))
				{
					if (DebugSettings.godMode)
						return true;

					return morphDef.IsTagged();
				}
			}
			return __result;
		}
	}
}
