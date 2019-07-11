using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Multiplayer.API;

namespace Pawnmorph
{
    /// <summary>
    /// Draw an info icon for mutations with their descriptions in tooltip
    /// </summary>
    [HarmonyPatch(typeof(HealthCardUtility), "DrawHediffRow")]
    public static class PatchHealthCardUtilityDrawHediffRow
    {
        private static readonly Texture2D icon = ContentFinder<Texture2D>.Get("UI/Icons/Info", true);
        static void Prefix(Rect rect, Pawn pawn, IEnumerable<Hediff> diffs, ref float curY)
        {
            if (diffs.OfType<Hediff_AddedMutation>().Where(x => x.def.description != null).FirstOrDefault() == null) return;

            float firstRowWidth = rect.width * 0.375f;
            Rect rectIcon = new Rect(firstRowWidth - icon.width - 4, curY + 1, icon.width, icon.height);
            GUI.DrawTexture(rectIcon, icon);
            TooltipHandler.TipRegion(rectIcon, () => Tooltip(diffs), (int)curY + 117857);
        }

        static string Tooltip(IEnumerable<Hediff> diffs)
        {
            StringBuilder tooltip = new StringBuilder();
            foreach (Hediff_AddedMutation mutation in diffs.OfType<Hediff_AddedMutation>())
            {
                if (mutation.def.description == null) continue;

                tooltip.AppendLine(mutation.MutationDescription);
            }

            return tooltip.ToString();
        }
    }
}
