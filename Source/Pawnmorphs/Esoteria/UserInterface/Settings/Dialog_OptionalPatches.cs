using System;
using System.Collections.Generic;
using System.Linq;
using Pawnmorph.HPatches.Optional;
using Pawnmorph.Utilities.Collections;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Pawnmorph.UserInterface.Settings
{
	internal class Dialog_OptionalPatches : Window
	{
		private const string APPLY_BUTTON_LOC_STRING = "ApplyButtonText";
		private const string RESET_BUTTON_LOC_STRING = "ResetButtonText";
		private const string CANCEL_BUTTON_LOC_STRING = "CancelButtonText";
		private static Vector2 APPLY_BUTTON_SIZE = new Vector2(120f, 40f);
		private static Vector2 RESET_BUTTON_SIZE = new Vector2(120f, 40f);
		private static Vector2 CANCEL_BUTTON_SIZE = new Vector2(120f, 40f);
		private const float SPACER_SIZE = 17f;


		FilterListBox<(Type, OptionalPatchAttribute)> _patchListBox;
		List<Type> _validTypes;

		Dictionary<string, bool> _settingsReference;
		Dictionary<string, bool> _settingsReferenceSession;

		public Dialog_OptionalPatches(Dictionary<string, bool> settingsReference)
		{
			_settingsReference = settingsReference;
			_settingsReferenceSession = new Dictionary<string, bool>();
			ResetSession();
		}

		private void ResetSession()
		{
			_settingsReferenceSession.Clear();
			_settingsReferenceSession.AddRange(_settingsReference);
		}

		public override void PostOpen()
		{
			base.PostOpen();

			// Discover all optional patches.
			List<(Type, OptionalPatchAttribute)> types = new List<(Type, OptionalPatchAttribute)>(50);
			foreach (Type type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
			{
				OptionalPatchAttribute attribute = type.GetCustomAttributes(typeof(OptionalPatchAttribute), false).FirstOrDefault() as OptionalPatchAttribute;
				if (attribute != null)
				{
					types.Add((type, attribute));
				}
			}

			_validTypes = new List<Type>(types.Select(x => x.Item1));
			var list = new ListFilter<(Type, OptionalPatchAttribute)>(types.OrderBy(x => x.Item2.Caption, StringComparer.CurrentCulture), (item, filterText) => item.Item2.Caption.ToLower().Contains(filterText));
			_patchListBox = new FilterListBox<(Type, OptionalPatchAttribute)>(list);
		}


		public override void DoWindowContents(Rect inRect)
		{
			float curY = 0;
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0, curY, inRect.width, Text.LineHeight), "PMOptionalPatchesHeader".Translate());

			curY += Text.LineHeight;

			Text.Font = GameFont.Small;
			Rect descriptionRect = new Rect(0, curY, inRect.width, 60);
			Widgets.Label(descriptionRect, "PMOptionalPatchesText".Translate());

			curY += descriptionRect.height;

			float totalHeight = inRect.height - curY - Math.Max(APPLY_BUTTON_SIZE.y, Math.Max(RESET_BUTTON_SIZE.y, CANCEL_BUTTON_SIZE.y));
			_patchListBox.Draw(inRect, 0, curY, totalHeight, (item, listing) =>
			{
				bool current = _settingsReferenceSession.TryGetValue(item.Item1.FullName, item.Item2.DefaultEnabled);
				listing.CheckboxLabeled(item.Item2.Caption, ref current, item.Item2.Description);
				_settingsReferenceSession[item.Item1.FullName] = current;
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
				ResetSession();
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

			for (int i = 0; i < _validTypes.Count; i++)
			{
				string type = _validTypes[i].FullName;
				_settingsReference[type] = _settingsReferenceSession[type];
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
