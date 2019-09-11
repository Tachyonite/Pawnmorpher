// Filter.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 09/11/2019 2:56 PM
// last updated 09/11/2019  2:56 PM

using System.Collections.Generic;

namespace Pawnmorph.Utilities
{
    /// <summary>
    /// generic class for a filter 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Filter<T>
    {
        public List<T> filterList;
        public bool isBlackList;

        public bool PassesFilter(T elem)
        {
            var contains = filterList?.Contains(elem) ?? false;
            if (isBlackList) return !contains;
            return contains; 
        }

    }
}