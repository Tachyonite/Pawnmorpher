using System.Collections.Generic;
using Pawnmorph.Chambers;
using Pawnmorph.Genebank.Model;
using Pawnmorph.UserInterface.TableBox;
using Verse;

namespace Pawnmorph.UserInterface.Genebank
{
	internal class GeneRowItem : ITableRow
	{
		public readonly IGenebankEntry Def;
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

		public GeneRowItem(IGenebankEntry def, int totalCapacity, string searchString)
		{
			_rowData = new Dictionary<TableColumn, string>();
			Def = def;

			SearchString = searchString.ToLower();
			Label = def.GetCaption();
			Size = def.GetRequiredStorage();
			StorageSpaceUsed = DatabaseUtilities.GetStorageString(Size);
			StorageSpaceUsedPercentage = "0%";
			if (totalCapacity > 0)
				StorageSpaceUsedPercentage = ((float)Size / totalCapacity).ToStringPercent();
		}

		public bool HasColumn(TableColumn column)
		{
			return _rowData.ContainsKey(column);
		}
	}
}
