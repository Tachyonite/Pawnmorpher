using Pawnmorph.Chambers;
using Pawnmorph.Hediffs;
using Pawnmorph.UserInterface.PartPicker;
using Pawnmorph.UserInterface.TableBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.UserInterface.Genebank
{
    internal class GeneRowItem : ITableRow
    {
        public readonly object Def;
        public readonly string Label;
        public readonly string StorageSpaceUsed;
        public readonly string StorageSpaceUsedPercentage;
        public readonly int Size;
        public string SearchString;

        private Dictionary<TableColumn, string> _rowData;


        public string this[TableColumn key]
        {
            get
            {
                return _rowData[key];
            }
            set
            {
                _rowData[key] = value;
            }
        }

        private GeneRowItem(object def)
        {
            _rowData = new Dictionary<TableColumn, string>();
            Def = def;
            StorageSpaceUsedPercentage = "0%";
        }

        public GeneRowItem(MutationDef def, int totalCapacity, string searchString)
            : this(def)
        {
            SearchString = searchString.ToLower();
			Label = def.LabelCap;
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
			Label = def.LabelCap;
			StorageSpaceUsed = DatabaseUtilities.GetStorageString(Size);
            if (totalCapacity > 0)
                StorageSpaceUsedPercentage = ((float)Size / totalCapacity).ToStringPercent();
		}

		public GeneRowItem(MutationTemplate def, int totalCapacity, string searchString)
			: this(def)
		{
			SearchString = searchString.ToLower();
			Label = def.Caption;
			Size = def.GenebankSize;
			StorageSpaceUsed = DatabaseUtilities.GetStorageString(Size);
			if (totalCapacity > 0)
				StorageSpaceUsedPercentage = ((float)Size / totalCapacity).ToStringPercent();
		}
        public bool HasColumn(TableColumn column)
        {
            return _rowData.ContainsKey(column);
        }
    }
}
