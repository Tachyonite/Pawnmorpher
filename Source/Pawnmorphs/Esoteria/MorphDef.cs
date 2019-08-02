// MorphDef.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 2:32 PM
// last updated 08/02/2019  2:32 PM

using System.Collections.Generic;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// def class for a 'morph' 
    /// </summary>
    public class MorphDef : Def
    {
        /// <summary>
        /// all categories the morph belongs to (canid, carnivore, ect) 
        /// </summary>
        public List<string> categories = new List<string>(); 
        public ThingDef race; //the animal race of the morph 
    }
}