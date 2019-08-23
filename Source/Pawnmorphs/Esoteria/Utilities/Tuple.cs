// Tuple.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/12/2019 6:00 PM
// last updated 08/12/2019  6:00 PM

namespace Pawnmorph.Utilities
{
    public class Tuple<T1,T2>
    {
        public T1 First { get; }
        public T2 Second { get; }

        public Tuple(T1 first, T2 second)
        {
            First = first;
            Second = second; 
        }

        
    }
}