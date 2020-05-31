using Pawnmorph.Hediffs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Pawnmorph.User_Interface
{
    class Dialog_PartPicker : Window
    {
        private Pawn pawn;
        private Vector2 scrollSize;
        private Vector2 scrollPos;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(500f, 500f);
            }
        }

        public Dialog_PartPicker(Pawn pawn)
        {
            this.pawn = pawn;
            forcePause = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Get all BodyPartDefs that mutations can apply to.
            List<BodyPartDef> allValidParts = DefDatabase<MutationDef>.AllDefs.SelectMany(m => m.parts).Distinct().ToList();

            // Get all possible parts in the pawn's body def that can have mutations applied to them.
            List<BodyPartRecord> pawnPartsThatCanGetMutations = pawn.RaceProps.body.AllParts.Where(m => allValidParts.Contains(m.def)).ToList();

            // Get all mutations on the pawn.
            List<Hediff_AddedMutation> mutationsOnPawn = pawn.health.hediffSet.hediffs.Where(m => m.def.GetType() == typeof(MutationDef)).Cast<Hediff_AddedMutation>().ToList();

            // Sentinel value
            float curY = 0f;

            // Title the window.
            Text.Font = GameFont.Medium;
            string title = $"Part Picker - {pawn.Name.ToStringShort} ({pawn.def.LabelCap})";
            Widgets.Label(new Rect(0f, 0f, InitialSize.x, Text.CalcHeight(title, InitialSize.x)), title);
            curY += 50f;

            // Create a scrollable List of all valid locations and their current hediffs
            Rect outRect = new Rect(0f, curY, InitialSize.x, InitialSize.y - curY);
            Rect viewRect = new Rect(0f, curY, InitialSize.x - 16f, scrollSize.y - curY);
            Widgets.BeginScrollView(outRect, ref scrollPos, viewRect);
            Text.Font = GameFont.Small;
            foreach (BodyPartRecord part in pawnPartsThatCanGetMutations)
            {
                string text = $"{part.Label}: {string.Join(", ", mutationsOnPawn.Where(m => m.Part == part).Select(n => n.LabelCap).ToList())}";
                float textHeight = Text.CalcHeight(text, InitialSize.x);
                Widgets.Label(new Rect(0f, curY, InitialSize.x, textHeight), text);
                curY += textHeight;
            }
            if (Event.current.type == EventType.Layout)
            {
                scrollSize.y = curY;
            }
            Widgets.EndScrollView();
        }
    }
}
