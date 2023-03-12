// LinqUtils.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/13/2019 6:50 AM
// last updated 08/13/2019  6:50 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Utilities
{
	/// <summary>
	///     utilities around IEnumerable interface
	/// </summary>
	public static class LinqUtils
	{
		/// <summary>
		///     Adds the distinct range to the given range
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="lst">The LST.</param>
		/// <param name="range">The range.</param>
		public static void AddDistinctRange<T>([NotNull] this List<T> lst, [NotNull] IEnumerable<T> range)
		{
			if (lst == null) throw new ArgumentNullException(nameof(lst));
			if (range == null) throw new ArgumentNullException(nameof(range));

			foreach (T val in range)
			{
				if (lst.Contains(val)) continue;
				lst.Add(val);
			}
		}

		/// <summary>
		/// returns an enumeration of the intersection of all enumerables given 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable">The enumerable.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">enumerable collection must contain at least 1 list - enumerable</exception>
		public static IEnumerable<T> IntersectAll<T>([NotNull] this IEnumerable<IEnumerable<T>> enumerable)
		{
			List<IEnumerable<T>>
				tmpList = enumerable
				   .ToList(); //need to store it as a list first as we can't assume enumerable can be enumerated over multiple times 

			if (tmpList.Count == 0)
				throw new ArgumentException("enumerable collection must contain at least 1 list", nameof(enumerable));
			var hSet = new HashSet<T>(tmpList[0].MakeSafe());
			for (var i = 1; i < tmpList.Count; i++)
			{
				IEnumerable<T> l = tmpList[i];
				hSet.IntersectWith(l.MakeSafe());
				if (hSet.Count == 0) break;
			}

			return hSet;
		}

		/// <summary>
		///     Adds the range to this linked list at the end of the list.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="ll">The ll.</param>
		/// <param name="enumerable">The enumerable.</param>
		/// <exception cref="ArgumentNullException">
		///     ll
		///     or
		///     enumerable
		/// </exception>
		public static void AddRange<T>([NotNull] this LinkedList<T> ll, [NotNull] IEnumerable<T> enumerable)
		{
			if (ll == null) throw new ArgumentNullException(nameof(ll));
			if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
			foreach (T i in enumerable) ll.AddLast(i);
		}

		/// <summary>
		///     Adds the element to the list with the given key, or creates a new list if the key is not in the dictionary already
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TElem">The type of the elem.</typeparam>
		/// <param name="dict">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="elem">The elem.</param>
		/// <exception cref="ArgumentNullException">
		///     dict
		///     or
		///     key
		/// </exception>
		public static void AddToKey<TKey, TElem>([NotNull] this IDictionary<TKey, List<TElem>> dict, [NotNull] TKey key,
												 [CanBeNull] TElem elem)
		{
			if (dict == null) throw new ArgumentNullException(nameof(dict));
			if (key == null) throw new ArgumentNullException(nameof(key));

			if (!dict.TryGetValue(key, out List<TElem> lst))
			{
				lst = new List<TElem> { elem };
				dict[key] = lst;
			}
			else
			{
				lst.Add(elem);
			}
		}

		/// <summary>
		///     Determines whether this enumeration of hediffDefs contains the def of the given hediff.
		/// </summary>
		/// <param name="enumerable">The enumerable.</param>
		/// <param name="hediff">The hediff.</param>
		/// <returns>
		///     <c>true</c> if this enumeration of hediffDefs contains the def of the given hediff; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     enumerable
		///     or
		///     hediff
		/// </exception>
		public static bool ContainsHediff([NotNull] this IEnumerable<HediffDef> enumerable, [NotNull] Hediff hediff)
		{
			if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
			if (hediff == null) throw new ArgumentNullException(nameof(hediff));

			foreach (HediffDef hediffDef in enumerable)
				if (hediffDef == hediff.def)
					return true;

			return false;
		}

		// In
		/// <summary> Check if this instance is in the given collection. </summary>
		/// <param name="thing"> The thing. </param>
		/// <param name="other"> The other. </param>
		public static bool In<T>(this T thing, T other) //explicit overloads for 1,2,3 make In a bit faster 
		{
			EqualityComparer<T> equals = EqualityComparer<T>.Default;
			return equals.Equals(thing, other);
		}

		/// <summary> Check if this instance is in the given collection. </summary>
		/// <param name="thing"> The thing. </param>
		/// <param name="other1"> The other1. </param>
		/// <param name="other2"> The other2. </param>
		public static bool In<T>(this T thing, T other1, T other2)
		{
			EqualityComparer<T> equals = EqualityComparer<T>.Default;
			return equals.Equals(thing, other1) || equals.Equals(thing, other2);
		}

		/// <summary> Check if this instance is in the given collection. </summary>
		/// <param name="thing"> The thing. </param>
		/// <param name="other1"> The other1. </param>
		/// <param name="other2"> The other2. </param>
		/// <param name="other3"> The other3. </param>
		public static bool In<T>(this T thing, T other1, T other2, T other3)
		{
			EqualityComparer<T> equals = EqualityComparer<T>.Default;
			return equals.Equals(thing, other1) || equals.Equals(thing, other2) || equals.Equals(thing, other3);
		}

		// Fallthrough
		/// <summary> Check if this instance is in the given collection. </summary>
		/// <param name="thing"> The thing. </param>
		/// <param name="others"> The others. </param>
		public static bool In<T>(this T thing, params T[] others)
		{
			return others.Contains(thing);
		}


		/// <summary>
		///     Determines whether this list is both non null and not empty.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="lst">The LST.</param>
		/// <returns>
		///     <c>true</c> if this list is both non null and not empty; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsNonNullAndNonEmpty<T>([CanBeNull] this IReadOnlyList<T> lst)
		{
			if (lst == null) return false;
			return lst.Count > 0;
		}

		/// <summary>
		///     if the given enumerable is null returns an empty enumerable, otherwise does nothing to the given enumerable
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable">The enumerable.</param>
		/// <returns></returns>
		[NotNull]
		public static IEnumerable<T> MakeSafe<T>([CanBeNull][NoEnumeration] this IEnumerable<T> enumerable)
		{
			return enumerable ?? Enumerable.Empty<T>();
		}


		/// <summary>gets a random element from the list</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="lst">The LST.</param>
		/// <param name="defaultVal">The default value.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">lst</exception>
		public static T RandElement<T>([NotNull] this IList<T> lst, T defaultVal = default)
		{
			if (lst == null) throw new ArgumentNullException(nameof(lst));
			if (lst.Count == 0) return defaultVal;
			if (lst.Count == 1) return lst[0];

			return lst[Rand.Range(0, lst.Count)];
		}
	}
}