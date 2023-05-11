using System.Collections;
using System.Collections.Generic;
using Verse;

namespace Pawnmorph.Utilities.Collections
{
	internal class ExposableList<T> : IExposable, IEnumerable<T>, IList<T>
	{
		private List<T> _list;

		public List<T> List => _list;

		public int Count => ((ICollection<T>)_list).Count;

		public bool IsReadOnly => ((ICollection<T>)_list).IsReadOnly;

		public T this[int index] { get => ((IList<T>)_list)[index]; set => ((IList<T>)_list)[index] = value; }

		public ExposableList()
		{
			_list = new List<T>();
		}

		public ExposableList(IEnumerable<T> collection)
		{
			_list = new List<T>(collection);
		}

		public void ExposeData()
		{
			Scribe_Collections.Look<T>(ref _list, "list", LookMode.Deep);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		public int IndexOf(T item)
		{
			return ((IList<T>)_list).IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			((IList<T>)_list).Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			((IList<T>)_list).RemoveAt(index);
		}

		public void Add(T item)
		{
			((ICollection<T>)_list).Add(item);
		}

		public void Clear()
		{
			((ICollection<T>)_list).Clear();
		}

		public bool Contains(T item)
		{
			return ((ICollection<T>)_list).Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			((ICollection<T>)_list).CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			return ((ICollection<T>)_list).Remove(item);
		}
	}
}
