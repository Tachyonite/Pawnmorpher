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

		private Queue<IGenebankEntry> _recentOptions;
		private byte _recentLength;
		private ChamberDatabase _database;

		public event EventHandler<IGenebankEntry> OnSelected;

		public IList<FloatMenuOption> AdditionalOptions { get; set; }

		public bool CanBrowse { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="RecentGenebankSelector{T}"/> class.
		/// </summary>
		/// <param name="historyLength">How many selections to keep as recent.</param>
		/// <param name="database">Reference to the genebank database component.</param>
		public RecentGenebankSelector(byte historyLength, ChamberDatabase database)
		{
			_recentLength = historyLength;
			_recentOptions = new Queue<IGenebankEntry>(historyLength);
			_database = database;
		}


		public void ShowOptions()
		{
			List<FloatMenuOption> options = new List<FloatMenuOption>();

			foreach (var recentItem in _recentOptions)
			{
				options.Add(new FloatMenuOption(recentItem.GetCaption(), () => ItemSelected(recentItem)));
			}

			if (CanBrowse)
				options.Add(new FloatMenuOption("TRANSLATE: Browse", BrowseGenebank));

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

			Find.WindowStack.Add(browseDialog);
		}

		private void ItemSelected(IGenebankEntry item)
		{
			if (_recentOptions.Count == _recentLength)
				_recentOptions.Dequeue();
			_recentOptions.Enqueue(item);
			OnSelected?.Invoke(this, item);
		}


		public void ExposeData()
		{
			List<IGenebankEntry> recent = _recentOptions.ToList();
			Scribe_Collections.Look(ref recent, "_recentGenebank" + typeof(T).Name);

			if (recent != null)
			{
				_recentOptions.Clear();
				for (int i = 0; i < recent.Count; i++)
				{
					_recentOptions.Enqueue(recent[i]);
				}
			}
		}
	}
}
