// DebugLogUtils.MutationLogging.cs created by Iron Wolf for Pawnmorph on //2020 
// last updated 03/01/2020  6:57 AM

using System.Linq;
using System.Text;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using RimWorld;
using Verse;

namespace Pawnmorph.DebugUtils
{
#pragma warning disable 1591

    public static partial class DebugLogUtils
    {
        [DebugOutput(category = MAIN_CATEGORY_NAME)]
        private static void LogAllMutationInfo()
        {
            var builder = new StringBuilder();
            foreach (MutationDef allMutation in MutationDef.AllMutations)
            {
                builder.AppendLine(allMutation.ToStringFull()); 
            }

            Log.Message(builder.ToString());
        }

        [DebugOutput(category = MAIN_CATEGORY_NAME)]
        static void GetRaceMaxInfluence()
        {
            StringBuilder builder = new StringBuilder(); 
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(t => t.race?.body != null))
            {
                builder.AppendLine($"{thingDef.defName}/{thingDef.label}={MorphUtilities.GetMaxInfluenceOfRace(thingDef)}"); 
            }

            Log.Message(builder.ToString()); 
        }
    }
}