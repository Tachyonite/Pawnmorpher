// RelationshipExtension.cs modified by Iron Wolf for Pawnmorph on 07/30/2019 1:48 PM
// last updated 07/30/2019  1:48 PM

using RimWorld;
using Verse;

namespace Pawnmorph.Thoughts
{
    /// <summary>
    /// extension info to add onto Relationships 
    /// </summary>
    public class RelationshipDefExtension : DefModExtension
    {
        public ThoughtDef transformThought;
        public ThoughtDef transformThoughtFemale;
        public ThoughtDef revertedThought;
        public ThoughtDef revertedThoughtFemale;
        public ThoughtDef permanentlyFeral;
        public ThoughtDef permanentlyFeralFemale; 
        //others? like changed/merged/ect. 
        
    }
}