using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Pawnmorph.User_Interface.Settings
{
    internal class Dialog_VisibleRaceSelection : Window
    {
        private const string APPLY_BUTTON_LOC_STRING = "ApplyButtonText";
        private const string RESET_BUTTON_LOC_STRING = "ResetButtonText";
        private const string CANCEL_BUTTON_LOC_STRING = "CancelButtonText";
        private static Vector2 APPLY_BUTTON_SIZE = new Vector2(120f, 40f);
        private static Vector2 RESET_BUTTON_SIZE = new Vector2(120f, 40f);
        private static Vector2 CANCEL_BUTTON_SIZE = new Vector2(120f, 40f);
        private const float SPACER_SIZE = 17f;



        private Vector2 _scrollPosition = new Vector2(0, 0);
        private string _searchText = string.Empty;
        private IEnumerable<AlienRace.ThingDef_AlienRace> _aliens;
        private Dictionary<AlienRace.ThingDef_AlienRace, bool> _selectedAliens;
        List<string> _settingsReference;

        public Dialog_VisibleRaceSelection(List<string> settingsReference)
        {
            _settingsReference = settingsReference;
            _selectedAliens = new Dictionary<AlienRace.ThingDef_AlienRace, bool>();
        }

        public override void PostOpen()
        {
            base.PostOpen();
            RefreshAliens();
        }

        private void RefreshAliens()
        {
            IEnumerable<AlienRace.ThingDef_AlienRace> aliens = DefDatabase<AlienRace.ThingDef_AlienRace>.AllDefsListForReading;

            aliens = aliens.Except((AlienRace.ThingDef_AlienRace)ThingDef.Named("Human"));

            // Exclude implicit and explicit morph races.
            aliens = aliens.Except(Hybrids.RaceGenerator.ImplicitRaces);
            aliens = aliens.Except(MorphDef.AllDefs.Where(m => m.ExplicitHybridRace != null).Select(m => m.ExplicitHybridRace).Cast<AlienRace.ThingDef_AlienRace>());
            aliens = aliens.Where(x => MutagenDefOf.defaultMutagen.CanInfect(x));

            _aliens = aliens;
            _selectedAliens = aliens.Where(x => _settingsReference.Contains(x.defName)).ToDictionary(x => x, x => true);
        }


        public override void DoWindowContents(Rect inRect)
        {
            float curY = 0;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0, curY, inRect.width, Text.LineHeight), "enableMutationVisualsHeader".Translate());

            curY += Text.LineHeight;

            Text.Font = GameFont.Small;
            Rect descriptionRect = new Rect(0, curY, inRect.width, 60);
            Widgets.Label(descriptionRect, "enableMutationVisualsText".Translate());

            curY += descriptionRect.height;

            _searchText = Widgets.TextArea(new Rect(0, curY, 200f, 28f), _searchText);
            if (Widgets.ButtonText(new Rect(205, curY, 28, 28), "X"))
                _searchText = "";

            curY += 35;

            float totalHeight = inRect.height - Math.Max(APPLY_BUTTON_SIZE.y, Math.Max(RESET_BUTTON_SIZE.y, CANCEL_BUTTON_SIZE.y));
            totalHeight -= 100;

            Rect listbox = new Rect(0, 0, inRect.width - 20, (_aliens.Count() + 1) * Text.LineHeight);
            Widgets.BeginScrollView(new Rect(0, curY, inRect.width, totalHeight), ref _scrollPosition, listbox);

            Text.Font = GameFont.Tiny;
            Listing_Standard lineListing = new Listing_Standard(listbox, () => _scrollPosition);
            lineListing.Begin(listbox);

            string searchText = _searchText.ToLower();
            foreach (var item in _aliens)
            {
                if (searchText == "" || item.LabelCap.ToString().ToLower().Contains(searchText))
                {
                    bool current = _selectedAliens.TryGetValue(item, false);
                    lineListing.CheckboxLabeled(item.LabelCap, ref current, item.modContentPack.ModMetaData.Name);
                    _selectedAliens[item] = current;
                }
            }
            lineListing.End();

            Widgets.EndScrollView();

            // Draw the apply, reset and cancel buttons.
            float buttonVertPos = inRect.height - Math.Max(APPLY_BUTTON_SIZE.y, Math.Max(RESET_BUTTON_SIZE.y, CANCEL_BUTTON_SIZE.y));
            float applyHorPos = inRect.width / 2 - APPLY_BUTTON_SIZE.x - RESET_BUTTON_SIZE.x / 2 - SPACER_SIZE;
            float resetHorPos = inRect.width / 2 - RESET_BUTTON_SIZE.x / 2;
            float cancelHorPos = inRect.width / 2 + RESET_BUTTON_SIZE.x / 2 + SPACER_SIZE;
            if (Widgets.ButtonText(new Rect(applyHorPos, buttonVertPos, APPLY_BUTTON_SIZE.x, APPLY_BUTTON_SIZE.y), APPLY_BUTTON_LOC_STRING.Translate()))
            {
                OnAcceptKeyPressed();
            }
            if (Widgets.ButtonText(new Rect(resetHorPos, buttonVertPos, RESET_BUTTON_SIZE.x, RESET_BUTTON_SIZE.y), RESET_BUTTON_LOC_STRING.Translate()))
            {
                RimWorld.SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
                RefreshAliens();
            }
            if (Widgets.ButtonText(new Rect(cancelHorPos, buttonVertPos, CANCEL_BUTTON_SIZE.x, CANCEL_BUTTON_SIZE.y), CANCEL_BUTTON_LOC_STRING.Translate()))
            {
                OnCancelKeyPressed();
            }
        }



        private void ApplyChanges()
        {
            Find.WindowStack.Add(new Dialog_Popup("requiresRestart".Translate(), new Vector2(300, 100)));
            _settingsReference.Clear();
            _settingsReference.AddRange(_selectedAliens.Where(x => x.Value).Select(x => x.Key.defName).ToList());
        }

        public override void OnCancelKeyPressed()
        {
            base.OnCancelKeyPressed();
        }

        public override void OnAcceptKeyPressed()
        {
            ApplyChanges();

            base.OnAcceptKeyPressed();
        }
    }
}
