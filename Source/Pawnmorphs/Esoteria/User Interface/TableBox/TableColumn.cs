using Pawnmorph.Utilities.Collections;
using RimWorld;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Pawnmorph.User_Interface.TableBox
{
    internal delegate void RowCallback<Rect, T>(ref Rect boundingBox, T item);

    abstract class TableColumn
    {
        public string Caption { get; }
        public float Width { get; }
        public bool IsFixedWidth { get; set; }
        public TableColumn(string caption, float width)
        {
            Width = width;
            Caption = caption;
            IsFixedWidth = true;
        }
    }



    internal class TableColumn<T> : TableColumn
    {
        public RowCallback<Rect, T> Callback { get; }
        public Action<ListFilter<T>, bool> OrderByCallback { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableColumn{T}"/> class.
        /// </summary>
        /// <param name="caption">The column's title.</param>
        /// <param name="width">The width of the column. Use 0.xf for percentage/fractional widths.</param>
        /// <param name="orderByCallback">Callback for ordering by this column. Arguments are the collection to apply ordering to and if current ordering is ascending. Null if not sortable.</param>
        public TableColumn(string caption, float width, Action<ListFilter<T>, bool> orderByCallback = null)
           : base(caption, width)
        {
            Callback = null;
            OrderByCallback = orderByCallback;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableColumn{T}"/> class.
        /// </summary>
        /// <param name="caption">The column's title.</param>
        /// <param name="width">The width of the column. Use 0.xf for percentage/fractional widths.</param>
        /// <param name="callback">The rendering callback when a cell of this column should be rendered.</param>
        /// <param name="orderByCallback">Callback for ordering by this column. Arguments are the collection to apply ordering to and if current ordering is ascending. Null if not sortable.</param>
        public TableColumn(string caption, float width, RowCallback<Rect, T> callback, Action<ListFilter<T>, bool> orderByCallback)
           : base (caption, width)
        {
            Callback = callback;
            OrderByCallback = orderByCallback;
        }
    }
}
