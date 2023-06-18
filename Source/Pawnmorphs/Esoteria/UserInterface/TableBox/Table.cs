using System;
using System.Collections.Generic;
using Pawnmorph.Utilities.Collections;
using UnityEngine;
using Verse;

namespace Pawnmorph.UserInterface.TableBox
{
	internal class Table<T> where T : ITableRow
	{
		private readonly string SEARCH_PLACEHOLDER = "TableControlSearchPlaceholder".Translate();
		private readonly float SEARCH_PLACEHOLDER_SIZE;


		public Table()
		{
			Text.Font = GameFont.Small;
			SEARCH_PLACEHOLDER_SIZE = Text.CalcSize(SEARCH_PLACEHOLDER).x;
		}


		private const float CELL_SPACING = 5;
		private const float ROW_SPACING = 3;

		private List<TableColumn<T>> _columns;
		private ListFilter<T> _rows;
		private string _searchText;
		private Vector2 _scrollPosition;
		private bool _ascendingOrder;
		private TableColumn _currentOrderColumn;
		private List<T> _selectedRows;
		private float _fixedColumnWidth;
		private float _dynamicColumnWidth;
		private GameFont _lineFont;
		private bool _multiSelect;

		/// <summary>
		/// Gets or sets the row filter function. Return true to include row and fall to skip.
		/// </summary>
		public Func<T, bool> RowFilter { get; set; }


        /// <summary>
        /// Returns the current selected rows.
        /// </summary>
        public IReadOnlyList<T> SelectedRows => _selectedRows;

		/// <summary>
		/// Triggered when selected rows is changed.
		/// </summary>
		public event EventHandler<IReadOnlyList<T>> SelectionChanged;

		/// <summary>
		/// Gets or sets the line font.
		/// </summary>
		/// <value>
		/// The line font.
		/// </value>
		public GameFont LineFont
		{
			get => _lineFont;
			set => _lineFont = value;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Table{T}"/> allows selecting multiple rows.
		/// </summary>
		/// <value>
		///   <c>true</c> if multi-select is enabled; otherwise, <c>false</c>.
		/// </value>
		public bool MultiSelect
		{
			get => _multiSelect;
			set => _multiSelect = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Table{T}" /> class.
		/// </summary>
		/// <param name="filterCallback">Callback used to apply filter text.</param>
		public Table(Func<T, string, bool> filterCallback)
		{
			_columns = new List<TableColumn<T>>();
			_rows = new ListFilter<T>(filterCallback);
			_ascendingOrder = false;
			_selectedRows = new List<T>(20);
		}


		/// <summary>Adds the column.</summary>
		/// <param name="header">Title of the column.</param>
		/// <param name="width">Column width.</param>
		/// <param name="orderByCallback">Optional callback to tell the column how to order rows.</param>
		public TableColumn<T> AddColumn(string header, float width, Action<ListFilter<T>, bool, TableColumn> orderByCallback = null)
		{
			TableColumn<T> column = new TableColumn<T>(header, width, orderByCallback);
			_columns.Add(column);
			return column;
		}

		/// <summary>
		/// Adds the column.
		/// </summary>
		/// <param name="header">Column caption.</param>
		/// <param name="width">Column width.</param>
		/// <param name="callback">Render callback when cell in column is drawn.</param>
		/// <param name="orderByCallback">Optional callback to tell the column how to order rows.</param>
		/// <returns></returns>
		public TableColumn<T> AddColumn(string header, float width, RowCallback<Rect, T> callback, Action<ListFilter<T>, bool, TableColumn> orderByCallback = null)
		{
			TableColumn<T> column = new TableColumn<T>(header, width, callback, orderByCallback);
			_columns.Add(column);
			return column;
		}

        /// <summary>
        /// Add a new row to the table.
        /// </summary>
        /// <param name="item">Row to add.</param>
        public void AddRow(T item)
        {
            if (RowFilter != null && RowFilter(item) == false)
                return;

			// Ensure new rows have every column in data cache to avoid needing to check for every single cell during rendering.
			foreach (TableColumn column in _columns)
            {
                if (item.HasColumn(column) == false)
                    item[column] = String.Empty;
            }

			_rows.Items.Add(item);
		}

		/// <summary>
		/// Remove specific row from table.
		/// </summary>
		/// <param name="item">Row to remove.</param>
		public void DeleteRow(T item)
		{
			_rows.Items.Remove(item);
		}

		/// <summary>
		/// Invalidate rows and recalculates columns.
		/// </summary>
		public void Refresh()
		{
			_rows.Invalidate();

			_fixedColumnWidth = 0;
			_dynamicColumnWidth = 0;
			TableColumn column;
			for (int i = 0; i < _columns.Count; i++)
			{
				column = _columns[i];

				if (column.IsFixedWidth)
					_fixedColumnWidth += column.Width;
				else
					_dynamicColumnWidth += column.Width;
			}
		}

		/// <summary>
		/// Clears all rows and columns from table.
		/// </summary>
		public void Clear()
		{
			_rows.Items.Clear();
			_columns.Clear();
		}

		private void Sort(TableColumn<T> column)
		{
			if (column.Callback != null && column.OrderByCallback == null)
				return;

			// If current sorting is ascending or new column is clicked.
			if (_ascendingOrder == false || _currentOrderColumn != column)
			{
				_ascendingOrder = true;
				if (column.OrderByCallback == null)
					_rows.OrderBy(x => x[column]);
				else
					column.OrderByCallback(_rows, _ascendingOrder, column);
				_currentOrderColumn = column;
			}
			else
			{
				_ascendingOrder = false;
				if (column.OrderByCallback == null)
					_rows.OrderByDescending(x => x[column]);
				else
					column.OrderByCallback(_rows, _ascendingOrder, column);
			}
		}

		/// <summary>
		/// Draw table to bounding box.
		/// </summary>
		/// <param name="boundingBox">Decides the size and position of the table.</param>
		public void Draw(Rect boundingBox)
		{
			Text.Font = GameFont.Small;

			float clearButtonSize = Text.LineHeight;
			Rect searchBox = new Rect(boundingBox.x, boundingBox.y, boundingBox.width - clearButtonSize - CELL_SPACING, clearButtonSize);
			_searchText = Widgets.TextField(searchBox, _searchText);
			if (Widgets.ButtonText(new Rect(boundingBox.xMax - clearButtonSize, boundingBox.y, clearButtonSize, clearButtonSize), "X"))
				_searchText = "";


			if (_searchText == String.Empty)
				Widgets.NoneLabelCenteredVertically(new Rect(searchBox.x + CELL_SPACING, searchBox.y, SEARCH_PLACEHOLDER_SIZE, Text.LineHeight), SEARCH_PLACEHOLDER);

			_rows.Filter = _searchText;

			boundingBox.y += clearButtonSize + 7;
			boundingBox.height -= clearButtonSize + 7;


			TableColumn<T> column;
			Rect columnHeader = new Rect(boundingBox);
			columnHeader.height = Text.LineHeight;

			float tableWidth = boundingBox.width - GenUI.ScrollBarWidth - CELL_SPACING;
			float leftoverWidth = tableWidth - _fixedColumnWidth - (CELL_SPACING * _columns.Count);
			for (int i = 0; i < _columns.Count; i++)
			{
				column = _columns[i];
				if (column.IsFixedWidth)
				{
					columnHeader.width = column.Width;
				}
				else
					columnHeader.width = column.Width / _dynamicColumnWidth * leftoverWidth;

				if (column.Callback == null || column.OrderByCallback != null)
					Widgets.DrawHighlightIfMouseover(columnHeader);

				Widgets.Label(columnHeader, column.Caption);
				if (Widgets.ButtonInvisible(columnHeader, true))
					Sort(column);

				columnHeader.x = columnHeader.xMax + CELL_SPACING;
			}

			boundingBox.y += Text.LineHeight + ROW_SPACING;
			boundingBox.height -= Text.LineHeight + ROW_SPACING;

			Text.Font = _lineFont;


			Rect rowRect = new Rect(0, 0, tableWidth, Text.LineHeight + ROW_SPACING);
			Rect listbox = new Rect(0, 0, tableWidth, (_rows.Filtered.Count + 1) * rowRect.height);

			Widgets.BeginScrollView(boundingBox, ref _scrollPosition, listbox);
			rowRect.y = _scrollPosition.y;

			T currentRow;
			// Get index of first row visible in scrollbox
			int currentIndex = Mathf.FloorToInt(_scrollPosition.y / rowRect.height);
			for (; currentIndex < _rows.Filtered.Count; currentIndex++)
			{
				currentRow = _rows.Filtered[currentIndex];
				rowRect.x = 0;
				for (int i = 0; i < _columns.Count; i++)
				{
					column = _columns[i];
					if (column.IsFixedWidth)
					{
						rowRect.width = column.Width;
					}
					else
						rowRect.width = column.Width / _dynamicColumnWidth * leftoverWidth;

					if (column.Callback != null)
						column.Callback(ref rowRect, currentRow);
					else
						Widgets.Label(rowRect, currentRow[column]);


					rowRect.x = rowRect.xMax + CELL_SPACING;
				}

				rowRect.x = 0;
				rowRect.width = tableWidth;

				// Hightlight entire row if selected.
				if (_selectedRows.Contains(currentRow))
					Widgets.DrawHighlightSelected(rowRect);

				// Hightlight row if moused over.
				Widgets.DrawHighlightIfMouseover(rowRect);

				if (Widgets.ButtonInvisible(rowRect, false))
				{
					if (_multiSelect == false || Event.current.control == false)
						_selectedRows.Clear();

					_selectedRows.Add(currentRow);

					SelectionChanged?.Invoke(this, _selectedRows);
				}

				rowRect.y += rowRect.height;

				// Break if next row starts outside bottom of scrollbox + 1 row to ensure smooth scrolling - though this should possibly not be needed for IMGUI.
				if (rowRect.y > boundingBox.height + _scrollPosition.y + rowRect.height)
					break;
			}

			Widgets.EndScrollView();
		}

	}
}
