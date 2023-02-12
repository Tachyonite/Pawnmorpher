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
