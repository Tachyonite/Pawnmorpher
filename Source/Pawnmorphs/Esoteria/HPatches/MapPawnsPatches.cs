// MapPawnsPatches.cs created by Iron Wolf for Pawnmorph on 02/29/2020 8:52 PM
// last updated 02/29/2020  8:52 PM

using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

#pragma warning disable 1591
namespace Pawnmorph.HPatches
{
#if true
    public class MapPawnsPatches
    {
        [HarmonyPatch(typeof(MapPawns)), HarmonyPatch(nameof(MapPawns.FreeHumanlikesOfFaction))]
        static class AddHumanlikeFormerHumans
        {
            [HarmonyPostfix]
            static void Postfix([NotNull] ref List<Pawn> __result, [NotNull] MapPawns __instance, Faction faction)
            {
                foreach (Pawn p in __instance.AllPawns)
                {
                    if(p.IsSapientFormerHuman() && p.Faction == faction && p.HostFaction == null)
                        __result.Add(p);
                }
            }
        }

        [HarmonyPatch(typeof(MapPawns)), HarmonyPatch(nameof(MapPawns.FreeHumanlikesSpawnedOfFaction))]
        static class AddHumanlikeFormerHumansSpawned
        {
            [HarmonyPostfix]
            static void Postfix([NotNull] ref List<Pawn> __result, [NotNull] MapPawns __instance, Faction faction)
            {
                foreach (Pawn p in __instance.AllPawns)
                {
                    if (p.IsSapientFormerHuman() && p.Faction == faction && p.Spawned && p.HostFaction == null)
                        __result.Add(p);
                }
            }
        }
    }
#endif 
}