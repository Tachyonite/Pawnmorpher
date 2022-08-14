using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnmorph.User_Interface.TableBox
{
    internal interface ITableRow
    {
        public Dictionary<TableColumn, string> RowData { get; }
    }
}
