using System;
using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Chambers;
using Pawnmorph.DefExtensions;
using Pawnmorph.Utilities.Collections;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Pawnmorph.UserInterface.Settings
{
    internal class Dialog_BlacklistAnimal : Window
    {
        private static string HEADER_TEXT = "PMBlacklistFormerHumansHeader".Translate();
        private static string DESCRIPTION_TEXT = "PMBlacklistFormerHumansText".Translate();
		private static string APPLY_BUTTON_TEXT = "ApplyButtonText".Translate();
        private static string RESET_BUTTON_TEXT = "ResetButtonText".Translate();
        private static string CANCEL_BUTTON_TEXT = "CancelButtonText".Translate();
        private static string FAILED_PARSING_TEXT = "PMFailedToParse".Translate();
		private static Vector2 APPLY_BUTTON_SIZE = new Vector2(120f, 40f);
        private static Vector2 RESET_BUTTON_SIZE = new Vector2(120f, 40f);
        private static Vector2 CANCEL_BUTTON_SIZE = new Vector2(120f, 40f);
		private static Vector2 RAW_BUTTON_SIZE = new Vector2(28, 28f);
		private const float SPACER_SIZE = 17f;

        private Dictionary<ThingDef, bool> _blacklistedAnimalDictonary;
        List<string> _settingsReference;
        FilterListBox<ThingDef> _animalListBox;

        public Dialog_BlacklistAnimal(List<string> settingsReference)
        {
            _settingsReference = settingsReference;
            _blacklistedAnimalDictonary = new Dictionary<ThingDef, bool>();
        }

        public override void PostOpen()
        {
            base.PostOpen();
            RefreshList();
        }

        private void RefreshList()
        {
            // Get all animal ThingDefs that are not dryads.
            IEnumerable<ThingDef> animals = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.race?.Animal == true && x.race?.Dryad != true);
            animals = animals.Except(MorphDef.AllDefs.SelectMany(x => x.AllAssociatedAnimals));

            _blacklistedAnimalDictonary = animals.ToDictionary(x => x, x => _settingsReference.Contains(x.defName) == false);
			

			var filterList = new ListFilter<ThingDef>(animals, (animal, filterText) => animal.LabelCap.ToString().ToLower().Contains(filterText));
            _animalListBox = new FilterListBox<ThingDef>(filterList);
        }


        public override void DoWindowContents(Rect inRect)
        {
            float curY = 0;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0, curY, inRect.width, Text.LineHeight), HEADER_TEXT);

            curY += Text.LineHeight;

            Text.Font = GameFont.Small;
            Rect descriptionRect = new Rect(0, curY, inRect.width, 60);
            Widgets.Label(descriptionRect, DESCRIPTION_TEXT);

            curY += descriptionRect.height;
			curY += 35;

			float totalHeight = inRect.height - curY - Math.Max(APPLY_BUTTON_SIZE.y, Math.Max(RESET_BUTTON_SIZE.y, CANCEL_BUTTON_SIZE.y)) - SPACER_SIZE;
            _animalListBox.Draw(inRect, 0, curY, totalHeight, (item, listing) =>
            {
                bool current = _blacklistedAnimalDictonary.TryGetValue(item, false);
                listing.CheckboxLabeled(item.LabelCap, ref current, item.modContentPack.ModMetaData.Name);
                _blacklistedAnimalDictonary[item] = current;
            });

            // Draw the apply, reset and cancel buttons.
			if (Widgets.ButtonImage(new Rect(5, inRect.height - 33, RAW_BUTTON_SIZE.x, RAW_BUTTON_SIZE.y), TexButton.Rename))
			{
				ShowRawInput();
			}

            float buttonVertPos = inRect.height - Math.Max(APPLY_BUTTON_SIZE.y, Math.Max(RESET_BUTTON_SIZE.y, CANCEL_BUTTON_SIZE.y));
            float applyHorPos = inRect.width / 2 - APPLY_BUTTON_SIZE.x - RESET_BUTTON_SIZE.x / 2 - SPACER_SIZE;
            float resetHorPos = inRect.width / 2 - RESET_BUTTON_SIZE.x / 2;
            float cancelHorPos = inRect.width / 2 + RESET_BUTTON_SIZE.x / 2 + SPACER_SIZE;
			if (Widgets.ButtonText(new Rect(applyHorPos, buttonVertPos, APPLY_BUTTON_SIZE.x, APPLY_BUTTON_SIZE.y), APPLY_BUTTON_TEXT))
            {
                OnAcceptKeyPressed();
            }
            if (Widgets.ButtonText(new Rect(resetHorPos, buttonVertPos, RESET_BUTTON_SIZE.x, RESET_BUTTON_SIZE.y), RESET_BUTTON_TEXT))
            {
                RimWorld.SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
                RefreshList();
            }
            if (Widgets.ButtonText(new Rect(cancelHorPos, buttonVertPos, CANCEL_BUTTON_SIZE.x, CANCEL_BUTTON_SIZE.y), CANCEL_BUTTON_TEXT))
            {
                OnCancelKeyPressed();
            }
        }


		private void ShowRawInput()
		{
            List<string> exportCollection = new List<string>();
			exportCollection.AddRange(_blacklistedAnimalDictonary.Where(x => x.Value == false).Select(x => x.Key.defName).ToList());
			exportCollection = exportCollection.Distinct().ToList();

			string stringValue = string.Join(",", exportCollection);
			Dialog_Textbox textBox = new Dialog_Textbox(stringValue, true, new Vector2(300, 100));

			textBox.ApplyAction = (value) =>
			{
				try
				{
                    _settingsReference.Clear();
					_settingsReference.AddRange(value.Split(',').Where(x => String.IsNullOrWhiteSpace(x) == false).Distinct());
                    RefreshList();
				}
				catch (Exception)
				{
					Find.WindowStack.Add(new Dialog_Popup(FAILED_PARSING_TEXT, new Vector2(300, 100)));
				}
			};

			Find.WindowStack.Add(textBox);
		}

		private void ApplyChanges()
        {
            // We want to persist any definitions assigned for defnames that could not be loaded into the game at present time.
            // Like animals from currently disabled mods.
            // Take the old list, add all disabled animals (this will cause duplicates) and then apply distinct.
			_settingsReference.AddRange(_blacklistedAnimalDictonary.Where(x => x.Value == false).Select(x => x.Key.defName).ToList());
            _settingsReference = _settingsReference.Distinct().ToList();

            foreach (var animal in _blacklistedAnimalDictonary)
            {
                FormerHumanSettings formerHumanConfig = animal.Key.GetModExtension<FormerHumanSettings>();

                if (animal.Value == false)
				{
					if (formerHumanConfig == null)
					{
						formerHumanConfig = new FormerHumanSettings();
                        if (animal.Key.modExtensions == null)
                            animal.Key.modExtensions = new List<DefModExtension>();

						animal.Key.modExtensions.Add(formerHumanConfig);
					}

					formerHumanConfig.neverFormerHuman = true;
				} 
                else if (formerHumanConfig != null)
                {
                    // Enable animal as valid former human
                    formerHumanConfig.neverFormerHuman = false;
                }
			}
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
