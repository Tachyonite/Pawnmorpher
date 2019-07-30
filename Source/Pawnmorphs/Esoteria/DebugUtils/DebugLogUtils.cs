// DebugLogUtils.cs modified by Iron Wolf for Pawnmorph on 07/30/2019 6:01 PM
// last updated 07/30/2019  6:01 PM

using System.Text;
using RimWorld;
using Verse;

namespace Pawnmorph.DebugUtils
{
    [HasDebugOutput]
    public static class DebugLogUtils
    {
        [Category("Pawnmorpher")]
        [DebugOutput]
        public static void OutputRelationshipPatches()
        {
            var defs = DefDatabase<PawnRelationDef>.AllDefs;

            StringBuilder builder = new StringBuilder(); 
            foreach (PawnRelationDef relationDef in defs)
            {
                var modPatch = relationDef.GetModExtension<Thoughts.RelationshipDefExtension>();
                if (modPatch != null)
                {
                    builder.AppendLine($"{relationDef.defName}:");
                    builder.AppendLine($"\t\ttransformThought:{modPatch.transformThought?.defName ?? "NULL"}");
                    builder.AppendLine($"\t\ttransformThoughtFemale:{modPatch.transformThoughtFemale?.defName ?? "NULL"}"); 
                }
                else
                {
                    builder.AppendLine($"{relationDef.defName} no mod patch found!!");
                }
            }

            Log.Message(builder.ToString()); 
        }


    }
}