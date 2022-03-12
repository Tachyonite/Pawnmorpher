using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Pawnmorph.Utilities;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
    /// <summary> Draw an info icon for mutations with their descriptions in tooltip. </summary>
    [HarmonyPatch(typeof(HealthCardUtility), "DrawHediffRow")]
    [StaticConstructorOnStartup]
    public static class PatchHealthCardUtilityDrawHediffRow
    {
        private static readonly Texture2D icon = ContentFinder<Texture2D>.Get("UI/Icons/Info", true);

        [HarmonyAfter("PeteTimesSix.CompactHediffs")]
        static void Prefix(Rect rect, Pawn pawn, IEnumerable<Hediff> diffs, ref float curY)
        {
            var dLst = diffs.MakeSafe().ToList(); 
            if (dLst.OfType<IDescriptiveHediff>().FirstOrDefault(x => x.Description != null) == null) return;

            float firstRowWidth = rect.width * 0.275f;
            Rect rectIcon = new Rect(firstRowWidth - icon.width - 4, curY + 1, icon.width, icon.height);
            var toolTipRect = rect;
            toolTipRect.x = rectIcon.x;
            toolTipRect.y = rectIcon.y;
            toolTipRect.height = rectIcon.height * dLst.Count; 
            
            //GUI.DrawTexture(rectIcon, icon);
            TooltipHandler.TipRegion(toolTipRect, () => Tooltip(dLst), (int) curY + 117857);
        }

        static string Tooltip(IEnumerable<Hediff> diffs)
        {
            StringBuilder tooltip = new StringBuilder();
            foreach (var mutation in diffs)
            {
                if (mutation is IDescriptiveHediff descriptive)
                    tooltip.AppendLine(descriptive.Description);

#if DEBUG
                tooltip.AppendLine(mutation.DebugString());
#endif
            }

            return tooltip.ToString();

        }
    }
}

