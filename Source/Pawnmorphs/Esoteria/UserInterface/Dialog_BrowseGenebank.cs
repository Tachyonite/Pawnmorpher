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
		private const float SPACING = 10f;
		private static readonly string BUTTON_SELECT = "PM_Genebank_DeleteButton".Translate();
		private static readonly float BUTTON_SELECT_SIZE;
		static Dialog_BrowseGenebank()
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
		/// Initializes a new instance of the <see cref="Dialog_BrowseGenebank"/> class.
		/// </summary>
		/// <param name="tab">The genebank tab to show.</param>
		/// <param name="onCloseCallback">Callback called when window is closed with selected row.</param>
		public Dialog_BrowseGenebank(GenebankTab tab, Action<IGenebankEntry> onCloseCallback = null)
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
			Vector2 size = PawnmorpherMod.Settings.GenebankWindowSize ?? new Vector2(Screen.width * 0.9f, Screen.height * 0.8f);
			_table.LineFont = PawnmorpherMod.Settings.GenebankWindowFont ?? GameFont.Tiny;

			base.windowRect = new Rect(location, size);
		}

		public override void PostOpen()
		{
			base.PostOpen();
			_tab.GenerateTable(_table);
		}

		private void Table_SelectionChanged(object sender, IReadOnlyList<GeneRowItem> e)
		{
			Selected = null;
			if (e.Count == 1)
				Selected = e[0].Def;
		}

		public override void DoWindowContents(Rect inRect)
		{

			Rect mainBox = inRect;
			mainBox.y += TabDrawer.TabHeight;
			mainBox.height -= TabDrawer.TabHeight;

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
