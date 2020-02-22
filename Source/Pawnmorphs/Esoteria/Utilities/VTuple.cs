// VTuple.cs created by Iron Wolf for Pawnmorph on 09/12/2019 1:00 PM
// last updated 09/12/2019  1:00 PM

using System;
using System.Collections.Generic;

namespace Pawnmorph.Utilities
{
    //value tuples 
    /// <summary>
    /// simple value tuple implementation 
    /// </summary>
    /// <typeparam name="TFirst">The type of the first.</typeparam>
    /// <typeparam name="TSecond">The type of the second.</typeparam>
    public readonly struct VTuple<TFirst, TSecond> : IEquatable<VTuple<TFirst, TSecond>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VTuple{TFirst, TSecond}"/> struct.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        public VTuple(TFirst first, TSecond second)
        {
            First = first;
            Second = second; 
        }

        /// <summary>The first</summary>
        public TFirst First { get; }
        /// <summary>The second</summary>
        public TSecond Second { get; }

        /// <summary>Returns the fully qualified type name of this instance.</summary>
        /// <returns>A <see cref="T:System.String" /> containing a fully qualified type name.</returns>
        public override string ToString()
        {
            return $"{First}:{Second}";
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(VTuple<TFirst, TSecond> other)
        {
            return EqualityComparer<TFirst>.Default.Equals(First, other.First)
                && EqualityComparer<TSecond>.Default.Equals(Second, other.Second);
        }

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <returns>true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false.</returns>
        /// <param name="obj">Another object to compare to. </param>
        public override bool Equals(object obj)
        {
            return obj is VTuple<TFirst, TSecond> other && Equals(other);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<TFirst>.Default.GetHashCode(First) * 397) ^ EqualityComparer<TSecond>.Default.GetHashCode(Second);
            }
        }

        /// <summary>Returns a value that indicates whether the values of two <see cref="T:Pawnmorph.Utilities.VTuple`2" /> objects are equal.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(VTuple<TFirst, TSecond> left, VTuple<TFirst, TSecond> right)
        {
            return left.Equals(right);
        }

        /// <summary>Returns a value that indicates whether two <see cref="T:Pawnmorph.Utilities.VTuple`2" /> objects have different values.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(VTuple<TFirst, TSecond> left, VTuple<TFirst, TSecond> right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// simple value tuple implementation 
    /// </summary>
    /// <typeparam name="TFirst">The type of the first.</typeparam>
    /// <typeparam name="TSecond">The type of the second.</typeparam>
    /// <typeparam name="TThird">The type of the third.</typeparam>
    public struct VTuple<TFirst, TSecond, TThird>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VTuple{TFirst, TSecond, TThird}"/> struct.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <param name="third">The third.</param>
        public VTuple(TFirst first, TSecond second, TThird third)
        {
            this.first = first;
            this.second = second;
            this.third = third; 
        }
        /// <summary>The first</summary>
        public TFirst first;
        /// <summary>The second</summary>
        public TSecond second;
        /// <summary>The third</summary>
        public TThird third; 
    }
}