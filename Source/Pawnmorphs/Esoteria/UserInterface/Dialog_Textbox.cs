using System;
using UnityEngine;
using Verse;

namespace Pawnmorph.UserInterface
{
	internal class Dialog_Textbox : Window
	{
		private string _message;
		private readonly bool _readonly;
		private static Vector2 CONTINUE_BUTTON_SIZE = new Vector2(120f, 40f);
		private Vector2 _initialSize;

		public Action<string> ApplyAction { get; set; }

		public Dialog_Textbox(string message, bool isReadonly, Vector2 size)
		{
			_message = message;
			_readonly = isReadonly;
			_initialSize = size;
			_initialSize.y += CONTINUE_BUTTON_SIZE.y;
			_initialSize.x = Math.Max(_initialSize.x, CONTINUE_BUTTON_SIZE.x);
			doCloseX = true;
			resizeable = true;
			draggable = true;
			absorbInputAroundWindow = true;
			closeOnClickedOutside = true;
		}

		public override Vector2 InitialSize => _initialSize;

		public override void DoWindowContents(Rect inRect)
		{
			Rect textboxRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - CONTINUE_BUTTON_SIZE.y - 10);

			Widgets.DrawBox(textboxRect);
			_message = Widgets.TextArea(textboxRect, _message, _readonly);

			float x = inRect.width / 2 - CONTINUE_BUTTON_SIZE.x / 2;
			float y = inRect.height - CONTINUE_BUTTON_SIZE.y - 5;
			if (Widgets.ButtonText(new Rect(new Vector2(x, y), CONTINUE_BUTTON_SIZE), "ApplyButtonText".Translate()))
			{
				if (ApplyAction != null)
					ApplyAction(_message);

				Close(true);
			}
		}
	}
}
