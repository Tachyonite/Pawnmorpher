using System;
using Pawnmorph.Utilities.Collections;
using UnityEngine;
using Verse;

namespace Pawnmorph.UserInterface
{
	/// <summary>
	/// Searchable list box.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class FilterListBox<T>
	{
		private Vector2 _scrollPosition = new Vector2(0, 0);
		private string _searchText = string.Empty;
		private ListFilter<T> _filteredList;

		public FilterListBox(ListFilter<T> collection)
		{
			_filteredList = collection;
		}

		/// <summary>
		/// Draws filter box.
		/// </summary>
		/// <param name="inRect">The parent rectangle.</param>
		/// <param name="x">X position.</param>
		/// <param name="y">Y position.</param>
		/// <param name="height">Filterbox height.</param>
		/// <param name="callback">The callback.</param>
		public void Draw(Rect inRect, float x, float y, float height, Action<T, Listing_Standard> callback)
		{
			float curY = y;

			float clearButtonSize = Text.LineHeight;
			_searchText = Widgets.TextArea(new Rect(x, curY, inRect.width - clearButtonSize - 10, clearButtonSize), _searchText);
			if (Widgets.ButtonText(new Rect(inRect.width - clearButtonSize - 5, curY, clearButtonSize, clearButtonSize), "X"))
				_searchText = "";
			_filteredList.Filter = _searchText;

			curY += clearButtonSize + 7;
			height -= clearButtonSize + 7;

			Text.Font = GameFont.Tiny;
			Listing_Standard lineListing = new Listing_Standard();
			float lineHeight = 30 + lineListing.verticalSpacing + lineListing.CurHeight;

			Rect listbox = new Rect(0, 0, inRect.width - 20, (_filteredList.Filtered.Count + 1) * lineHeight);
			Widgets.BeginScrollView(new Rect(x, curY, inRect.width, height), ref _scrollPosition, listbox);

			// Begin listcontrol and add empty gap for all the space above scrollbox.
			lineListing.Begin(listbox);
			lineListing.Gap(_scrollPosition.y);

			// Get index of first row visible in scrollbox
			int currentIndex = Mathf.FloorToInt(_scrollPosition.y / lineHeight);
			for (; currentIndex < _filteredList.Filtered.Count; currentIndex++)
			{
				callback(_filteredList.Filtered[currentIndex], lineListing);

				// Break if next row starts outside bottom of scrollbox + 1 row to ensure smooth scrolling - though this should possibly not be needed for IMGUI.
				if (lineListing.CurHeight > height + _scrollPosition.y + lineHeight)
					break;
			}
			lineListing.End();

			Widgets.EndScrollView();
		}
	}
}
