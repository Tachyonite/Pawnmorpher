using Pawnmorph.Genebank.Model;
using Pawnmorph.UserInterface.Genebank;
using Pawnmorph.UserInterface.TableBox;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Pawnmorph.UserInterface
{
	internal class Dialog_BrowseGenebank : Window
	{
		private const float MAIN_COLUMN_WIDTH_FRACT = 0.60f;
		private const float SPACING = 10f;
		private readonly string BUTTON_SELECT = "PM_Genebank_SelectButton".Translate();
		private readonly float BUTTON_SELECT_SIZE;

		public Dialog_BrowseGenebank()
		{
			BUTTON_SELECT_SIZE = Mathf.Max(Text.CalcSize(BUTTON_SELECT).x, 100f);
		}


		private GenebankTab _tab;
		private Table<GeneRowItem> _table;

		/// <summary>
		/// Gets the selected row.
		/// </summary>
		public IGenebankEntry Selected { get; private set; }

		/// <summary>
		/// Gets or sets the callback action called when window is closed.
		/// </summary>
		public Action<IGenebankEntry> OnCloseCallback { get; set; }

		/// <summary>
		/// Gets or sets the table's row filter.
		/// </summary>
		public Func<GeneRowItem, bool> RowFilter
		{
			get => _table.RowFilter;
			set => _table.RowFilter = value;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="Dialog_BrowseGenebank"/> class.
		/// </summary>
		/// <param name="tab">The genebank tab to show.</param>
		/// <param name="onCloseCallback">Callback called when window is closed with selected row.</param>
		public Dialog_BrowseGenebank(GenebankTab tab, Action<IGenebankEntry> onCloseCallback = null)
			: this()
		{
			_tab = tab;

			_table = new Table<GeneRowItem>((item, text) => item.SearchString.Contains(text));
			_table.SelectionChanged += Table_SelectionChanged;
			_table.MultiSelect = false;

			this.resizeable = true;
			this.draggable = true;
			this.doCloseX = true;
			OnCloseCallback = onCloseCallback;
		}

		protected override void SetInitialSizeAndPosition()
		{
			base.SetInitialSizeAndPosition();

			Vector2 location = PawnmorpherMod.Settings.GenebankWindowLocation ?? new Vector2(40, 40);
			Vector2 size = PawnmorpherMod.Settings.GenebankWindowSize ?? new Vector2(UI.screenWidth * 0.9f, UI.screenHeight * 0.8f);
			_table.LineFont = PawnmorpherMod.Settings.GenebankWindowFont ?? GameFont.Tiny;

			base.windowRect = new Rect(location, size);
		}

		public override void PostOpen()
		{
			base.PostOpen();
			_table.Clear();
			_tab.GenerateTable(_table);
			_table.Refresh();
		}

		private void Table_SelectionChanged(object sender, IReadOnlyList<GeneRowItem> e)
		{
			Selected = null;
			if (e.Count == 1)
				Selected = e[0].RowObject;

			_tab.SelectionChanged(e);
		}

		public override void DoWindowContents(Rect inRect)
		{
			float mainWidth = inRect.width * MAIN_COLUMN_WIDTH_FRACT;
			float detailsWidth = inRect.width - mainWidth - SPACING;

			Rect mainBox = inRect;
			mainBox.width = mainWidth;

			Widgets.DrawBoxSolidWithOutline(mainBox, Color.black, Color.gray);


			Rect footer = new Rect(mainBox.x, mainBox.yMax - 40, mainBox.width, 40);
			mainBox.height -= footer.height;
			footer = footer.ContractedBy(SPACING);

			_table.Draw(mainBox.ContractedBy(SPACING));

			Text.Font = GameFont.Small;

			Widgets.DrawLineHorizontal(footer.x, footer.y - SPACING, footer.width);
			if (Widgets.ButtonText(new Rect(footer.x, footer.y, BUTTON_SELECT_SIZE, footer.height), BUTTON_SELECT))
			{
				SelectRow();
			}


			Rect detailsBox = new Rect(inRect.xMax - detailsWidth, inRect.y, detailsWidth, inRect.height + SPACING);
			Widgets.DrawBoxSolidWithOutline(detailsBox, Color.black, Color.gray);
			
			_tab.DrawDetails(detailsBox.ContractedBy(SPACING));

			//if (_tab != null)
			//	_tab.DrawFooter(new Rect(footer.x + BUTTON_SELECT_SIZE + SPACING, footer.y, footer.width - BUTTON_SELECT_SIZE - SPACING, footer.height));
		}

		private void SelectRow()
		{
			OnCloseCallback?.Invoke(Selected);
			this.Close();
		}
	}
}
