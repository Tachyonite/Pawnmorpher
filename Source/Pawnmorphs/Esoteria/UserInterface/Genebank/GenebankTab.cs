using System.Collections.Generic;
using Pawnmorph.Chambers;
using Pawnmorph.Genebank.Model;
using Pawnmorph.UserInterface.TableBox;
using UnityEngine;
using Verse;

namespace Pawnmorph.UserInterface.Genebank
{
	abstract class GenebankTab
	{
		protected const float SPACING = 10f;
		protected const float PREVIEW_SIZE = 200f;
		protected readonly string FEMALE = "Female".Translate().CapitalizeFirst();
		protected readonly string MALE = "Male".Translate().CapitalizeFirst();
		public Window_Genebank Parent;

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
		/// Invoked after columns have been added before any rows are added to a table.
		/// Add postfix patch to this to add additional custom columns to the table.
		/// </summary>
		/// <param name="table">The generated table. Use AddColumn method to add new columns.</param>
		/// <example>
		/// table.AddColumn(Caption, Size);
		/// </example>
		public abstract void AddColumnHook(Table<GeneRowItem> table);


		/// <summary>
		/// Invoked after a new row is generated but before it is added to a table.
		/// Add postfix patch to this to add custom columns and data.
		/// </summary>
		/// <param name="row">The newly generated row. Add column data to row indexer, see example.</param>
		/// <param name="searchText">The search string for this specific row. Append any additional searchable words to this.</param>
		/// <example>
		/// row[column] = "CellValue";
		/// </example>
		public abstract void AddedRowHook(GeneRowItem row, string searchText);

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
		public abstract void Delete(IGenebankEntry def);

		/// <summary>
		/// Called every frame to draw additional footer buttons.
		/// </summary>
		/// <param name="inRect">The footer bounding box.</param>
		public virtual void DrawFooter(Rect inRect)
		{ }

	}
}
