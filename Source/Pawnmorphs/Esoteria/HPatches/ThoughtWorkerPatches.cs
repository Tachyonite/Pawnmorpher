// ThoughtWorkerPatches.cs created by Iron Wolf for Pawnmorph on 07/24/2021 10:56 AM
// last updated 07/24/2021  10:56 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
	/// <summary>
	/// patches thought workers 
	/// </summary>
	public static class ThoughtWorkerPatches
	{
		/// <summary>
		/// patches thought worker.
		/// </summary>
		/// <param name="harInstance">The har instance.</param>
		public static void DoPatches([NotNull] Harmony harInstance)
		{
			try
			{
				if (!ModsConfig.IdeologyActive) return;

				BindingFlags stBindingFlags = BindingFlags.NonPublic | BindingFlags.Static;
				var normalPatchMethod = typeof(ThoughtWorkerPatches).GetMethod("DisableFormerHumanThoughtNormal", stBindingFlags);
				var socialPatchMethod = typeof(ThoughtWorkerPatches).GetMethod("DisableFormerHumanThoughtSocial", stBindingFlags);


				foreach (MethodInfo method in AllNormalMethods)
				{
					harInstance.Patch(method, postfix: new HarmonyMethod(normalPatchMethod));
				}

				foreach (MethodInfo allSocialMethod in AllSocialMethods)
				{
					harInstance.Patch(allSocialMethod, postfix: new HarmonyMethod(socialPatchMethod));
				}
			}
			catch (Exception e)
			{
				Log.Error($"unable to perform thought worker patching!\n{e}");
			}
		}

		[HarmonyPatch(typeof(ThoughtWorker_Hot), "CurrentStateInternal")]
		static class FixThoughtWorkerHot
		{
			static bool Prefix(Pawn p, ref ThoughtState __result)
			{
				if (p.IsAnimal() || (ModsConfig.IdeologyActive && p.Ideo == null))
				{
					__result = false;
					return false;
				}

				return true;
			}
		}


		[HarmonyPatch(typeof(ThoughtWorker_Cold), "CurrentStateInternal")]
		static class FixThoughtWorkerCold
		{
			static bool Prefix(Pawn p, ref ThoughtState __result)
			{
				if (p.IsAnimal() || (ModsConfig.IdeologyActive && p.Ideo == null))
				{
					__result = false;
					return false;
				}

				return true;
			}
		}

		[NotNull]
		static IEnumerable<Type> AllPreceptNudityThoughts
		{
			get
			{
				var maleNudity = DefDatabase<IssueDef>.GetNamed("Nudity_Male");
				var femaleNudity = DefDatabase<IssueDef>.GetNamed("Nudity_Female");

				var precepts = DefDatabase<PreceptDef>.AllDefs.Where(p => p.issue == maleNudity || p.issue == femaleNudity);
				var comps = precepts.SelectMany(p => p.comps.MakeSafe().OfType<PreceptComp_SituationalThought>());
				var types = comps.Select(t => t.thought.workerClass);
				return types.Distinct();
			}
		}

		[NotNull]

		static IEnumerable<MethodInfo> AllNormalMethods
		{
			get
			{
				return AllPreceptNudityThoughts.Where(tp => !tp.Name.Contains("Social"))
											   .Select(t => t.GetMethod("ShouldHaveThought",
																		BindingFlags.NonPublic | BindingFlags.Instance));
			}
		}

		[NotNull]
		static IEnumerable<MethodInfo> AllSocialMethods
		{
			get
			{
				return AllPreceptNudityThoughts.Where(tp => tp.Name.Contains("Social"))
											   .Select(t => t.GetMethod("ShouldHaveThought",
																		BindingFlags.NonPublic | BindingFlags.Instance));
			}
		}


		static void DisableFormerHumanThoughtNormal(Pawn p, ref ThoughtState __result)
		{
			if (__result.Active && p.IsFormerHuman())
			{
				__result = false;
			}


		}

		static void DisableFormerHumanThoughtSocial(Pawn p, Pawn otherPawn, ref ThoughtState __result)
		{
			if (__result.Active && otherPawn?.IsFormerHuman() == true)
			{
				__result = false;
			}
		}

	}
}