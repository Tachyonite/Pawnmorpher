// ThoughtDefs.cs modified by Iron Wolf for Pawnmorph on 07/31/2019 4:39 PM
// last updated 07/31/2019  4:39 PM

using RimWorld;
#pragma warning disable 1591
namespace Pawnmorph.Thoughts
{
    /// <summary>
    /// static class containing default/frequently used thoughtDefs 
    /// </summary>
    [DefOf]
    public static class ThoughtDefOfs
    {
        static ThoughtDefOfs()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThoughtDef));
        }

        //transformed 
        public static ThoughtDef ColonistTransformedThought;
        public static ThoughtDef PrisonerTransformedThought;
        public static ThoughtDef RivalTransformedThought;
        public static ThoughtDef FriendTransformedThought; 

        //reverted 
        public static ThoughtDef ColonistRevertedThought;
        public static ThoughtDef RivalRevertedThought;
        public static ThoughtDef FriendRevertedThought; 
        
        //permanently feral reactions
        public static ThoughtDef ColonistPermFeralThought;
        public static ThoughtDef FriendPermFeralThought;
        public static ThoughtDef RivalPermFeralThought; 

        //merging 
        public static ThoughtDef ColonistMergedThought;
        public static ThoughtDef PrisonerMergedThought;
        public static ThoughtDef FriendMergedThought;
        public static ThoughtDef RivalMergedThought; 

        
    }
}