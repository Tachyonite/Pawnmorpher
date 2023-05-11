// PortraitCachePatches.cs created by Iron Wolf for Pawnmorph on 03/08/2020 11:05 AM
// last updated 03/08/2020  11:05 AM

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.HPatches
{
	static class PortraitCachePatches
	{

		[HarmonyPatch(typeof(PortraitsCache), nameof(PortraitsCache.Get))]
		static class GetPatch
		{



			static float GetZoomMultiplier([NotNull] Pawn pawn)
			{
				var sz = pawn.kindDef.lifeStages[pawn.ageTracker.CurLifeStageIndex].bodyGraphicData.drawSize.x;  //assume x & y are the same 

				var mult = 1 / sz;
				//pow brings the multiplier closer to 1, 'dampening' the effect of the sapient humans draw size
				return Mathf.Pow(mult, 1 / 3f);

			}

			[HarmonyPrefix]
			static bool Prefix([NotNull] Pawn pawn,
							   ref Vector2 size,
							   ref Vector3 cameraOffset,
							   ref float cameraZoom,
							   bool supersample,
							   bool compensateForUIScale)
			{
				if (pawn.IsFormerHuman())
				{
					float mult = GetZoomMultiplier(pawn);
					cameraZoom *= mult;
					cameraOffset.z /= mult;
				}

				return true;
			}
		}
	}
}