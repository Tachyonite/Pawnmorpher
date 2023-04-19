// PartAddress.cs created by Iron Wolf for Pawnmorph on 03/16/2020 6:55 PM
// last updated 03/16/2020  6:55 PM

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	///     this class represents the 'address' of a body part on a pawn
	/// </summary>
	public class PartAddress : IReadOnlyList<string>, IEquatable<PartAddress>
	{
		private static readonly Dictionary<string, PartAddress> _internDict = new Dictionary<string, PartAddress>();

		[NotNull] private readonly List<string> _lst;

		/// <summary>
		/// Loads the data from XML custom.
		/// </summary>
		/// <param name="xmlRoot">The XML root.</param>
		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			_lst.Clear();
			if (xmlRoot.NodeType == XmlNodeType.Text || xmlRoot.NodeType == XmlNodeType.CDATA)
			{
				_lst.AddRange(xmlRoot.Value.Split('.'));
			}
			else
			{
				Log.Error($"in {xmlRoot.Name}: trying to load {nameof(PartAddress)}, expected string or CDATA but got {xmlRoot.NodeType}");
			}
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="PartAddress" /> class.
		/// </summary>
		public PartAddress()
		{
			_lst = new List<string>();
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="PartAddress" /> class.
		/// </summary>
		/// <param name="address">The address.</param>
		/// <exception cref="ArgumentNullException">address</exception>
		public PartAddress([NotNull] IEnumerable<string> address)
		{
			if (address == null) throw new ArgumentNullException(nameof(address));
			_lst = address.ToList();
		}

		/// <summary>Returns an enumerator that iterates through a collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_lst).GetEnumerator();
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<string> GetEnumerator()
		{
			return _lst.GetEnumerator();
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		///     <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise,
		///     <see langword="false" />.
		/// </returns>
		public bool Equals(PartAddress other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			if (other._lst.Count != _lst.Count) return false;

			for (var i = 0; i < _lst.Count; i++)
				if (_lst[i] != other[i])
					return false;

			return true;
		}

		/// <summary>Gets the number of elements in the collection.</summary>
		/// <returns>The number of elements in the collection. </returns>
		public int Count => _lst.Count;

		/// <summary>Gets the element at the specified index in the read-only list.</summary>
		/// <param name="index">The zero-based index of the element to get. </param>
		/// <returns>The element at the specified index in the read-only list.</returns>
		public string this[int index] => _lst[index];

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object. </param>
		/// <returns>
		///     <see langword="true" /> if the specified object  is equal to the current object; otherwise,
		///     <see langword="false" />.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((PartAddress)obj);
		}

		/// <summary>Serves as the default hash function. </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				if (_lst.Count == 0) return 0;
				const int prime = 31;
				var result = 1;
				foreach (string s in _lst) result = result * prime + s.GetHashCode();

				return result;
			}
		}

		/// <summary>
		///     Returns a value that indicates whether the values of two <see cref="T:Pawnmorph.PartAddress" /> objects are
		///     equal.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns>
		///     true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise,
		///     false.
		/// </returns>
		public static bool operator ==(PartAddress left, PartAddress right)
		{
			return Equals(left, right);
		}

		/// <summary>
		///     Returns a value that indicates whether two <see cref="T:Pawnmorph.PartAddress" /> objects have different
		///     values.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
		public static bool operator !=(PartAddress left, PartAddress right)
		{
			return !Equals(left, right);
		}

		/// <summary>
		///     Parses the specified string into a part address
		/// </summary>
		/// the input string is supposed to be in the form of torso.leg.toe etc.
		/// <param name="str">The string.</param>
		/// <returns></returns>
		[NotNull]
		public static PartAddress Parse(string str)
		{
			if (_internDict.TryGetValue(str, out PartAddress addr)) return addr;

			addr = new PartAddress(str.Split('.'));
			_internDict[str] = addr;
			return addr;
		}

		/// <summary>Returns a string that represents the current object.</summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString()
		{
			return string.Join(".", _lst);
		}
	}
}