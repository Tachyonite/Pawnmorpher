﻿using Pawnmorph.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Pawnmorph.User_Interface
{
    internal class FilterListBox<T>
    {
        private Vector2 _scrollPosition = new Vector2(0, 0);
        private string _searchText = string.Empty;
        private ListFilter<T> _filteredList;

        public FilterListBox(ListFilter<T> collection)
        {
            _filteredList = collection;
        }

        public void Draw(Rect inRect, float x, float y, float totalHeight, Action<T, Listing_Standard> callback)
        {
            float curY = y;

            _searchText = Widgets.TextArea(new Rect(0, curY, 200f, 28f), _searchText);
            if (Widgets.ButtonText(new Rect(205, curY, 28, 28), "X"))
                _searchText = "";

            curY += 35;
            totalHeight -= 35;

            _filteredList.Filter = _searchText.ToLower();
            Rect listbox = new Rect(0, 0, inRect.width - 20, (_filteredList.Filtered.Count() + 1) * Text.LineHeight);
            Widgets.BeginScrollView(new Rect(0, curY, inRect.width, totalHeight), ref _scrollPosition, listbox);

            Text.Font = GameFont.Tiny;
            Listing_Standard lineListing = new Listing_Standard(listbox, () => _scrollPosition);
            lineListing.Begin(listbox);

            foreach (T item in _filteredList.Filtered)
            {
                callback(item, lineListing);
            }

            lineListing.End();

            Widgets.EndScrollView();
        }
    }
}
