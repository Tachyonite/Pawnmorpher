// ChamberTfInitiationReport.cs created by Iron Wolf for Pawnmorph on 06/25/2021 5:26 PM
// last updated 06/25/2021  5:26 PM

using System;

namespace Pawnmorph.Chambers
{
	/// <summary>
	///     simple struct to wrap the state of a chamber tf requirement
	/// </summary>
	public readonly struct ChamberTfInitiationReport : IEquatable<ChamberTfInitiationReport>
	{
		/// <summary>
		/// Gets the true.
		/// </summary>
		/// <value>
		/// The true.
		/// </value>
		public static ChamberTfInitiationReport True => new ChamberTfInitiationReport(true, "");

		/// <summary>
		///     if the chamber transformation can be initiated
		/// </summary>
		public readonly bool canInitiate;

		/// <summary>
		///     if canInitiate is false the reason why
		/// </summary>
		public readonly string reason;

		/// <summary>
		///     Initializes a new instance of the <see cref="ChamberTfInitiationReport" /> struct.
		/// </summary>
		/// <param name="canInitiate">if set to <c>true</c> [can initiate].</param>
		/// <param name="reason">The reason.</param>
		public ChamberTfInitiationReport(bool canInitiate, string reason)
		{
			this.canInitiate = canInitiate;
			this.reason = reason;
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		///     <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise,
		///     <see langword="false" />.
		/// </returns>
		public bool Equals(ChamberTfInitiationReport other)
		{
			return canInitiate == other.canInitiate
				&& reason == other.reason;
		}

		/// <summary>Indicates whether this instance and a specified object are equal.</summary>
		/// <param name="obj">The object to compare with the current instance. </param>
		/// <returns>
		///     <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same
		///     value; otherwise, <see langword="false" />.
		/// </returns>
		public override bool Equals(object obj)
		{
			return obj is ChamberTfInitiationReport other && Equals(other);
		}

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return (canInitiate.GetHashCode() * 397) ^ (reason != null ? reason.GetHashCode() : 0);
			}
		}

		/// <summary>
		///     Returns a value that indicates whether the values of two
		///     <see cref="T:Pawnmorph.Chambers.ChamberTfInitiationReport" /> objects are equal.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns>
		///     true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise,
		///     false.
		/// </returns>
		public static bool operator ==(ChamberTfInitiationReport left, ChamberTfInitiationReport right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///     Performs an implicit conversion from <see cref="ChamberTfInitiationReport" /> to <see cref="System.Boolean" />.
		/// </summary>
		/// <param name="report">The report.</param>
		/// <returns>
		///     report.canInitiate
		/// </returns>
		public static implicit operator bool(ChamberTfInitiationReport report)
		{
			return report.canInitiate;
		}

		/// <summary>
		///     Returns a value that indicates whether two <see cref="T:Pawnmorph.Chambers.ChamberTfInitiationReport" />
		///     objects have different values.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
		public static bool operator !=(ChamberTfInitiationReport left, ChamberTfInitiationReport right)
		{
			return !left.Equals(right);
		}


		/// <summary>
		///     Converts to string.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			if (canInitiate) return "true";
			return reason ?? "";
		}
	}
}