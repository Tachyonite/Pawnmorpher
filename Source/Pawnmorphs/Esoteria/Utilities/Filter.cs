﻿// Filter.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 09/11/2019 2:56 PM
// last updated 09/11/2019  2:56 PM

using System.Collections.Generic;

namespace Pawnmorph.Utilities
{
    /// <summary> Generic class for a filter. </summary>
    public class Filter<T>
    {
        /// <summary>
        /// the list of entries in the filter 
        /// </summary>
        public List<T> filterList;
        /// <summary>
        /// if this filter is a black list 
        /// </summary>
        public bool isBlackList = true;

        /// <summary>
        /// returns true if the given element passes through the filter 
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        public bool PassesFilter(T elem)
        {
            var contains = filterList?.Contains(elem) ?? false;
            if (isBlackList) return !contains;
            return contains; 
        }
    }
}
