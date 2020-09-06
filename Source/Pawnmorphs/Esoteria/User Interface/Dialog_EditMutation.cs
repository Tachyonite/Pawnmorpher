using AlienRace;
using Pawnmorph.Hediffs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Pawnmorph.User_Interface
{
    class Dialog_EditMutation : Window
    {
        public List<Hediff_AddedMutation> mutations;

        public Dialog_EditMutation(List<Hediff_AddedMutation> mutations)
        {
            this.mutations = mutations;
            doCloseX = true;
            resizeable = true;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(100f, 75f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            float curY = 0f;
            foreach (Hediff_AddedMutation mutation in mutations)
            {
                string slideLabel = $"{mutation.Part.LabelCap} - {mutation.LabelCap} (Stage {mutation.CurStageIndex})";
                float slideHeight = Text.CalcSize(slideLabel).y;
                Widgets.HorizontalSlider(new Rect(inRect.x, curY, inRect.width, slideHeight), mutation.Severity, mutation.def.minSeverity, mutation.def.maxSeverity, leftAlignedLabel: slideLabel, rightAlignedLabel: mutation.Severity.ToString(), roundTo: 0.001f);
                curY += slideHeight;
            }
        }
    }
}
