// MorphDefOfs.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 2:46 PM
// last updated 08/02/2019  2:46 PM

using RimWorld;
#pragma warning disable 1591
namespace Pawnmorph
{
    [DefOf]
    public static class MorphDefOfs
    {
        public static MorphDef WolfMorph;
        public static MorphDef WargMorph;
        public static MorphDef FoxMorph;
        public static MorphDef HuskyMorph;
        
        static MorphDefOfs()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MorphDef)); 
        }
    }
}