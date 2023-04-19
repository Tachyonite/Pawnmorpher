using System;
using UnityEngine;
using Verse;

namespace Pawnmorph.UserInterface
{
	internal class Dialog_Popup : Window
	{
		private readonly string _message;
		private static Vector2 CONTINUE_BUTTON_SIZE = new Vector2(120f, 40f);
		private Vector2 _initialSize;

		public Dialog_Popup(string message, Vector2 size)
		{
			_message = message;
			_initialSize = size;
			_initialSize.y += CONTINUE_BUTTON_SIZE.y;
			_initialSize.x = Math.Max(_initialSize.x, CONTINUE_BUTTON_SIZE.x);
		}

		public override Vector2 InitialSize => _initialSize;

		public override void DoWindowContents(Rect inRect)
		{
			Widgets.Label(inRect, _message);

			float x = inRect.width / 2 - CONTINUE_BUTTON_SIZE.x / 2;
			float y = inRect.height - CONTINUE_BUTTON_SIZE.y - 5;

			if (Widgets.ButtonText(new Rect(new Vector2(x, y), CONTINUE_BUTTON_SIZE), "Continue"))
				this.Close(true);
		}
	}
}
