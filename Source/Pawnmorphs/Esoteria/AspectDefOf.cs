// AspectDefOf.cs modified by Iron Wolf for Pawnmorph on 09/29/2019 12:58 PM
// last updated 09/29/2019  12:58 PM

using JetBrains.Annotations;
using RimWorld;

namespace Pawnmorph
{
    /// <summary>
    /// DefOf class for commonly referenced Aspects 
    /// </summary>
    [DefOf]
    public static class AspectDefOf
    {



        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public static AspectDef EtherState;

        static AspectDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(AspectDef));
        }

    }
}