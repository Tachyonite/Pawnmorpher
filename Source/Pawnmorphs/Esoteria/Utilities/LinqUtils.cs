// LinqUtils.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/13/2019 6:50 AM
// last updated 08/13/2019  6:50 AM

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Utilities
{
    public static class LinqUtils
    {
        public static T RandElement<T>([NotNull] this IList<T> lst, T defaultVal = default(T))
        {
            if (lst == null) throw new ArgumentNullException(nameof(lst));
            if (lst.Count == 0)
            {
                return defaultVal; 
            }

            if (lst.Count == 1) return lst[0]; 

            return lst[ Rand.Range(0, lst.Count)]; 

        }


        //in        
        /// <summary>
        /// check if this instance is in the given collection 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="thing">The thing.</param>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public static bool In<T>(this T thing, T other) //explicit overloads for 1,2,3 make In a bit faster 
        {
            var equals = EqualityComparer<T>.Default;
            return equals.Equals(thing, other); 
        }


        /// <summary>
        /// check if this instance is in the given collection 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="thing">The thing.</param>
        /// <param name="other1">The other1.</param>
        /// <param name="other2">The other2.</param>
        /// <returns></returns>
        public static bool In<T>(this T thing, T other1, T other2)
        {
            var equals = EqualityComparer<T>.Default;
            return equals.Equals(thing, other1) || equals.Equals(thing, other2);
        }

        /// <summary>
        /// check if this instance is in the given collection 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="thing">The thing.</param>
        /// <param name="other1">The other1.</param>
        /// <param name="other2">The other2.</param>
        /// <param name="other3">The other3.</param>
        /// <returns></returns>
        public static bool In<T>(this T thing, T other1, T other2, T other3)
        {
            var equals = EqualityComparer<T>.Default;
            return equals.Equals(thing, other1) || equals.Equals(thing, other2) || equals.Equals(thing, other3); 
        }

        //fallthrough     

        /// <summary>
        /// check if this instance is in the given collection 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="thing">The thing.</param>
        /// <param name="others">The others.</param>
        /// <returns></returns>
        public static bool In<T>(this T thing, params T[] others)
        {
            return others.Contains(thing); 
        }

    }
}