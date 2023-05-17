using Pawnmorph.Chambers;
using Pawnmorph.UserInterface.Genebank.Tabs;
using Pawnmorph.UserInterface.Genebank;
using Pawnmorph.UserInterface.TableBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Pawnmorph.UserInterface
{
	internal class Window_Sequencer : Window
	{
		private readonly string HEADER = "PM_Genebank_Header".Translate();
		private readonly string CAPACITY_AVAILABLE = "PM_Genebank_AvailableHeader".Translate();
		private readonly string CAPACITY_TOTAL = "PM_Genebank_TotalHeader".Translate();
		private readonly float CAPACITY_WIDTH;


		private readonly string TAB_MUTATIONS_HEADER = "PM_Genebank_MutationTab_Caption".Translate();
		private readonly string TAB_ANIMALS_HEADER = "PM_Genebank_AnimalsTab_Caption".Translate();
		private readonly string TAB_TEMPLATES_HEADER = "PM_Genebank_TemplateTab_Caption".Translate();

		private readonly string COLUMN_SIZE = "PM_Column_Stats_Size".Translate();
		private readonly float COLUMN_SIZE_SIZE;

		private readonly string BUTTON_DELETE = "PM_Genebank_DeleteButton".Translate();
		private readonly float BUTTON_DELETE_SIZE;

		private readonly string BUTTON_FONT = "PM_Genebank_FontButton".Translate();
		private readonly float BUTTON_FONT_SIZE;
		private readonly string BUTTON_FONT_TINY = "PM_Genebank_FontButtonTiny".Translate();
		private readonly string BUTTON_FONT_SMALL = "PM_Genebank_FontButtonSmall".Translate();
		private readonly string BUTTON_FONT_MEDIUM = "PM_Genebank_FontButtonMedium".Translate();

		private const float MAIN_COLUMN_WIDTH_FRACT = 0.60f;
		private const float SPACING = 10f;
		private const float HEADER_HEIGHT = 150;

		private readonly List<TabRecord> _tabs;
		private readonly Table<GeneRowItem> _table;
		private float _mainWidth;
		private float _detailsWidth;
		private float _currentY;
		private ChamberDatabase _chamberDatabase;

		public Window_Sequencer()
		{
			Text.Font = GameFont.Small;
			CAPACITY_WIDTH = Math.Max(Text.CalcSize(CAPACITY_AVAILABLE).x, Text.CalcSize(CAPACITY_TOTAL).x) * 2 + SPACING * 2;
			COLUMN_SIZE_SIZE = Mathf.Max(Text.CalcSize(COLUMN_SIZE).x, 100f);
			BUTTON_DELETE_SIZE = Mathf.Max(Text.CalcSize(BUTTON_DELETE).x, 100f);
			BUTTON_FONT_SIZE = Mathf.Max(Text.CalcSize(BUTTON_FONT).x, 80f);

			_tabs = new List<TabRecord>();
			_table = new Table<GeneRowItem>((item, text) => item.SearchString.Contains(text));
			_table.SelectionChanged += Table_SelectionChanged;
			_table.MultiSelect = true;

			this.resizeable = true;
			this.draggable = true;
			this.doCloseX = true;
		}

		private void Table_SelectionChanged(object sender, IReadOnlyList<GeneRowItem> e)
		{

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


			_chamberDatabase = Find.World.GetComponent<ChamberDatabase>();
			if (_chamberDatabase == null)
			{
				Log.Error("Unable to find chamber database world component!");
				this.Close();
			}



		}

		public override void DoWindowContents(Rect inRect)
		{
			throw new NotImplementedException();
		}
	}
}
