using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Pawnmorph.Utilities.Collections
{
	internal class ListFilter<T>
	{
		ReadOnlyCollection<T> _filteredCollection;
		List<T> _totalCollection;
		Func<T, string, bool> _filterCallback;
		private string _filterString;

		/// <summary>
		/// Gets the filtered collection of items.
		/// </summary>
		public ReadOnlyCollection<T> Filtered => _filteredCollection;

		/// <summary>
		/// Gets or sets the collection with all items.
		/// </summary>
		public IList<T> Items
		{
			get => _totalCollection;
			set
			{
				_totalCollection = value.ToList();
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the filter string parsed to the filter callback when filtering collection.
		/// </summary>
		public string Filter
		{
			get => _filterString;
			set
			{
				if (_filterString == value || (_filterString != null && _filterString.Equals(value)))
					return;

				_filterString = value.ToLower();
				Invalidate();
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ListFilter{T}"/> class.
		/// </summary>
		/// <param name="collection">Initial collection that is copied.</param>
		/// <param name="filterCallback">Filter callback called for each item when filter text is modified. Provides Item, Filtertext and expects a bool returned on whether or not item is visible.</param>
		public ListFilter(IEnumerable<T> collection, Func<T, string, bool> filterCallback)
		{
			_totalCollection = collection.ToList();
			_filterCallback = filterCallback;
			Invalidate();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ListFilter{T}"/> class.
		/// </summary>
		/// <param name="filterCallback">Filter callback called for each item when filter text is modified. Provides Item, Filtertext and expects a bool returned on whether or not item is visible.</param>
		public ListFilter(Func<T, string, bool> filterCallback)
		{
			_totalCollection = new List<T>();
			_filterCallback = filterCallback;
		}

		/// <summary>
		/// Orders underlying collection ascending.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="keySelector">The key selector.</param>
		public void OrderBy<TKey>(Func<T, TKey> keySelector)
		{
			_totalCollection = _totalCollection.OrderBy(keySelector).ToList();
			Invalidate();
		}

		/// <summary>
		/// Orders underlying collection descending.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="keySelector">The key selector.</param>
		public void OrderByDescending<TKey>(Func<T, TKey> keySelector)
		{
			_totalCollection = _totalCollection.OrderByDescending(keySelector).ToList();
			Invalidate();
		}

		/// <summary>
		/// Invalidates the filtered collection and regenerates it.
		/// </summary>
		public void Invalidate()
		{
			if (String.IsNullOrEmpty(_filterString))
			{
				_filteredCollection = _totalCollection.ToList().AsReadOnly();
				return;
			}

			_filteredCollection = _totalCollection.Where(x => _filterCallback(x, _filterString)).ToList().AsReadOnly();
		}

	}
}
