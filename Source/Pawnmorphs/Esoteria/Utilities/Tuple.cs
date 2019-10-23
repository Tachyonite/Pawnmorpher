// Tuple.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/12/2019 6:00 PM
// last updated 08/12/2019  6:00 PM

namespace Pawnmorph.Utilities
{
    /// <summary>
    /// simple tuple class
    /// </summary>
    /// <typeparam name="T1">The type of the 1.</typeparam>
    /// <typeparam name="T2">The type of the 2.</typeparam>
    public class Tuple<T1,T2>
    {
        /// <summary>Gets the first value in the tuple</summary>
        /// <value>The first.</value>
        public T1 First { get; }
        /// <summary>Gets the second value in the tuple</summary>
        /// <value>The second.</value>
        public T2 Second { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tuple{T1, T2}"/> class.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        public Tuple(T1 first, T2 second)
        {
            First = first;
            Second = second; 
        }
    }
}