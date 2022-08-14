using Pawnmorph.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;

namespace Pawnmorph.User_Interface.TableBox
{
    internal class Table<T> where T : ITableRow
    {
        private const float CELL_SPACING = 5;
        private const float ROW_SPACING = 3;

        private List<TableColumn<T>> _columns;
        private ListFilter<T> _rows;
        private string _searchText;
        private Vector2 _scrollPosition;

        public Table(Func<T, string, bool> filterCallback)
        {
            _columns = new List<TableColumn<T>>();
            _rows = new ListFilter<T>(filterCallback);
        }

        public TableColumn<T> AddColumn(string header, float width, RowCallback<Rect, T> callback = null)
        {
            TableColumn<T> column = new TableColumn<T>(header, width, callback);
            _columns.Add(column);
            return column;
        }

        public void AddRow(T item)
        {
            // Ensure new rows have every column in data cache to avoid needing to check for every single cell during rendering.
            foreach (TableColumn column in _columns)
            {
                if (item.RowData.ContainsKey(column) == false)
                    item.RowData[column] = String.Empty;
            }

            _rows.Items.Add(item);
        }

        public void Refresh()
        {
            _rows.Invalidate();
        }

        public void Clear()
        {
            _rows.Items.Clear();
            _columns.Clear();
        }

        public void Draw(Rect boundingBox)
        {
            Rect searchBox = new Rect(boundingBox.x, boundingBox.y, boundingBox.width - 28f - CELL_SPACING, 28f);
            _searchText = Widgets.TextArea(searchBox, _searchText);
            if (Widgets.ButtonText(new Rect(searchBox.width + CELL_SPACING, boundingBox.y, 28, 28), "X"))
                _searchText = "";
            _rows.Filter = _searchText;

            boundingBox.y += 35;
            boundingBox.height -= 35;


            TableColumn<T> column;
            Rect columnHeader = new Rect(boundingBox);

            float tableWidth = boundingBox.width - GenUI.ScrollBarWidth - CELL_SPACING;
            for (int i = 0; i < _columns.Count; i++)
            {
                column = _columns[i];
                if (column.IsFixedWidth)
                {
                    columnHeader.width = column.Width;
                }
                else
                    columnHeader.width = tableWidth * column.Width;

                Widgets.Label(columnHeader, column.Caption);
                columnHeader.x = columnHeader.xMax + CELL_SPACING;
            }

            boundingBox.y += Text.LineHeight + ROW_SPACING;
            boundingBox.height -= Text.LineHeight + ROW_SPACING;

            Text.Font = GameFont.Tiny;


            Rect rowRect = new Rect(0, 0, tableWidth, Text.LineHeight + ROW_SPACING);
            Rect listbox = new Rect(0, 0, tableWidth, (_rows.Filtered.Count + 1) * rowRect.height);

            Widgets.BeginScrollView(boundingBox, ref _scrollPosition, listbox);
            rowRect.y = _scrollPosition.y;

            // Get index of first row visible in scrollbox
            int currentIndex = Mathf.FloorToInt(_scrollPosition.y / rowRect.height);
            for (; currentIndex < _rows.Filtered.Count; currentIndex++)
            {
                rowRect.x = 0;
                for (int i = 0; i < _columns.Count; i++)
                {
                    column = _columns[i];
                    if (column.IsFixedWidth)
                    {
                        rowRect.width = column.Width;
                    }
                    else
                        rowRect.width = tableWidth * column.Width;

                    if (column.Callback != null)
                        column.Callback(ref rowRect, _rows.Filtered[currentIndex]);
                    else
                        Widgets.Label(rowRect, _rows.Filtered[currentIndex].RowData[column]);

                    rowRect.x = rowRect.xMax + CELL_SPACING;
                }
                rowRect.y += rowRect.height;

                //
                //Widgets.DrawLine(new Vector2(viewRect.x, viewRect.y), new Vector2(viewRect.x + mainView.width, viewRect.y),
                //                 Color.black, lineWidth);

                // Break if next row starts outside bottom of scrollbox + 1 row to ensure smooth scrolling - though this should possibly not be needed for IMGUI.
                if (rowRect.y > boundingBox.height + _scrollPosition.y + rowRect.height)
                    break;
            }

            Widgets.EndScrollView();
        }
    }
}
