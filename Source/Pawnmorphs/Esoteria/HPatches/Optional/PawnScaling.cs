﻿using System.Reflection;
using System.Runtime.CompilerServices;
using AlienRace;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using static AlienRace.AlienPartGenerator;

namespace Pawnmorph.HPatches.Optional
{
	[OptionalPatch("PMPawnScalingCaption", "PMPawnScalingDescription", nameof(_enabled), false)]
	[HarmonyLib.HarmonyPatch]
	static class PawnScaling
	{
		static bool _enabled = false;
		static Pawn _currentPawn;
		static float _currentScaledBodySize;



		static bool Prepare(MethodBase original)
		{
			if (original == null && _enabled)
			{
				StatsUtility.GetEvents(PMStatDefOf.PM_BodySize).StatChanged += PawnScaling_StatChanged;
			}

			return _enabled;
		}

		// Trigger pawn graphics update at the end of the tick if body size stat changes.
		private static void PawnScaling_StatChanged(Verse.Pawn pawn, RimWorld.StatDef stat, float oldValue, float newValue)
		{
			LongEventHandler.ExecuteWhenFinished(() =>
			{
				_currentPawn = null;
				ResolveAllGraphics(pawn);
			});
		}

		// Updates all draw sizes on comp to specified size.
		private static void SetCompScales(AlienComp comp, Pawn pawn, float bodysize)
		{
			float sizeOffset = bodysize;

			var partGenerator = (pawn.def as ThingDef_AlienRace).alienRace.generalSettings.alienPartGenerator;
			Vector2 sizeOffsetVector = new Vector2(sizeOffset, sizeOffset);

			comp.customDrawSize = partGenerator.customDrawSize * sizeOffsetVector;
			comp.customHeadDrawSize = partGenerator.customHeadDrawSize * sizeOffsetVector;
			comp.customPortraitDrawSize = partGenerator.customPortraitDrawSize * sizeOffsetVector;
			comp.customPortraitHeadDrawSize = partGenerator.customPortraitHeadDrawSize * sizeOffsetVector;
		}

		// Override HAR comp scales.
		[HarmonyLib.HarmonyPatch(typeof(AlienComp), nameof(AlienComp.PostSpawnSetup)), HarmonyLib.HarmonyPostfix]
		private static void PostSpawnSetup(bool respawningAfterLoad, AlienComp __instance)
		{
			SetCompScales(__instance, (Pawn)__instance.parent, GetScale((Pawn)__instance.parent));
		}


		[HarmonyLib.HarmonyAfter(new string[] { "erdelf.HumanoidAlienRaces" })]
		[HarmonyLib.HarmonyPatch(typeof(Verse.PawnGraphicSet), nameof(Verse.PawnGraphicSet.ResolveAllGraphics)), HarmonyLib.HarmonyPostfix]
		private static void ResolveAllGraphics(Pawn ___pawn)
		{
			float bodysize = GetScale(___pawn);
			if (bodysize == 1)
				return;

			AlienComp comp = CompCacher<AlienComp>.GetCompCached(___pawn);
			if (comp != null)
			{
				// Set draw sizes
				SetCompScales(comp, ___pawn, bodysize);
			};
		}

		// Apply scale to body addon offsets.
		[HarmonyLib.HarmonyPatch(typeof(AlienRace.HarmonyPatches), nameof(AlienRace.HarmonyPatches.DrawAddonsFinalHook)), HarmonyLib.HarmonyPostfix]
		private static void DrawAddonsFinalHook(Pawn pawn, AlienRace.AlienPartGenerator.BodyAddon addon, ref Graphic graphic, ref Vector3 offsetVector, ref float angle, ref Material mat)
		{
			float value = GetScale(pawn);
			offsetVector.x *= value;
			// Don't affect y layer
			offsetVector.z *= value;
		}


		[HarmonyLib.HarmonyAfter(new string[] { "erdelf.HumanoidAlienRaces" })]
		[HarmonyLib.HarmonyPatch(typeof(RimWorld.PawnCacheRenderer), nameof(RimWorld.PawnCacheRenderer.RenderPawn)), HarmonyLib.HarmonyPrefix]
		private static void CacheRenderPawnPrefix(Pawn pawn, ref float cameraZoom, bool portrait)
		{
			if (portrait)
			{
				cameraZoom *= 1f / GetScale(pawn);
			}
		}

		[HarmonyLib.HarmonyPatch(typeof(GlobalTextureAtlasManager), nameof(GlobalTextureAtlasManager.TryGetPawnFrameSet)), HarmonyLib.HarmonyPrefix]
		private static bool TryGetPawnFrameSetPrefix(Pawn pawn)
		{
			if (GetScale(pawn) > 1.0f)
				return false;

			return true;
		}


		// Calculate the scale multiplier based on Pawnmorpher's BodySize multiplier
		// TODO: Add a toggle to allow scaling by any body size difference compared to normal instead
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float GetScale(Pawn pawn)
		{
			if (_currentPawn != pawn)
			{
				_currentPawn = pawn;
				_currentScaledBodySize = Mathf.Sqrt(StatsUtility.GetStat(pawn, PMStatDefOf.PM_BodySize, 300) ?? 1f);
			}

			return _currentScaledBodySize;
		}

		// Offset rendered pawn from actual position to move selection box to their feet.
		[HarmonyLib.HarmonyPatch(typeof(Pawn), nameof(Pawn.DrawAt)), HarmonyLib.HarmonyPrefix]
		private static void DrawAt(ref Vector3 drawLoc, bool flip, Pawn __instance)
		{
			float bodySize = GetScale(__instance);
			// Don't offset draw position of animals sprites, and only care about those with more than 1 body size.
			if (__instance.RaceProps.Humanlike && bodySize != 1)
			{
				// Draw location is the full position not an offset, so find offset based on scale assing a ratio of 1 to 1.
				// Offset drawn pawn sprite with half the height upward. 1 bodysize = 1 height.
				// Only offset when standing.
				if (__instance.GetPosture() == RimWorld.PawnPosture.Standing)
					drawLoc.z += (bodySize - __instance.RaceProps.baseBodySize) / 2f;
			}
		}

		// Apply scale offset to head position.
		[HarmonyLib.HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.BaseHeadOffsetAt)), HarmonyLib.HarmonyPostfix]
		private static void BaseHeadOffsetAt(Rot4 rotation, ref Vector3 __result, Pawn ___pawn)
		{
			float bodySize = GetScale(___pawn);
			if (bodySize == 1)
				return;

			float size = Mathf.Floor(bodySize * 10) / 10;
			__result.z = __result.z * size;
			__result.x = __result.x * size;
		}
	}
}
