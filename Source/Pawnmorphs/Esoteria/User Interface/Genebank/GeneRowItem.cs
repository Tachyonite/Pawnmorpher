using Pawnmorph.Chambers;
using Pawnmorph.Hediffs;
using Pawnmorph.User_Interface.TableBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.User_Interface.Genebank
{
    internal class GeneRowItem : ITableRow
    {
        public readonly Def Def;
        public readonly string Label;
        public readonly string StorageSpaceUsed;
        public readonly string StorageSpaceUsedPercentage;
        public readonly int Size;
        public string SearchString;

        public Dictionary<TableColumn, string> RowData { get; }

        private GeneRowItem(Def def)
        {
            RowData = new Dictionary<TableColumn, string>();
            Label = def.LabelCap;
            Def = def;
            StorageSpaceUsedPercentage = "0%";
        }

        public GeneRowItem(MutationDef def, int totalCapacity, string searchString)
            : this(def)
        {
            SearchString = searchString.ToLower();
            Size = def.GetRequiredStorage();
            StorageSpaceUsed = DatabaseUtilities.GetStorageString(Size);
            if (totalCapacity > 0)
                StorageSpaceUsedPercentage = ((float)Size / totalCapacity).ToStringPercent();
        }

        public GeneRowItem(PawnKindDef def, int totalCapacity, string searchString)
            : this(def)
        {
            SearchString = searchString.ToLower();
            Size = def.GetRequiredStorage();
            StorageSpaceUsed = DatabaseUtilities.GetStorageString(Size);
            if (totalCapacity > 0)
                StorageSpaceUsedPercentage = ((float)Size / totalCapacity).ToStringPercent();
        }
    }
}
