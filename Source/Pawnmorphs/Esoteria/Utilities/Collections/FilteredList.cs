using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

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


        public ListFilter(IEnumerable<T> collection, Func<T, string, bool> filterCallback)
        {
            _totalCollection = collection.ToList();
            _filterCallback = filterCallback;
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
