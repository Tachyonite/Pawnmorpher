using Pawnmorph.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static AlienRace.AlienPartGenerator;
using static RimWorld.PawnUtility;

namespace Pawnmorph.HPatches.Optional
{
    [OptionalPatch("Pawn scaling.", "Changes the size of pawns based on their bodysize.\nThis patch is experimental and may incur visual bugs and glitches.", nameof(_enabled), true)]
    [HarmonyLib.HarmonyPatch]
    static class PawnScaling
    {
        static Dictionary<float, AlienGraphicMeshSet> _meshCache;
        static bool _enabled = true;

        static bool Prepare(MethodBase original)
        {
            if (original == null && _enabled)
            {
                StatsUtility.GetEvents(PMStatDefOf.PM_BodySize).StatChanged += PawnScaling_StatChanged;
                _meshCache = new Dictionary<float, AlienGraphicMeshSet>();
                Log.Message("[PM] Optional pawn scaling patch enabled.");
            }

            return _enabled;
        }

        private static void PawnScaling_StatChanged(Verse.Pawn pawn, RimWorld.StatDef stat, float oldValue, float newValue)
        {
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                ResolveAllGraphics(pawn);
            });
        }

        private static void SetCompScales(AlienComp comp, float size)
        {
            comp.customDrawSize = new Vector2(size, size);
            comp.customHeadDrawSize = new Vector2(size, size);
            comp.customPortraitDrawSize = new Vector2(size, size);
            comp.customPortraitHeadDrawSize = new Vector2(size, size);
        }


        [HarmonyLib.HarmonyPatch(typeof(AlienComp), nameof(AlienComp.PostSpawnSetup)), HarmonyLib.HarmonyPostfix]
        private static void PostSpawnSetup(bool respawningAfterLoad, AlienComp __instance)
        {
            SetCompScales(__instance, GetScale(((Pawn)__instance.parent).BodySize));
        }


        [HarmonyLib.HarmonyAfter(new string[] { "erdelf.HumanoidAlienRaces" })]
        [HarmonyLib.HarmonyPatch(typeof(Verse.PawnGraphicSet), nameof(Verse.PawnGraphicSet.ResolveAllGraphics)), HarmonyLib.HarmonyPostfix]
        private static void ResolveAllGraphics(Pawn ___pawn)
        {
            float bodysize = ___pawn.BodySize;
            if (bodysize == 1)
                return;

            AlienComp comp = CompCacher<AlienComp>.GetCompCached(___pawn);
            if (comp != null)
            {
                float size = GetScale(bodysize);

                SetCompScales(comp, size);

                if (_meshCache.TryGetValue(size, out var mesh) == false)
                {
                    mesh = new AlienGraphicMeshSet()
                    {
                        bodySet = new GraphicMeshSet(1.5f * size, 1.5f * size),
                        headSet = new GraphicMeshSet(1.5f * size, 1.5f * size),
                        hairSetAverage = new GraphicMeshSet(1.5f * size, 1.5f * size)
                    };
                    _meshCache.Add(size, mesh);
                }
                
                comp.alienGraphics = mesh;
                comp.alienHeadGraphics = mesh;
                comp.alienPortraitGraphics = mesh;
                comp.alienPortraitHeadGraphics = mesh;
            };
        }


        [HarmonyLib.HarmonyPatch(typeof(AlienRace.HarmonyPatches), nameof(AlienRace.HarmonyPatches.DrawAddonsFinalHook)), HarmonyLib.HarmonyPostfix]
        public static void DrawAddonsFinalHook(Pawn pawn, AlienRace.AlienPartGenerator.BodyAddon addon, ref Graphic graphic, ref Vector3 offsetVector, ref float angle, ref Material mat)
        {
            float value = GetScale(pawn.BodySize);
            offsetVector.x *= value;
            // Don't affect y layer
            offsetVector.z *= value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetScale(float bodysize)
        {
            return Mathf.Sqrt(bodysize);
        }


        [HarmonyLib.HarmonyPatch(typeof(Pawn), nameof(Pawn.DrawAt)), HarmonyLib.HarmonyPrefix]
        public static void DrawAt(ref Vector3 drawLoc, bool flip, Pawn __instance)
        {
            if (__instance.BodySize > 1)
            {
                // Draw location is the full position not an offset, so find offset based on scale assing a ratio of 1 to 1.
                // Offset drawn pawn sprite with half the height upward. 1 bodysize = 1 height.
                // Only offset when standing.
                if (__instance.GetPosture() == RimWorld.PawnPosture.Standing)
                    drawLoc.z += GetScale(__instance.BodySize) / 2;
            }
        }


        [HarmonyLib.HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.BaseHeadOffsetAt)), HarmonyLib.HarmonyPostfix]
        public static void BaseHeadOffsetAt(Rot4 rotation, ref Vector3 __result, Pawn ___pawn)
        {
            if (___pawn.BodySize == 1)
                return;

            float size = Mathf.Floor(GetScale(___pawn.BodySize) * 10) / 10;
            __result.z = __result.z * size;
            __result.x = __result.x * size;
        }
    }
}
