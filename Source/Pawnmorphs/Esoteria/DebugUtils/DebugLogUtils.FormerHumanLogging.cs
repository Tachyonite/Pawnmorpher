// DebugLogUtils.FormerHumanLogging.cs created by Iron Wolf for Pawnmorph on //2020 
// last updated 03/01/2020  10:28 AM

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

#pragma warning disable 1591
namespace Pawnmorph.DebugUtils
{
    public static partial class DebugLogUtils
    {
        private const string FH_CATEGORY = MAIN_CATEGORY_NAME +  "-Former Humans"; 

        [DebugOutput(category = FH_CATEGORY)]
        static void LogFormerHumanLordStatus()
        {
            StringBuilder builder = new StringBuilder();

            foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonists.Where(p => p.IsFormerHuman()))
            {
                builder.AppendLine($"{pawn.Name}-{(pawn.GetLord()?.CurLordToil?.GetType().Name).ToStringSafe()}"); 
            }

            Log.Message(builder.ToString()); 
        }

        [DebugOutput(category = FH_CATEGORY)]
        static void PrintAnimalThinkTree()
        {
            var tTree = DefDatabase<ThinkTreeDef>.GetNamed("Animal");
            if (tTree == null) return;

            var outStr = TreeUtilities.PrettyPrintTree(tTree.thinkRoot, GetChildren, GetNodeLabel);

            Log.Message(outStr); 

        }

        static IEnumerable<ThinkNode> GetChildren([NotNull] ThinkNode node)
        {
            if (node is ThinkNode_SubtreesByTag) return Enumerable.Empty<ThinkNode>();
            if (node is ThinkNode_Subtree) return Enumerable.Empty<ThinkNode>();
            return node.subNodes.MakeSafe(); 
        }

        static string GetNodeLabel([NotNull] ThinkNode node)
        {
            var tLabel = node.GetType().Name; 
            if (node is ThinkNode_SubtreesByTag streeTag)
            {
                return tLabel + "-" + streeTag.insertTag; 
            }

            if (node is ThinkNode_Subtree sTree)
            {
                var fInfo = typeof(ThinkNode_Subtree).GetField("treeDef", BindingFlags.Instance | BindingFlags.NonPublic);
                var def = (Def) fInfo.GetValue(sTree);

                return tLabel + "-" + def.defName; 
            }

            return tLabel; 
        }

    }
}