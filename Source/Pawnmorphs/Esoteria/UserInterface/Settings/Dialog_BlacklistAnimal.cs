using System;
using System.Collections.Generic;
using System.Linq;
using Pawnmorph.FormerHumans;
using Pawnmorph.Utilities.Collections;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Pawnmorph.UserInterface.Settings
{
	internal class Dialog_BlacklistAnimal : Window
	{
		private static readonly string HEADER_TEXT = "PMBlacklistFormerHumansHeader".Translate();
		private static readonly string DESCRIPTION_TEXT = "PMBlacklistFormerHumansText".Translate();
		private static readonly string APPLY_BUTTON_TEXT = "ApplyButtonText".Translate();
		private static readonly string RESET_BUTTON_TEXT = "ResetButtonText".Translate();
		private static readonly string CANCEL_BUTTON_TEXT = "CancelButtonText".Translate();
		private static readonly string FAILED_PARSING_TEXT = "PMFailedToParse".Translate();
		private static readonly Vector2 APPLY_BUTTON_SIZE = new Vector2(120f, 40f);
		private static readonly Vector2 RESET_BUTTON_SIZE = new Vector2(120f, 40f);
		private static readonly Vector2 CANCEL_BUTTON_SIZE = new Vector2(120f, 40f);
		private static readonly Vector2 RAW_BUTTON_SIZE = new Vector2(28, 28f);
		private const float SPACER_SIZE = 8f;

		private static readonly Dictionary<FormerHumanRestrictions, string> OPTIONS = new Dictionary<FormerHumanRestrictions, string>()
		{
			[FormerHumanRestrictions.Enabled] = "PMBlacklistFormerHumansEnabled".Translate(),
			[FormerHumanRestrictions.Restricted] = "PMBlacklistFormerHumansRestricted".Translate(),
			[FormerHumanRestrictions.Disabled] = "PMBlacklistFormerHumansDisabled".Translate(),
		};

		private static readonly Dictionary<FormerHumanRestrictions, string> OPTIONS_DESCRIPTIONS = new Dictionary<FormerHumanRestrictions, string>()
		{
			[FormerHumanRestrictions.Enabled] = "PMBlacklistFormerHumansEnabledDescription".Translate(),
			[FormerHumanRestrictions.Restricted] = "PMBlacklistFormerHumansRestrictedDescription".Translate(),
			[FormerHumanRestrictions.Disabled] = "PMBlacklistFormerHumansDisabledDescription".Translate(),
		};


		private Dictionary<ThingDef, FormerHumanRestrictions> _canBeFormerHumanDictonary;
		Dictionary<string, FormerHumanRestrictions> _settingsReference;
		FilterListBox<ThingDef> _animalListBox;
		HashSet<ThingDef> _morphAnimals;



		public Dialog_BlacklistAnimal(Dictionary<string, FormerHumanRestrictions> settingsReference)
		{
			_settingsReference = settingsReference;
			_canBeFormerHumanDictonary = new Dictionary<ThingDef, FormerHumanRestrictions>();


			this.resizeable = true;
			this.draggable = true;
		}

		public override void PostOpen()
		{
			base.PostOpen();
			RefreshList();
		}

		private void RefreshList()
		{
			// Get all animal ThingDefs that are not dryads or associated with a morph.
			IEnumerable<ThingDef> animals = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsValidFormerHuman(true, true));
			_canBeFormerHumanDictonary = animals.ToDictionary(x => x, x => _settingsReference.TryGetValue(x.defName, FormerHumanRestrictions.Enabled));


			_morphAnimals = MorphDef.AllDefs.SelectMany(x => x.AllAssociatedAnimals).ToHashSet();

			var filterList = new ListFilter<ThingDef>(animals,
				(animal, filterText) => animal.LabelCap.ToString().ToLower().Contains(filterText) ||
				OPTIONS[_canBeFormerHumanDictonary[animal]].ToLower().Contains(filterText));
			_animalListBox = new FilterListBox<ThingDef>(filterList);
		}


		public override void DoWindowContents(Rect inRect)
		{
			float curY = 0;
			Text.Font = GameFont.Medium;
			Widgets.Label(0, ref curY, inRect.width, HEADER_TEXT);

			Text.Font = GameFont.Small;
			Widgets.Label(0, ref curY, inRect.width, DESCRIPTION_TEXT);
			curY += SPACER_SIZE;

			float totalHeight = inRect.height - curY - Math.Max(APPLY_BUTTON_SIZE.y, Math.Max(RESET_BUTTON_SIZE.y, CANCEL_BUTTON_SIZE.y)) - SPACER_SIZE;
			_animalListBox.Draw(inRect, 0, curY, totalHeight, (item, listing) =>
			{
				FormerHumanRestrictions current = _canBeFormerHumanDictonary[item];

				string tooltip = "";
				string source = item.modContentPack?.ModMetaData?.Name;
				if (String.IsNullOrWhiteSpace(source) == false)
				{
					tooltip += "PMBlacklistFormerHumansAddedBy".Translate(source);
					tooltip += Environment.NewLine;
					tooltip += Environment.NewLine;
				}
				tooltip += OPTIONS_DESCRIPTIONS[current];

				if (listing.ButtonTextLabeled(item.LabelCap, OPTIONS[current], tooltip: tooltip))
				{
					List<FloatMenuOption> options = new List<FloatMenuOption>();

					foreach (KeyValuePair<FormerHumanRestrictions, string> option in OPTIONS)
					{
						if (option.Key == FormerHumanRestrictions.Disabled && _morphAnimals.Contains(item))
							continue;

						options.Add(new FloatMenuOption(option.Value, () => _canBeFormerHumanDictonary[item] = option.Key));
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
			Dictionary<string, FormerHumanRestrictions> currentValue = _canBeFormerHumanDictonary.Where(x => x.Value != FormerHumanRestrictions.Enabled).ToDictionary(x => x.Key.defName, x => x.Value);
			string stringValue = string.Join(",", currentValue);
			Dialog_Textbox textBox = new Dialog_Textbox(stringValue, true, new Vector2(300, 100));

			textBox.ApplyAction = (value) =>
			{
				try
				{
					string[] items = value.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);

					foreach (var item in items.Select(x => x.Split(',')).Where(x => x[1].Length > 0).ToDictionary(y => y[0], y => y[1]))
					{
						ThingDef key = _canBeFormerHumanDictonary.Keys.SingleOrDefault(x => x.defName == item.Key);
						if (key != null)
							_canBeFormerHumanDictonary[key] = (FormerHumanRestrictions)Enum.Parse(typeof(FormerHumanRestrictions), item.Value);
					}
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
			var blacklist = _canBeFormerHumanDictonary.Where(x => x.Value != FormerHumanRestrictions.Enabled);
			var whitelist = _canBeFormerHumanDictonary.Where(x => x.Value == FormerHumanRestrictions.Enabled);

			// Ensure existing blacklist entries are only removed if they are explicitly un-blacklisted
			// That way defs that may not exist (i.e. because they're from an unloaded mod) will be persisted
			foreach (var item in whitelist)
				_settingsReference.Remove(item.Key.defName);

			foreach (var item in blacklist)
				_settingsReference[item.Key.defName] = item.Value;

			FormerHumanUtilities.CacheValidFormerHumans();
			Need_Control.InvalidateRaceCache();
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
