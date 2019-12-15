// HuntingPatches.cs modified by Iron Wolf for Pawnmorph on 12/15/2019 7:40 AM
// last updated 12/15/2019  7:40 AM

using System.Collections.Generic;
using System.Linq;
using Harmony;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.HPatches
{
    static class HuntingPatches
    {
        [HarmonyPatch(typeof(JobDriver_PredatorHunt)), HarmonyPatch("MakeNewToils")]
        static class JobDriver_PredatorHuntPatch
        {
            static void Postfix(ref IEnumerable<Toil> __result, [NotNull] JobDriver_PredatorHunt __instance)
            {
                var saLevel = __instance.pawn.GetFormerHumanStatus();
                if (saLevel != FormerHumanStatus.Sapient) return;
                List<Toil> lst = __result.ToList();
                var addThoughtToil = Toils_General.Do(() => { FormerHumanUtilities.GiveSapientAnimalHuntingThought(__instance.pawn, __instance.Prey);  });
                lst.Add(addThoughtToil);
                __result = lst; 
            }

            
        }
    }
}