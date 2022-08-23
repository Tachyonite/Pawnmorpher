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

        /// <summary>
        /// The very first method to be called. Only called once.
        /// </summary>
        /// <param name="databank">The databank.</param>
        public abstract void Initialize(ChamberDatabase databank);

        /// <summary>
        /// Called to populate <see cref="TableBox.Table{GeneRowItem}"/>. Only called once.
        /// </summary>
        /// <param name="table">The table to be populated.</param>
        public abstract void GenerateTable(TableBox.Table<GeneRowItem> table);

        /// <summary>
        /// Called when the selected rows have changed.
        /// </summary>
        /// <param name="selectedRows">The selected rows.</param>
        public abstract void SelectionChanged(IReadOnlyList<GeneRowItem> selectedRows);

        /// <summary>
        /// Called every frame to draw details section.
        /// </summary>
        /// <param name="inRect">The details section bounding box.</param>
        public abstract void DrawDetails(Rect inRect);

        /// <summary>
        /// Called when user clicks the delete button.
        /// </summary>
        /// <param name="def">The def to be deleted.</param>
        public abstract void Delete(Def def);
    }
}
