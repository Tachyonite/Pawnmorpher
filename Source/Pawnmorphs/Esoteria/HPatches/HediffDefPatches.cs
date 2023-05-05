// HediffDefPatches.cs created by Iron Wolf for Pawnmorph on 08/24/2021 7:44 PM
// last updated 08/24/2021  7:44 PM

using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.HPatches
{

	static class HediffDefPatches
	{
		[NotNull] private static readonly Dictionary<ushort, bool> _immunityCache = new Dictionary<ushort, bool>(100);

		[HarmonyPatch(typeof(HediffDef), nameof(HediffDef.PossibleToDevelopImmunityNaturally))]
		private static class HediffImmunityPatch
		{
			private static bool Prefix(HediffDef __instance, ref bool __result)
			{
				if (_immunityCache.TryGetValue(__instance.shortHash, out __result))
					return false;

				return true;
			}

			private static void Postfix(HediffDef __instance, ref bool __result, bool __runOriginal)
			{
				if (__runOriginal)
					_immunityCache[__instance.shortHash] = __result;
			}
		}

		[HarmonyPatch(typeof(HediffDef), nameof(HediffDef.ConfigErrors))]
		static class AdditionalErrorChecks
		{
			static IEnumerable<string> Postfix(IEnumerable<string> errors, HediffDef __instance)
			{
				foreach (string s in errors.MakeSafe())
				{
					yield return s;
				}

				if (__instance.stages != null)
				{
					StringBuilder builder = new StringBuilder();
					for (var index = 0; index < __instance.stages.Count; index++)
					{
						IInitializableStage instanceStage = __instance.stages[index] as IInitializableStage;
						if (instanceStage == null) continue;
						builder.Clear();
						foreach (string configError in instanceStage.ConfigErrors(__instance))
						{
							builder.AppendLine(configError);
						}

						if (builder.Length > 0)
						{
							yield return $"in stage {index}:\n" + builder;
						}
					}
				}
			}
		}
		[HarmonyPatch(typeof(HediffDef), nameof(HediffDef.ResolveReferences))]
		static class AdditionalResolveReferencesChecks
		{
			static void Postfix(HediffDef __instance)
			{
				if (__instance?.stages != null)
				{
					foreach (IInitializableStage iStage in __instance.stages.OfType<IInitializableStage>())
					{
						iStage.ResolveReferences(__instance);
					}
				}
			}
		}
	}
}