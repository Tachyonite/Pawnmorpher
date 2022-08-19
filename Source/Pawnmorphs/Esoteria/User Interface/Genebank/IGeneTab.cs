using Pawnmorph.Chambers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnmorph.User_Interface.Genebank
{
    interface IGeneTab
    {
        public abstract void GenerateTable(TableBox.Table<GeneRowItem> table, ChamberDatabase databank);

        public abstract void InitDetails(Preview.Preview[] previews);

        public abstract void SelectedRow(IReadOnlyList<GeneRowItem> selectedRows, Preview.Preview[] previews);
    }
}
