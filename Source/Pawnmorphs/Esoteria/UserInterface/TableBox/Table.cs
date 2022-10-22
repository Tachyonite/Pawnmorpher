using Pawnmorph.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;

namespace Pawnmorph.UserInterface.TableBox
{
    internal class Table<T> where T : ITableRow
    {
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

        /// <summary>
        /// Returns the current selected rows.
        /// </summary>
        public IReadOnlyList<T> SelectedRows => _selectedRows;

        /// <summary>
        /// Triggered when selected rows is changed.
        /// </summary>
        public event EventHandler<IReadOnlyList<T>> SelectionChanged;

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
            Rect searchBox = new Rect(boundingBox.x, boundingBox.y, boundingBox.width - 28f - CELL_SPACING, 28f);
            _searchText = Widgets.TextField(searchBox, _searchText);
            if (Widgets.ButtonText(new Rect(boundingBox.xMax - 28f, boundingBox.y, 28, 28), "X"))
                _searchText = "";
            
            searchBox.width = 70f;
            if (_searchText == String.Empty)
                Widgets.NoneLabelCenteredVertically(searchBox, "Search...");
            
            _rows.Filter = _searchText;
            
            boundingBox.y += 35;
            boundingBox.height -= 35;


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

            Text.Font = GameFont.Tiny;


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
                    if (Event.current.control == false)
                        _selectedRows.Clear();
                    
                    _selectedRows.Add(currentRow);

                    SelectionChanged?.Invoke(this, _selectedRows);
                    //if (Input.GetKey(KeyCode.LeftShift) == false)
                    //{

                    //}
                    //else
                    //{
                    //    if (Input.GetKey(KeyCode.LeftControl) == false)
                    //        _selectedRows.Clear();

                    //    _selectedRows.Add(currentRow);
                    //}
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
