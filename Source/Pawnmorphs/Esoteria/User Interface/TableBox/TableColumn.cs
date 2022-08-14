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

        public TableColumn(string caption, float width, RowCallback<Rect, T> callback = null)
           : base (caption, width)
        {
            Callback = callback;
        }
    }
}
