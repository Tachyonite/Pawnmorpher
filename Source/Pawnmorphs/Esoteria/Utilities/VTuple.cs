// VTuple.cs created by Iron Wolf for Pawnmorph on 09/12/2019 1:00 PM
// last updated 09/12/2019  1:00 PM

namespace Pawnmorph.Utilities
{
    //value tuples 
    /// <summary>
    /// simple value tuple implementation 
    /// </summary>
    /// <typeparam name="TFirst">The type of the first.</typeparam>
    /// <typeparam name="TSecond">The type of the second.</typeparam>
    public struct VTuple<TFirst, TSecond>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VTuple{TFirst, TSecond}"/> struct.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        public VTuple(TFirst first, TSecond second)
        {
            this.first = first;
            this.second = second; 
        }

        /// <summary>The first</summary>
        public TFirst first;
        /// <summary>The second</summary>
        public TSecond second;

        /// <summary>Returns the fully qualified type name of this instance.</summary>
        /// <returns>A <see cref="T:System.String" /> containing a fully qualified type name.</returns>
        public override string ToString()
        {
            return $"{first}:{second}";
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