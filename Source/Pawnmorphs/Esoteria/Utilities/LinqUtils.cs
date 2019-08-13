// LinqUtils.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/13/2019 6:50 AM
// last updated 08/13/2019  6:50 AM

using System;
using System.Collections;
using System.Collections.Generic;
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
    }
}