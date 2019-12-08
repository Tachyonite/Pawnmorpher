// Need_Control.cs modified by Iron Wolf for Pawnmorph on 12/07/2019 1:48 PM
// last updated 12/07/2019  1:49 PM

using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using static Pawnmorph.InstinctUtilities;

namespace Pawnmorph
{
    /// <summary>
    ///     need that represents a sapient animal's control or humanity left
    /// </summary>
    [StaticConstructorOnStartup]
    public class Need_Control : Need //TODO make this instinct based not control based 
    {

        [NotNull]
        private static readonly Texture2D BarInstantMarkerTex;

        [NotNull]
        private static readonly Texture2D NeedUnitDividerTex;

        
        static Need_Control()
        {
            NeedUnitDividerTex = ContentFinder<Texture2D>.Get("UI/Misc/NeedUnitDivider", true);
            BarInstantMarkerTex = ContentFinder<Texture2D>.Get("UI/Misc/BarInstantMarker", true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Need_Control"/> class.
        /// </summary>
        public Need_Control() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Need_Control"/> class.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        public Need_Control(Pawn pawn):base(pawn)
        {

        }

        /// <summary>
        ///     Gets the maximum level.
        /// </summary>
        /// <value>
        ///     The maximum level.
        /// </value>
        public override float MaxLevel =>
            Mathf.Max(CalculateNetResistance(pawn) / AVERAGE_RESISTANCE, 0.01f); //this should never be zero 


        /// <summary>
        ///     Adds the instinct change to this need
        /// </summary>
        /// <param name="instinctChange">The instinct change.</param>
        public void AddInstinctChange(int instinctChange)
        {
            CurLevel += CalculateControlChange(pawn, instinctChange) / AVERAGE_RESISTANCE;
        }

        /// <summary>
        ///     called every so often by the need manager.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void NeedInterval()
        {
            //empty 
        }

        void DrawBarThreshold(Rect barRect, float threshPct)
        {
            float num = (float)((barRect.width <= 60f) ? 1 : 2);
            Rect position = new Rect(barRect.x + barRect.width * threshPct - (num - 1f), barRect.y + barRect.height / 2f, num, barRect.height / 2f);
            Texture2D image;
            if (threshPct < this.CurLevelPercentage)
            {
                image = BaseContent.BlackTex;
                GUI.color = new Color(1f, 1f, 1f, 0.9f);
            }
            else
            {
                image = BaseContent.GreyTex;
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
            }
            GUI.DrawTexture(position, image);
            GUI.color = Color.white;
        }

        void DrawBarDivision(Rect barRect, float threshPct)
        {
            float num = 5f;
            Rect rect = new Rect(barRect.x + barRect.width * threshPct - (num - 1f), barRect.y, num, barRect.height);
            if (threshPct < this.CurLevelPercentage)
            {
                GUI.color = new Color(0f, 0f, 0f, 0.9f);
            }
            else
            {
                GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
            Rect position = rect;
            position.yMax = position.yMin + 4f;
            GUI.DrawTextureWithTexCoords(position, NeedUnitDividerTex, new Rect(0f, 0.5f, 1f, 0.5f));
            Rect position2 = rect;
            position2.yMin = position2.yMax - 4f;
            GUI.DrawTextureWithTexCoords(position2, NeedUnitDividerTex, new Rect(0f, 0f, 1f, 0.5f));
            Rect position3 = rect;
            position3.yMin = position.yMax;
            position3.yMax = position2.yMin;
            if (position3.height > 0f)
            {
                GUI.DrawTextureWithTexCoords(position3, NeedUnitDividerTex, new Rect(0f, 0.4f, 1f, 0.2f));
            }
            GUI.color = Color.white;
        }

        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = 2147483647, float customMargin = -1, bool drawArrows = true,
                                       bool doTooltip = true)
        {
            if (rect.height > 70f)
            {
                float num = (rect.height - 70f) / 2f;
                rect.height = 70f;
                rect.y += num;
            }
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }
            if (doTooltip)
            {
                TooltipHandler.TipRegion(rect, new TipSignal(() => this.GetTipString(), rect.GetHashCode()));
            }
            float num2 = 14f;
            float num3 = (customMargin < 0f) ? (num2 + 15f) : customMargin;
            if (rect.height < 50f)
            {
                num2 *= Mathf.InverseLerp(0f, 50f, rect.height);
            }
            Text.Font = ((rect.height <= 55f) ? GameFont.Tiny : GameFont.Small);
            Text.Anchor = TextAnchor.LowerLeft;
            Rect rect2 = new Rect(rect.x + num3 + rect.width * 0.1f, rect.y, rect.width - num3 - rect.width * 0.1f, rect.height / 2f);
            Widgets.Label(rect2, this.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect3 = new Rect(rect.x, rect.y + rect.height / 2f, rect.width, rect.height / 2f);
            rect3 = new Rect(rect3.x + num3, rect3.y, rect3.width - num3 * 2f, rect3.height - num2);
            Rect rect4 = rect3;
            float num4 = 1f;
            if (this.def.scaleBar && this.MaxLevel < 1f)
            {
                num4 = this.MaxLevel;
            }
            rect4.width *= num4;
            Rect barRect = Widgets.FillableBar(rect4, this.CurLevelPercentage);
            if (drawArrows)
            {
                Widgets.FillableBarChangeArrows(rect4, this.GUIChangeArrow);
            }
            if (this.threshPercents != null)
            {
                for (int i = 0; i < Mathf.Min(this.threshPercents.Count, maxThresholdMarkers); i++)
                {
                    this.DrawBarThreshold(barRect, this.threshPercents[i] * num4);
                }
            }
            if (this.def.scaleBar)
            {
                int num5 = 1;
                while ((float)num5 < this.MaxLevel)
                {
                    this.DrawBarDivision(barRect, (float)num5 / this.MaxLevel * num4);
                    num5++;
                }
            }
            float curInstantLevelPercentage = this.CurInstantLevelPercentage;
            if (curInstantLevelPercentage >= 0f)
            {
                this.DrawBarInstantMarkerAt(rect3, curInstantLevelPercentage * num4);
            }
            if (!this.def.tutorHighlightTag.NullOrEmpty())
            {
                UIHighlighter.HighlightOpportunity(rect, this.def.tutorHighlightTag);
            }
            Text.Font = GameFont.Small;
        }

        /// <summary>
        ///     Sets the initial level.
        /// </summary>
        public override void SetInitialLevel()
        {
            CurLevelPercentage = 1;
        }
    }
}