using System;
using Pawnmorph.Utilities.Collections;
using UnityEngine;

namespace Pawnmorph.UserInterface.TableBox
{
	internal delegate void RowCallback<Rect, T>(ref Rect boundingBox, T item);

	abstract class TableColumn
	{
		/// <summary>
		/// Gets the title of the column.
		/// </summary>
		public string Caption { get; }

		/// <summary>
		/// Gets the width of the column. Use decimal value between 0 and 1 as percentage for dynamic width. <see cref="TableColumn.IsFixedWidth"/>
		/// </summary>
		public float Width { get; }

		/// <summary>
		/// Gets or sets whether this column has a fixed width.
		/// Set width as a value between 0 and 1 as a percentage of how much of the remaining width after all fixed columns are added this column should use.
		/// </summary>
		/// <value>
		///   <c>true</c> if this column is fixed width; otherwise, <c>false</c>.
		/// </value>
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
		public Action<ListFilter<T>, bool, TableColumn> OrderByCallback { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TableColumn{T}"/> class.
		/// </summary>
		/// <param name="caption">The column's title.</param>
		/// <param name="width">The width of the column. Use 0.xf for percentage/fractional widths.</param>
		/// <param name="orderByCallback">Callback for ordering by this column. Arguments are the collection to apply ordering to and if current ordering is ascending. Null if not sortable.</param>
		public TableColumn(string caption, float width, Action<ListFilter<T>, bool, TableColumn> orderByCallback = null)
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
		public TableColumn(string caption, float width, RowCallback<Rect, T> callback, Action<ListFilter<T>, bool, TableColumn> orderByCallback)
		   : base(caption, width)
		{
			Callback = callback;
			OrderByCallback = orderByCallback;
		}
	}
}
