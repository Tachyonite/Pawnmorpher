// RaceMutagenExtension.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/13/2019 4:10 PM
// last updated 08/13/2019  4:10 PM

using System.Collections.Generic;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// extension used to blacklist a race from one or more mutagen strains 
    /// </summary>
    public class RaceMutagenExtension : DefModExtension
    {
        public bool immuneToAll;
        public List<MutagenDef> blackList = new List<MutagenDef>(); 
    }
}