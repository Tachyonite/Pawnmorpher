// PMImplicitDefGenerator.cs created by Iron Wolf for Pawnmorph on 10/09/2021 9:59 AM
// last updated 10/09/2021  9:59 AM

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// static class that generates all implicit defs in the mod 
    /// </summary>
    public static class PMImplicitDefGenerator
    {
        private static readonly MethodInfo GiveHashMethod;
        
        
        static PMImplicitDefGenerator()
        {
            GiveHashMethod = typeof(ShortHashGiver).GetMethod("GiveShortHash", BindingFlags.NonPublic | BindingFlags.Static);
        }

        [NotNull]
        private static readonly object[] tmpArr = new object[2]; 
        static void GiveShortHash([NotNull] Def def, [NotNull] System.Type type)
        {
            tmpArr[0] = def;
            tmpArr[1] = type;
            GiveHashMethod.Invoke(null, tmpArr); 
        }
    }
}