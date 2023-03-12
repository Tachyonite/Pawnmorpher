using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Pawnmorph.Utilities.Collections
{
	/// <summary>
	/// A simple multi-value dictionary implementation that maps one key to multiple values
	/// Uses lists internally, so is not particularly efficient for large numbers of values on one key
	/// </summary>
	public class MultiDict<K, V>
	{
		private readonly Dictionary<K, List<V>> dict = new Dictionary<K, List<V>>();

		/// <summary>
		/// Gets or sets all the values with the specified key at once.  An empty or
		/// null collection deletes the key
		/// </summary>
		/// <param name="key">Key.</param>
		public ICollection<V> this[K key]
		{
			get => dict.TryGetValue(key, Enumerable.Empty<V>().ToList());
			set
			{
				var val = value?.ToList();
				if (val != null && val.Count > 0)
					dict[key] = val;
				else
					dict.Remove(key);
			}
		}

		/// <summary>
		/// Returns a collection of all the keys in the dictionary
		/// </summary>
		/// <value>The keys.</value>
		public ICollection<K> Keys => dict.Keys;

		/// <summary>
		/// Returns a collection of all the values in the dictionary
		/// </summary>
		/// <value>The values.</value>
		public ICollection<V> Values => dict.Values.SelectMany(l => l.AsEnumerable()).ToList();

		/// <summary>
		/// Returns a count of all values in the dictionary
		/// </summary>
		/// <value>The count.</value>
		public int Count => dict.Values.SelectMany(l => l.AsEnumerable()).Count();

		/// <summary>
		/// Adds the specified value to the dictionary under the given key
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public void Add(K key, V value)
		{
			if (!dict.TryGetValue(key, out List<V> list))
			{
				list = new List<V>();
				dict[key] = list;
			}
			list.Add(value);
		}

		/// <summary>
		/// Adds the specified value to the dictionary under the given key
		/// </summary>
		/// <param name="item">the key-value pair.</param>
		public void Add(KeyValuePair<K, V> item)
		{
			Add(item.Key, item.Value);
		}

		/// <summary>
		/// Adds multiple values to the dictionary under the given key
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="values">Values.</param>
		public void Add(K key, IEnumerable<V> values)
		{
			if (!dict.TryGetValue(key, out List<V> list))
			{
				list = new List<V>();
				dict[key] = list;
			}
			list.AddRange(values);
		}

		/// <summary>
		/// Empties the dictionary
		/// </summary>
		public void Clear()
		{
			dict.Clear();
		}

		/// <summary>
		/// Checkes whether the dictionary contains the specific value
		/// </summary>
		/// <returns><see langword="true"/> if the value is contained, <see langword="false"/> otherwise.</returns>
		/// <param name="value">The value.</param>
		public bool Contains(V value)
		{
			return Values.Contains(value);
		}

		/// <summary>
		/// Checkes whether the dictionary contains the specific value under the give key
		/// </summary>
		/// <returns><see langword="true"/> if the value is contained, <see langword="false"/> otherwise.</returns>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public bool Contains(K key, V value)
		{
			return dict[key]?.Contains(value) ?? false;
		}

		/// <summary>
		/// Checkes whether the dictionary contains the specific value under the given key
		/// </summary>
		/// <returns><see langword="true"/> if the value is contained, <see langword="false"/> otherwise.</returns>
		/// <param name="item">The key-value pair.</param>
		public bool Contains(KeyValuePair<K, V> item)
		{
			return Contains(item.Key, item.Value);
		}

		/// <summary>
		/// Checkes whether the dictionary contains the specific key
		/// </summary>
		/// <returns><see langword="true"/> if the value is contained, <see langword="false"/> otherwise.</returns>
		/// <param name="key">Item.</param>
		public bool ContainsKey(K key)
		{
			return dict.ContainsKey(key);
		}

		/// <summary>
		/// Gets a key-value pair enumerator.
		/// </summary>
		/// <returns>The enumerator.</returns>
		public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
		{
			return dict.SelectMany(kvp => kvp.Value.Select(v => new KeyValuePair<K, V>(kvp.Key, v))).GetEnumerator();
		}

		/// <summary>
		/// Remove all values associated with the specified key.
		/// </summary>
		/// <returns><see langword="true"/> if the key existed and was removed, <see langword="false"/> otherwise./// </returns>
		/// <param name="key">Key.</param>
		public bool Remove(K key)
		{
			return dict.Remove(key);
		}

		/// <summary>
		/// Removes the given value associated with the specified key.
		/// </summary>
		/// <returns><see langword="true"/> if the value existed and was removed, <see langword="false"/> otherwise.</returns>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public bool Remove(K key, V value)
		{
			if (dict.TryGetValue(key, out List<V> list))
			{
				return list.Remove(value);
			}
			return false;
		}

		/// <summary>
		/// Removes the given value associated with the specified key.
		/// </summary>
		/// <returns><see langword="true"/> if the value existed and was removed, <see langword="false"/> otherwise.</returns>
		/// <param name="item">The key-value pair.</param>
		public bool Remove(KeyValuePair<K, V> item)
		{
			return Remove(item.Key, item.Value);
		}

		/// <summary>
		/// Removes all the given value associated with the specified key.
		/// </summary>
		/// <returns><see langword="true"/> if at least one value existed and was removed, <see langword="false"/> otherwise.</returns>
		/// <param name="key">Key.</param>
		/// <param name="values">Values.</param>
		public bool Remove(K key, IEnumerable<V> values)
		{
			if (dict.TryGetValue(key, out List<V> list))
			{
				bool ret = false;
				foreach (V v in values)
					ret |= list.Remove(v);
				return ret;
			}
			return false;
		}

		/// <summary>
		/// Tries to get the values associated with the key.
		/// </summary>
		/// <returns><c>true</c>, if the at least one value exists under that key, <c>false</c> otherwise.</returns>
		/// <param name="key">Key.</param>
		/// <param name="values">Values.</param>
		public bool TryGetValue(K key, out ICollection<V> values)
		{
			bool ret = dict.TryGetValue(key, out List<V> list);
			values = list ?? Enumerable.Empty<V>().ToList();
			return ret;
		}
	}
}
