using Pawnmorph.Chambers;
using Pawnmorph.DefExtensions;
using Pawnmorph.Genebank.Model;
using Pawnmorph.UserInterface;
using Pawnmorph.UserInterface.Genebank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Pawnmorph.Genebank
{
	internal class RecentGenebankSelector<T> : IExposable where T : GenebankTab, new()
	{
		private static readonly string NO_OPTIONS_TRANSLATION = "PMAnimalPickerGizmoNoChoices".Translate();

		private IGenebankEntry[] _recentOptions;
		private byte _recentLength;
		private ChamberDatabase _database;

		/// <summary>
		/// Occurs when user makes a selection.
		/// </summary>
		public event EventHandler<IGenebankEntry> OnSelected;

		/// <summary>
		/// Gets or sets additional options that will always be shown last.
		/// </summary>
		public IEnumerable<FloatMenuOption> AdditionalOptions { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether browse button is enabled.
		/// </summary>
		/// <value>
		///   <c>true</c> if the browse is enabled; otherwise, <c>false</c>.
		/// </value>
		public bool CanBrowse { get; set; }

		/// <summary>
		/// Gets or sets the row filter applied to genebank table when browsing.
		/// </summary>
		public Func<IGenebankEntry, bool> RowFilter { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="RecentGenebankSelector{T}"/> class.
		/// </summary>
		/// <param name="historyLength">How many selections to keep as recent.</param>
		/// <param name="database">Reference to the genebank database component.</param>
		public RecentGenebankSelector(byte historyLength, ChamberDatabase database)
		{
			_recentLength = historyLength;
			_recentOptions = new IGenebankEntry[_recentLength];
			_database = database;
			CanBrowse = true;
		}


		public void ShowOptions()
		{
			List<FloatMenuOption> options = new List<FloatMenuOption>();


			for (int i = _recentOptions.Length - 1; i >= 0; i--)
			{
				IGenebankEntry recentItem = _recentOptions[i];
				if (recentItem == null)
					continue;

				options.Add(new FloatMenuOption(recentItem.GetCaption(), () => ItemSelected(recentItem)));
			}

			if (CanBrowse)
				options.Add(new FloatMenuOption("Browse", BrowseGenebank));

			if (AdditionalOptions != null)
				options.AddRange(AdditionalOptions);

			if (options.Count == 0)
			{
				var emptyOption = new FloatMenuOption(NO_OPTIONS_TRANSLATION, null);
				emptyOption.Disabled = true;
				options.Add(emptyOption);
			}

			Find.WindowStack.Add(new FloatMenu(options));
		}

		private void BrowseGenebank()
		{
			GenebankTab tab = new T();
			tab.Initialize(_database);

			Dialog_BrowseGenebank browseDialog = new Dialog_BrowseGenebank(tab, (item) =>
			{
				if (item != null) 
				{

					ItemSelected(item);
				}
			});
			if (RowFilter != null)
			{
				browseDialog.RowFilter += (item) => RowFilter(item.RowObject);
			}
			Find.WindowStack.Add(browseDialog);
		}

		private void ItemSelected(IGenebankEntry item)
		{
			int index = Array.IndexOf(_recentOptions, item);
			if (index < 0)
			{
				// Get index of first null
				index = Array.IndexOf(_recentOptions, null);
				if (index < 0) // Otherwise start at the end.
					index = _recentOptions.Length - 1;
			}

			for (int i = index; i >= 1; i--)
				_recentOptions[i] = _recentOptions[i - 1];

			_recentOptions[0] = item;
			OnSelected?.Invoke(this, item);
		}


		public void ExposeData()
		{
			List<IGenebankEntry> recent = _recentOptions.Where(x => x != null).ToList();
			Scribe_Collections.Look(ref recent, "_recentGenebank" + typeof(T).Name);
			if (recent != null)
				recent.CopyTo(_recentOptions);
		}
	}
}
