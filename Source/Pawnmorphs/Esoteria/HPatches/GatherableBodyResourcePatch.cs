// GatherableBodyResourcePatch.cs modified by Iron Wolf for Pawnmorph on 12/02/2019 4:26 PM
// last updated 12/02/2019  4:26 PM

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
    
    [HarmonyPatch(typeof(CompHasGatherableBodyResource))]
    [HarmonyPatch(nameof(CompHasGatherableBodyResource.Gathered))]
    internal static class GatherableBodyResourcePatch
    {
        [HarmonyPostfix]
        internal static void GenerateThoughtsAbout([NotNull] Pawn doer, [NotNull] CompHasGatherableBodyResource __instance)
        {
            var selfPawn = __instance.parent as Pawn;
            if (!selfPawn.IsFormerHuman() || selfPawn.needs?.mood == null) return; 

            //TODO put this in a def extension or something 
            if (__instance is CompMilkable)
            {
                if (ThoughtMaker.MakeThought(PMThoughtDefOf.SapientAnimalMilked) is Thought_Memory memory)
                {
                    selfPawn.TryGainMemory(memory); 
                }

            }
        }
    }
}