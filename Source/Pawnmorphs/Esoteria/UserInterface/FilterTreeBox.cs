using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Unix.Native;
using Pawnmorph.UserInterface.TreeBox;
using Pawnmorph.Utilities.Collections;
using TMPro;
using UnityEngine;
using Verse;

namespace Pawnmorph.UserInterface
{
	/// <summary>
	/// Searchable list box.
	/// </summary>
	internal class FilterTreeBox
	{
		private Vector2 _scrollPosition = new Vector2(0, 0);
		private string _currentFilter = string.Empty;
		private List<TreeNode_FilterBox> _roots;
		private List<TreeNode_FilterBox> _items;

		public FilterTreeBox(List<TreeNode_FilterBox> roots)
		{
			_roots = roots;
			_items = new List<TreeNode_FilterBox>();
			UpdateFilter();
		}


		private void UpdateFilter()
		{
			_items.Clear();
			// Reset tree.
			if (_currentFilter.Length == 0)
			{
				for (int i = _roots.Count - 1; i >= 0; i--)
				{
					_roots[i].SetVisibility(true, true);
					_roots[i].GetVisibleNodes(_items);
				}
			}
			else
			{
				for (int i = _roots.Count - 1; i >= 0; i--)
				{
					_roots[i].SetVisibility(false, false);
					TreeSearch(_roots[i]);
					_roots[i].GetVisibleNodes(_items);
				}
			}
		}

		private void UpdateVisibleNodes()
		{
			_items.Clear();
			for (int i = _roots.Count - 1; i >= 0; i--)
			{
				_roots[i].GetVisibleNodes(_items);
			}
		}

		private void TreeSearch(TreeNode_FilterBox node)
		{
			if (node.Label.ToLower().Contains(_currentFilter))
			{
				// If node is match then all children will be visible too.
				node.SetVisibility(true, true);

				// Make parents visible.
				TreeNode_FilterBox parent = (TreeNode_FilterBox)node.parentNode;
				while (parent != null)
				{
					parent.SetVisibility(true, false);
					parent = (TreeNode_FilterBox)parent.parentNode;
				}
				return;
			}

			// Otherwise keep searching.
			for (int i = node.children.Count - 1; i >= 0; i--)
			{
				if (node.children[i] is TreeNode_FilterBox filterNode)
				{
					filterNode.SetVisibility(false, false);
					TreeSearch(filterNode);
				}
			}
		}

		/// <summary>
		/// Draws filter box.
		/// </summary>
		/// <param name="inRect">The parent rectangle.</param>
		public void Draw(Rect inRect)
		{
			float curY = inRect.y;
			float height = inRect.height;

			float clearButtonSize = Text.LineHeight;
			string searchText = Widgets.TextArea(new Rect(inRect.x, curY, inRect.width - clearButtonSize - 10, clearButtonSize), _currentFilter).ToLower().Trim();
			if (Widgets.ButtonText(new Rect(inRect.width - clearButtonSize - 5, curY, clearButtonSize, clearButtonSize), "X"))
				searchText = "";

			if (_currentFilter != searchText)
			{
				_currentFilter = searchText;
				UpdateFilter();
			}

			curY += clearButtonSize + 7;
			height -= clearButtonSize + 7;

			Text.Font = GameFont.Small;
			Listing_TreeFilter lineListing = new Listing_TreeFilter();
			float lineHeight = lineListing.lineHeight + lineListing.verticalSpacing;

			int count = _items.Count;
			Rect listbox = new Rect(0, 0, inRect.width - 20, (count + 1) * lineHeight);
			Widgets.BeginScrollView(new Rect(inRect.x, curY, inRect.width, height), ref _scrollPosition, listbox);

			// Begin listcontrol and add empty gap for all the space above scrollbox.
			lineListing.Begin(listbox);
			lineListing.Gap(_scrollPosition.y);

			// Get index of first row visible in scrollbox
			int currentIndex = Mathf.FloorToInt(_scrollPosition.y / lineHeight);
			for (; currentIndex < count; currentIndex++)
			{
				TreeNode_FilterBox node = _items[currentIndex];
				if (lineListing.Node(node, node.nestDepth, false))
				{
					UpdateVisibleNodes();
					break;
				}

				// Break if next row starts outside bottom of scrollbox + 1 row to ensure smooth scrolling - though this should possibly not be needed for IMGUI.
				if (lineListing.CurHeight > height + _scrollPosition.y + lineHeight)
					break;
			}
			lineListing.End();
			Widgets.EndScrollView();
		}
	}
}
