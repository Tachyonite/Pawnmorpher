using Pawnmorph.Chambers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Pawnmorph.User_Interface.Genebank
{
    abstract class GenebankTab
    {
        protected const float SPACING = 10f;
        protected const float PREVIEW_SIZE = 200f;

        public abstract void Initialize(ChamberDatabase databank);

        public abstract void GenerateTable(TableBox.Table<GeneRowItem> table);

        public abstract void SelectionChanged(IReadOnlyList<GeneRowItem> selectedRows);

        public abstract void DrawDetails(Rect inRect);

        public abstract void Delete(Def def);
    }
}
