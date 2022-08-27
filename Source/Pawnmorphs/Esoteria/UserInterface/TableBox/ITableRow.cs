using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnmorph.UserInterface.TableBox
{
    internal interface ITableRow
    {
        public bool HasColumn(TableColumn column);

        public string this[TableColumn key]
        {
            get;
            set;
        }
    }
}
