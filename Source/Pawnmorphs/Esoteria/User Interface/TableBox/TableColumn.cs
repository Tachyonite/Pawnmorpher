using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Pawnmorph.User_Interface.TableBox
{
    internal delegate void RowCallback<Rect, T>(ref Rect boundingBox, T item);
    internal class TableColumn<T>
    {

        public string Caption { get; }
        public float Width { get; }
        public bool IsFixedWidth { get; set; }
        public RowCallback<Rect, T> Callback { get; }

        public TableColumn(string caption, float width, RowCallback<Rect, T> callback)
        {
            Callback = callback;
            Width = width;
            Caption = caption;
            IsFixedWidth = true;
        }
    }
}
