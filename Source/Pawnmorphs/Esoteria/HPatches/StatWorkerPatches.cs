// StatWorkerPatches.cs modified by Iron Wolf for Pawnmorph on 09/29/2019 8:45 AM
// last updated 09/29/2019  8:45 AM

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Pawnmorph.Utilities;
using Prepatcher;
using RimWorld;
using Verse;

#pragma warning disable 1591
namespace Pawnmorph.HPatches
{
	public static class StatWorkerPatches
	{
		private static Dictionary<ulong, TimedCache<float>> _cache = new Dictionary<ulong, TimedCache<float>>(200);

		static StatWorkerPatches()
		{
			PawnmorphGameComp.OnClear += OnClear;
		}

		private static void OnClear(PawnmorphGameComp obj)
		{
			_cache.Clear();
		}

		/// <summary>
		/// If using prepatcher, this method gets a value stored on the Pawn to indicate if this method should be skipped entirely. Always returns false otherwise.
		/// </summary>
		/// <param name="target">The pawn in question.</param>
		/// https://github.com/Zetrith/Prepatcher/wiki/Adding-fields
		[PrepatcherField]
		private static ref bool PmShouldSkipStatWorker(this Pawn target)
		{
			_placeholder = false;
			return ref _placeholder;
		}

		private static bool _placeholder = false;


		[HarmonyPatch(typeof(StatWorker))]
		[HarmonyPatch(nameof(StatWorker.GetValueUnfinalized))]
		[HarmonyPatch(new Type[]
		{
			typeof(StatRequest), typeof(bool)
		})]
		internal static class GetValueUnfinalizedPatch
		{
			static float Postfix(float __result, StatRequest req, StatDef ___stat)
			{
				if (req.Thing is Pawn pawn)
				{
					if (PmShouldSkipStatWorker(pawn))
						return __result;

					ulong lookupID = (ulong)pawn.thingIDNumber << 32 | ___stat.shortHash;

					if (_cache.TryGetValue(lookupID, out TimedCache<float> cachedStat) == false)
					{
						cachedStat = _cache[lookupID] = new TimedCache<float>(() => CalculateOffsets(pawn, ___stat));
						cachedStat.Update();
						cachedStat.Offset(pawn.HashOffset());
					}

					__result += cachedStat.GetValue(1000);
				}
				return __result;
			}

			public static void Invalidate(Pawn pawn)
			{
				PmShouldSkipStatWorker(pawn) = false;
				ulong pawnId = (ulong)pawn.thingIDNumber << 32;
				foreach (var item in _cache)
				{
					if ((item.Key & pawnId) == pawnId)
					{
						item.Value.QueueUpdate();
					}
				}
			}

			private static float CalculateOffsets(Pawn pawn, StatDef stat)
			{
				float offset = 0;
				bool shouldSkip = true;

				var allAspects = pawn.GetAspectTracker()?.Aspects;
				if (allAspects != null)
				{
					foreach (var aspect in allAspects)
					{
						foreach (StatModifier statOffset in aspect.StatOffsets)
						{
							shouldSkip = false;
							if (statOffset.stat != stat)
								continue;

							offset += statOffset.value;
						}
					}
				}

				if (pawn.health?.hediffSet != null)
				{
					foreach (var hediffComps in pawn.health.hediffSet.hediffs.OfType<HediffWithComps>())
					{
						HediffComp_Production productionComp = hediffComps.TryGetComp<HediffComp_Production>();
						if (productionComp?.CurStage?.statOffsets == null)
							continue;

						foreach (StatModifier statOffset in productionComp.CurStage.statOffsets)
						{
							shouldSkip = false;
							if (statOffset.stat != stat)
								continue;

							offset += statOffset.value;
						}
					}
				}

				PmShouldSkipStatWorker(pawn) = shouldSkip;
				return offset;
			}
		}
	}
}