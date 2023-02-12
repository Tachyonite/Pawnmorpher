using System;
using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities.Collections;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Pawnmorph.UserInterface.Settings
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

		private Dictionary<AlienRace.ThingDef_AlienRace, bool> _selectedAliens;
		List<string> _settingsReference;


		FilterListBox<AlienRace.ThingDef_AlienRace> _aliensListBox;

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
			aliens = aliens.Except(Hybrids.RaceGenerator.ExplicitPatchedRaces.Select(x => x.ExplicitHybridRace).OfType<AlienRace.ThingDef_AlienRace>());
			aliens = aliens.Where(x => MutagenDefOf.defaultMutagen.CanInfect(x));

			_selectedAliens = aliens.Where(x => _settingsReference.Contains(x.defName)).ToDictionary(x => x, x => true);

			var filterList = new ListFilter<AlienRace.ThingDef_AlienRace>(aliens, (alien, filterText) => alien.LabelCap.ToString().ToLower().Contains(filterText));
			_aliensListBox = new FilterListBox<AlienRace.ThingDef_AlienRace>(filterList);
		}


		public override void DoWindowContents(Rect inRect)
		{
			float curY = 0;
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0, curY, inRect.width, Text.LineHeight), "PMEnableMutationVisualsHeader".Translate());

			curY += Text.LineHeight;

			Text.Font = GameFont.Small;
			Rect descriptionRect = new Rect(0, curY, inRect.width, 60);
			Widgets.Label(descriptionRect, "PMEnableMutationVisualsText".Translate());

			curY += descriptionRect.height;

			float totalHeight = inRect.height - curY - Math.Max(APPLY_BUTTON_SIZE.y, Math.Max(RESET_BUTTON_SIZE.y, CANCEL_BUTTON_SIZE.y));
			totalHeight -= 100;

			_aliensListBox.Draw(inRect, 0, curY, totalHeight, (item, listing) =>
			{
				bool current = _selectedAliens.TryGetValue(item, false);
				listing.CheckboxLabeled(item.LabelCap, ref current, item.modContentPack.ModMetaData.Name);
				_selectedAliens[item] = current;
			});

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
