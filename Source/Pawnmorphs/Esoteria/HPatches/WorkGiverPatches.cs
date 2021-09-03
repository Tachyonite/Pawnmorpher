// WorkGiverPatches.cs created by Iron Wolf for Pawnmorph on 05/10/2020 7:48 AM
// last updated 05/10/2020  7:49 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Pawnmorph.DefExtensions;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.HPatches
{
    static class WorkGiverPatches
    {
        private static bool CanInteract(Pawn p)
        {
            return p.RaceProps.Animal || p.GetIntelligence() == Intelligence.Animal;
        }

        static class InteractionPatches
        {
            [HarmonyPatch( typeof(WorkGiver_InteractAnimal),"CanInteractWithAnimal", new Type[] {typeof(Pawn), typeof(Pawn), typeof(string), typeof(bool), typeof(bool) , typeof(bool) ,typeof(bool)}),
             HarmonyPrefix]
            static bool DontInteractSelfFix(ref bool __result, Pawn pawn, Pawn animal, ref string jobFailReason, bool forced, bool canInteractWhileSleeping, bool ignoreSkillRequirements)
            {
                if (pawn == animal)
                {
                    __result = false;
                    return false; 
                }

                return true; 
            }
        }


        [HarmonyPatch(typeof(WorkGiver_GatherAnimalBodyResources))]
        static class GatherAnimalBodyResourcesPatches
        {
            [HarmonyPatch(nameof(WorkGiver_GatherAnimalBodyResources.HasJobOnThing))]
            [HarmonyPostfix]
            private static void DontInteractSelfFix(ref bool __result, Pawn pawn, Thing t, bool forced)
            {
                if (__result) __result = pawn != t;
            }

        }

        [HarmonyPatch(typeof(WorkGiver_Slaughter))]
        private static class SlaughterPatches
        {
            [HarmonyPatch("HasJobOnThing")]
            [HarmonyPostfix]
            private static void DontInteractSelfFix(ref bool __result, Pawn pawn, Thing t, bool forced)
            {
                if (__result) __result = pawn != t;
            }
        }



        [HarmonyPatch(typeof(WorkGiver_Train))]
        private static class TrainPatches
        {
            
            [HarmonyPatch("JobOnThing")]
            [HarmonyTranspiler]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> instructionList = instructions.ToList();
                Log.Message("Patching WorkGiver Tamed");
                for (var i = 0; i < instructionList.Count - 1; i++)
                {
                    CodeInstruction jInst = instructionList[i + 1];
                    CodeInstruction iInst = instructionList[i];
                    if (iInst.opcode == OpCodes.Callvirt && (MethodInfo) iInst.operand == PatchUtilities.GetRacePropsMethod)
                        if (jInst.opcode == OpCodes.Callvirt
                         && (MethodInfo) jInst.operand == PatchUtilities.RimworldIsAnimalMethod)
                        {
                            iInst.opcode = OpCodes.Call;
                            iInst.operand =
                                typeof(WorkGiverPatches).GetMethod(nameof(CanInteract),
                                                                   BindingFlags.NonPublic | BindingFlags.Static);
                            jInst.opcode = OpCodes.Nop;
                            jInst.operand = null;
                            break;
                        }
                }

                return instructionList;
            }
        }

        /// <summary>
        /// Calculates a plant job for an extended plant
        /// 
        /// Yes, this whole method is a copy-paste of the original. Transpiling would be cleaner,
        /// but in the original the first call to PlantUtility.GrowthSeasonNow() is called before the
        /// plant def is even calculated, so successfuly transpiling it would involve icky amounts
        /// of reordering method calls.
        /// </summary>
        /// <returns>The plant job.</returns>
        /// <param name="pawn">Pawn.</param>
        /// <param name="c">C.</param>
        /// <param name="forced">If set to <c>true</c> forced.</param>
        /// <param name="wantedPlantDef">Wanted plant def.</param>
        /// <param name="plantInfo">Additional plant info.</param>
        /// <param name="defLabel">The def label of the workgiver</param>
        public static Job GetPlantJob(Pawn pawn, IntVec3 c, bool forced, ThingDef wantedPlantDef, AdditionalPlantInfo plantInfo, string defLabel)
        {
            Map map = pawn.Map;
            if (c.IsForbidden(pawn))
            {
                return null;
            }
            if (!plantInfo.GrowthSeasonNow(c, map))
            {
                return null;
            }
            List<Thing> thingList = c.GetThingList(map);
            Zone_Growing zone_Growing = c.GetZone(map) as Zone_Growing;
            bool flag = false;
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing thing = thingList[i];
                if (thing.def == wantedPlantDef)
                {
                    return null;
                }
                if ((thing is Blueprint || thing is Frame) && thing.Faction == pawn.Faction)
                {
                    flag = true;
                }
            }
            if (flag)
            {
                Thing edifice = c.GetEdifice(map);
                if (edifice == null || edifice.def.fertility < 0f)
                {
                    return null;
                }
            }
            if (wantedPlantDef.plant.cavePlant)
            {
                if (!c.Roofed(map))
                {
                    JobFailReason.Is("CantSowCavePlantBecauseUnroofed".Translate());
                    return null;
                }
                if (map.glowGrid.GameGlowAt(c, true) > 0f)
                {
                    JobFailReason.Is("CantSowCavePlantBecauseOfLight".Translate());
                    return null;
                }
            }
            if (wantedPlantDef.plant.interferesWithRoof && c.Roofed(pawn.Map))
            {
                return null;
            }
            Plant plant = c.GetPlant(map);
            if (plant != null && plant.def.plant.blockAdjacentSow)
            {
                if (!pawn.CanReserve(plant, 1, -1, null, forced) || plant.IsForbidden(pawn))
                {
                    return null;
                }
                if (zone_Growing != null && !zone_Growing.allowCut)
                {
                    return null;
                }
                if (!PlantUtility.PawnWillingToCutPlant_Job(plant, pawn))
                {
                    return null;
                }
                return JobMaker.MakeJob(JobDefOf.CutPlant, plant);
            }
            Thing thing2 = PlantUtility.AdjacentSowBlocker(wantedPlantDef, c, map);
            if (thing2 != null)
            {
                Plant plant2 = thing2 as Plant;
                if (plant2 != null && pawn.CanReserveAndReach(plant2, PathEndMode.Touch, Danger.Deadly, 1, -1, null, forced) && !plant2.IsForbidden(pawn))
                {
                    IPlantToGrowSettable plantToGrowSettable = plant2.Position.GetPlantToGrowSettable(plant2.Map);
                    if (plantToGrowSettable == null || plantToGrowSettable.GetPlantDefToGrow() != plant2.def)
                    {
                        Zone_Growing zone_Growing2 = c.GetZone(map) as Zone_Growing;
                        Zone_Growing zone_Growing3 = plant2.Position.GetZone(map) as Zone_Growing;
                        if ((zone_Growing2 != null && !zone_Growing2.allowCut) || (zone_Growing3 != null && !zone_Growing3.allowCut))
                        {
                            return null;
                        }
                        if (!PlantUtility.PawnWillingToCutPlant_Job(plant2, pawn))
                        {
                            return null;
                        }
                        return JobMaker.MakeJob(JobDefOf.CutPlant, plant2);
                    }
                }
                return null;
            }
            if (wantedPlantDef.plant.sowMinSkill > 0 && pawn.skills != null && pawn.skills.GetSkill(SkillDefOf.Plants).Level < wantedPlantDef.plant.sowMinSkill)
            {
                JobFailReason.Is("UnderAllowedSkill".Translate(wantedPlantDef.plant.sowMinSkill), defLabel);
                return null;
            }
            for (int j = 0; j < thingList.Count; j++)
            {
                Thing thing3 = thingList[j];
                if (thing3.def.BlocksPlanting(false))
                {
                    if (!pawn.CanReserve(thing3, 1, -1, null, forced))
                    {
                        return null;
                    }
                    if (thing3.def.category == ThingCategory.Plant)
                    {
                        if (!thing3.IsForbidden(pawn))
                        {
                            if (zone_Growing != null && !zone_Growing.allowCut)
                            {
                                return null;
                            }
                            if (!PlantUtility.PawnWillingToCutPlant_Job(thing3, pawn))
                            {
                                return null;
                            }
                            return JobMaker.MakeJob(JobDefOf.CutPlant, thing3);
                        }
                        return null;
                    }
                    if (thing3.def.EverHaulable)
                    {
                        return HaulAIUtility.HaulAsideJobFor(pawn, thing3);
                    }
                    return null;
                }
            }
            if (!wantedPlantDef.CanNowPlantAt(c, map, false) || !plantInfo.GrowthSeasonNow(c, map) || !pawn.CanReserve(c, 1, -1, null, forced))
            {
                return null;
            }
            Job job = JobMaker.MakeJob(JobDefOf.Sow, c);
            job.plantDefToSow = wantedPlantDef;
            return job;
        }

        /// <summary>
        /// Patch the sowing workgiver to check the plant's growing season if it's an extended plant
        /// </summary>
        [HarmonyPatch(typeof(WorkGiver_GrowerSow))]
        private static class SowingPatches
        {
            [HarmonyPatch(nameof(WorkGiver_GrowerSow.JobOnCell))]
            [HarmonyPrefix]
            static bool FixSowingCheck(Pawn pawn, IntVec3 c, bool forced, WorkGiver_GrowerSow __instance, ref Job __result, ref ThingDef ___wantedPlantDef)
            {
                // I have no idea why vanilla has this static field.  Caching I guess?
                // Either way, it's kinda gross but vanilla uses it like this so it's probably(?) fine
                if (___wantedPlantDef == null)
                    ___wantedPlantDef = WorkGiver_Grower.CalculateWantedPlantDef(c, pawn.Map);

                // No plant == no job, don't need to defer to the original
                if (___wantedPlantDef == null)
                {
                    __result = null;
                    return false;
                }

                var plantInfo = ___wantedPlantDef.GetModExtension<AdditionalPlantInfo>();
                // If we don't have a plant info, just defer to the original
                if (plantInfo == null)
                    return true;

                __result = GetPlantJob(pawn, c, forced, ___wantedPlantDef, plantInfo, __instance.def.label);
                return false;
            }
        }
    }
}