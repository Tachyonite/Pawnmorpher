using Pawnmorph.Utilities.Collections;
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
    internal class Dialog_RaceReplacements : Window
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
        private Dictionary<MorphDef, AlienRace.ThingDef_AlienRace> _selectedReplacements;
        private Dictionary<string, string> _settingsReference;
        private List<MorphDef> _patchedMorphs;
        private ListFilter<MorphDef> _morphs;

        public Dialog_RaceReplacements(Dictionary<string, string> settingsReference)
        {
            _settingsReference = settingsReference;
            _selectedReplacements = new Dictionary<MorphDef, AlienRace.ThingDef_AlienRace>();
            _patchedMorphs = Hybrids.RaceGenerator.ExplicitPatchedRaces;
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

            // Exclude implicit and uninfectable morph races.
            aliens = aliens.Except(Hybrids.RaceGenerator.ImplicitRaces);
            aliens = aliens.Where(x => MutagenDefOf.defaultMutagen.CanInfect(x));


            IEnumerable<MorphDef> morphs = DefDatabase<MorphDef>.AllDefs.OrderBy<MorphDef, string>(x => x.LabelCap, StringComparer.CurrentCulture);

            // Only include the existing options if they match current values (meaning they have not been patched by other mods)
            _selectedReplacements = morphs.ToDictionary(x => x, x =>
            {
                if (_settingsReference.TryGetValue(x.defName, out string raceDefName) && _patchedMorphs.Contains(x) == false)
                {
                    return x.ExplicitHybridRace as AlienRace.ThingDef_AlienRace;
                }

                return null;
            });
            _aliens = aliens.Except(_patchedMorphs.Select(x => x.ExplicitHybridRace as AlienRace.ThingDef_AlienRace));
            _morphs = new ListFilter<MorphDef>(morphs, (item, filterText) => item.LabelCap.ToString().ToLower().Contains(filterText));
        }


        public override void DoWindowContents(Rect inRect)
        {
            float curY = 0;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0, curY, inRect.width, Text.LineHeight), "PMRaceReplacementHeader".Translate());

            curY += Text.LineHeight;

            Text.Font = GameFont.Small;
            Rect descriptionRect = new Rect(0, curY, inRect.width, 60);
            Widgets.Label(descriptionRect, "PMRaceReplacementText".Translate());

            curY += descriptionRect.height;

            _searchText = Widgets.TextArea(new Rect(0, curY, 200f, 28f), _searchText);
            if (Widgets.ButtonText(new Rect(205, curY, 28, 28), "X"))
                _searchText = "";

            curY += 35;

            float totalHeight = inRect.height - curY - Math.Max(APPLY_BUTTON_SIZE.y, Math.Max(RESET_BUTTON_SIZE.y, CANCEL_BUTTON_SIZE.y)) - SPACER_SIZE;


            _morphs.Filter = _searchText.ToLower();
            Rect listbox = new Rect(0, 0, inRect.width - 20, (_morphs.Filtered.Count() + 1) * Text.LineHeight);
            Widgets.BeginScrollView(new Rect(0, curY, inRect.width, totalHeight), ref _scrollPosition, listbox);

            Text.Font = GameFont.Tiny;
            Listing_Standard lineListing = new Listing_Standard(listbox, () => _scrollPosition);
            lineListing.Begin(listbox);
            
            foreach (var morph in _morphs.Filtered)
            {
                if (_patchedMorphs.Contains(morph))
                {
                    lineListing.LabelDouble(morph.LabelCap, $"{morph.ExplicitHybridRace.LabelCap} ({morph.ExplicitHybridRace.modContentPack.Name})", "PMRaceReplacementLocked".Translate());
                    continue;
                }

                ThingDef alien = _selectedReplacements[morph];
                if (lineListing.ButtonTextLabeled(morph.LabelCap, alien == null ? "" : $"{alien.LabelCap} ({alien.modContentPack.Name})"))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();

                    if (alien != null)
                        options.Add(new FloatMenuOption(" ", () => _selectedReplacements[morph] = null));

                    foreach (var alienItem in _aliens.Except(_selectedReplacements.Values))
                    {
                        options.Add(new FloatMenuOption($"{alienItem.LabelCap} ({alienItem.modContentPack.Name})", () => _selectedReplacements[morph] = alienItem));
                    }

                    if (options.Count > 0)
                        Find.WindowStack.Add(new FloatMenu(options));
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
            Find.WindowStack.Add(new Dialog_Popup("PMRequiresRestart".Translate(), new Vector2(300, 100)));
            _settingsReference.Clear();
            _settingsReference.AddRange(_selectedReplacements.Where(x => x.Value != null).ToDictionary(x => x.Key.defName, x => x.Value.defName));
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
