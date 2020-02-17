using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
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
        static void Prefix(Rect rect, Pawn pawn, IEnumerable<Hediff> diffs, ref float curY)
        {
            if (diffs.OfType<IDescriptiveHediff>().FirstOrDefault(x => x.Description != null) == null) return;

            float firstRowWidth = rect.width * 0.375f;
            Rect rectIcon = new Rect(firstRowWidth - icon.width - 4, curY + 1, icon.width, icon.height);
            GUI.DrawTexture(rectIcon, icon);
            TooltipHandler.TipRegion(rectIcon, () => Tooltip(diffs), (int)curY + 117857);
        }

        static string Tooltip(IEnumerable<Hediff> diffs)
        {
            StringBuilder tooltip = new StringBuilder();
            foreach (var mutation in diffs.OfType<IDescriptiveHediff>())
            {
                var desc = mutation.Description;
                if (desc == null) continue;

                tooltip.AppendLine(desc);
            }

            return tooltip.ToString();
        }
    }
}
