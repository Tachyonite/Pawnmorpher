// MainTabWindow_ChamberDatabase.Interning.cs created by Iron Wolf for Pawnmorph on //2020 
// last updated 10/13/2020  7:45 AM

using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace Pawnmorph
{
    public partial class MainTabWindow_ChamberDatabase
    {
        [NotNull]
        private static readonly Dictionary<RowEntry, string> _internDict = new Dictionary<RowEntry, string>();


        
        static string GetDescriptionStringFor(RowEntry rEntry)
        {
            if (_internDict.TryGetValue(rEntry, out string val))
            {
                return val;
                
            }

            _internDict[rEntry] = rEntry.label + " : " + rEntry.storageSpaceUsed; //only calculate this once 
            return _internDict[rEntry]; 
        }

    }
}