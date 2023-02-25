// MutationSite.cs created by Iron Wolf for Pawnmorph on 05/25/2020 9:46 AM
// last updated 05/25/2020  9:46 AM

using System;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// readonly struct describing the 'slot' or 'site' a mutation can occupy 
	/// </summary>
	/// <seealso cref="MutationSite" />
	public readonly struct MutationSite : IEquatable<MutationSite>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MutationSite"/> struct.
		/// </summary>
		/// <param name="record">The record.</param>
		/// <param name="layer">The layer.</param>
		public MutationSite(BodyPartRecord record, MutationLayer layer)
		{
			Record = record;
			Layer = layer;
		}

		/// <summary>Returns the fully qualified type name of this instance.</summary>
		/// <returns>The fully qualified type name.</returns>
		public override string ToString()
		{
			return $"{Record?.Label ?? "NULL"}:{Layer}";
		}

		/// <summary>
		/// Gets the body part record of this site
		/// </summary>
		/// <value>
		/// The record.
		/// </value>
		[CanBeNull]
		public BodyPartRecord Record { get; }

		/// <summary>
		/// Gets the layer of this site
		/// </summary>
		/// <value>
		/// The layer.
		/// </value>
		public MutationLayer Layer { get; }

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
		public bool Equals(MutationSite other)
		{
			return Equals(Record, other.Record)
				&& Layer == other.Layer;
		}

		/// <summary>Indicates whether this instance and a specified object are equal.</summary>
		/// <param name="obj">The object to compare with the current instance. </param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />. </returns>
		public override bool Equals(object obj)
		{
			return obj is MutationSite other && Equals(other);
		}

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return ((Record != null ? Record.GetHashCode() : 0) * 397) ^ (int)Layer;
			}
		}

		/// <summary>Returns a value that indicates whether the values of two <see cref="T:Pawnmorph.MutationSite" /> objects are equal.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
		public static bool operator ==(MutationSite left, MutationSite right)
		{
			return left.Equals(right);
		}

		/// <summary>Returns a value that indicates whether two <see cref="T:Pawnmorph.MutationSite" /> objects have different values.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
		public static bool operator !=(MutationSite left, MutationSite right)
		{
			return !left.Equals(right);
		}
	}
}