using Pawnmorph.Chambers;
using Pawnmorph.Genebank.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.UserInterface.TableBox
{
	internal class TableRow<T> : ITableRow
	{
		private Dictionary<TableColumn, string> _rowData;
		public T RowObject { get; private set; }
		public object Tag;

        public string SearchString;

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

		public TableRow(T rowObject, string searchString = null)
		{
			_rowData = new Dictionary<TableColumn, string>();
			SearchString = searchString?.ToLower() ?? string.Empty;
			RowObject = rowObject;
		}

		public bool HasColumn(TableColumn column)
		{
			return _rowData.ContainsKey(column);
		}
	}
}
