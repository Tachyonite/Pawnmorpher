using System;
using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Chambers;
using Pawnmorph.Utilities.Collections;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Pawnmorph.UserInterface.Settings
{
	internal class Dialog_AnimalAssociations : Window
	{
		private const string APPLY_BUTTON_LOC_STRING = "ApplyButtonText";
		private const string RESET_BUTTON_LOC_STRING = "ResetButtonText";
		private const string CANCEL_BUTTON_LOC_STRING = "CancelButtonText";
		private static Vector2 APPLY_BUTTON_SIZE = new Vector2(120f, 40f);
		private static Vector2 RESET_BUTTON_SIZE = new Vector2(120f, 40f);
		private static Vector2 CANCEL_BUTTON_SIZE = new Vector2(120f, 40f);
		private static Vector2 RAW_BUTTON_SIZE = new Vector2(28, 28f);
		private const float SPACER_SIZE = 17f;


		private IEnumerable<ThingDef> _animals;
		private Dictionary<string, string> _settingsReference;
		private Dictionary<string, string> _settingsReferenceCache;
		private List<MorphDef> _morphs;


		FilterListBox<ThingDef> _animalListBox;

		public Dialog_AnimalAssociations(Dictionary<string, string> settingsReference)
		{
			_settingsReference = settingsReference;
			_settingsReferenceCache = settingsReference.ToDictionary(x => x.Key, x => x.Value);
			resizeable = true;
			draggable = true;
		}

		public override void PostOpen()
		{
			base.PostOpen();
			RefreshAliens();
		}

		private void RefreshAliens()
		{
			_settingsReferenceCache.Clear();
			_settingsReferenceCache.AddRange(_settingsReference);

			_morphs = DefDatabase<MorphDef>.AllDefsListForReading;

			List<ThingDef> currentAssociated = _morphs.SelectMany(x => x.AllAssociatedAnimals)
													  .Where(x => _settingsReferenceCache.ContainsKey(x.defName) == false)
													  .ToList();

			_animals = DefDatabase<ThingDef>.AllDefs.Where(x => x.IsValidAnimal() && currentAssociated.Contains(x) == false).ToList();

			var animalFilterList = new ListFilter<ThingDef>(_animals, (item, filterText) => (item.LabelCap.ToString() + item.modContentPack?.Name ?? "").ToLower().Contains(filterText));
			_animalListBox = new FilterListBox<ThingDef>(animalFilterList);
		}


		public override void DoWindowContents(Rect inRect)
		{
			float curY = 0;
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0, curY, inRect.width, Text.LineHeight), "PMAnimalAssociationsHeader".Translate());

			curY += Text.LineHeight;

			Text.Font = GameFont.Small;
			Rect descriptionRect = new Rect(0, curY, inRect.width, 60);
			Widgets.Label(descriptionRect, "PMAnimalAssociationsText".Translate());

			curY += descriptionRect.height;

			curY += 35;

			float totalHeight = inRect.height - curY - Math.Max(APPLY_BUTTON_SIZE.y, Math.Max(RESET_BUTTON_SIZE.y, CANCEL_BUTTON_SIZE.y)) - SPACER_SIZE;

			MorphDef morphDef = null;
			_animalListBox.Draw(inRect, 0, curY, totalHeight, (animal, listing) =>
			{
				morphDef = null;

				if (_settingsReferenceCache.TryGetValue(animal.defName, out string morph))
					morphDef = _morphs.FirstOrDefault(x => x.defName == morph);

				if (listing.ButtonTextLabeled($"{animal.LabelCap} ({animal.modContentPack?.Name})", morphDef?.LabelCap ?? morph))
				{
					List<FloatMenuOption> options = new List<FloatMenuOption>();

					// If something is already selected, add deselect option.
					if (morph != null)
						options.Add(new FloatMenuOption(" ", () => _settingsReferenceCache[animal.defName] = null));

					foreach (var morphItem in _morphs.Where(x => _settingsReferenceCache.Values.Contains(x.defName) == false))
					{
						options.Add(new FloatMenuOption($"{morphItem.LabelCap}", () => _settingsReferenceCache[animal.defName] = morphItem.defName));
					}

					if (options.Count > 0)
						Find.WindowStack.Add(new FloatMenu(options));
				}
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


		private void ShowRawInput()
		{
			Dictionary<string, string> currentValue = _settingsReferenceCache.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);
			string stringValue = string.Join(",", currentValue);
			Dialog_Textbox textBox = new Dialog_Textbox(stringValue, true, new Vector2(300, 100));

			textBox.ApplyAction = (value) =>
			{
				try
				{
					string[] items = value.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
					_settingsReferenceCache = items.Select(x => x.Split(',')).Where(x => x[1].Length > 0).ToDictionary(y => y[0], y => y[1]);
					RefreshAliens();
				}
				catch (Exception)
				{
					Find.WindowStack.Add(new Dialog_Popup("PMFailedToParse".Translate(), new Vector2(300, 100)));
				}
			};

			Find.WindowStack.Add(textBox);
		}




		private void ApplyChanges()
		{
			Find.WindowStack.Add(new Dialog_Popup("PMRequiresRestart".Translate(), new Vector2(300, 100)));
			_settingsReference.Clear();
			_settingsReference.AddRange(_settingsReferenceCache);
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
