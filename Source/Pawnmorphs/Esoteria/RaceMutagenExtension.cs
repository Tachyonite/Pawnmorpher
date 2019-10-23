// RaceMutagenExtension.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/13/2019 4:10 PM
// last updated 08/13/2019  4:10 PM

using System.Collections.Generic;
using Verse;

namespace Pawnmorph
{
    /// <summary> Extension used to blacklist a race from one or more mutagen strains. </summary>
    public class RaceMutagenExtension : DefModExtension
    {
        /// <summary>if to make this race immune to all mutations</summary>
        public bool immuneToAll;
        /// <summary>a list of mutagens sources this race will be immune to</summary>
        public List<MutagenDef> blackList = new List<MutagenDef>();
    }
}