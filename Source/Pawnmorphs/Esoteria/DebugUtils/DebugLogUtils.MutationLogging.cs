// DebugLogUtils.MutationLogging.cs created by Iron Wolf for Pawnmorph on //2020 
// last updated 03/01/2020  6:57 AM

using System.Linq;
using System.Text;
using Harmony;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using RimWorld;
using Verse;

namespace Pawnmorph.DebugUtils
{
#pragma warning disable 1591

    public static partial class DebugLogUtils
    {
        [DebugOutput]
        [Category(MAIN_CATEGORY_NAME)]
        private static void LogAllMutationInfo()
        {
            var builder = new StringBuilder();
            foreach (MutationDef allMutation in MutationDef.AllMutations)
            {
                builder.AppendLine(allMutation.ToStringFull()); 
            }

            Log.Message(builder.ToString());
        }
    }
}