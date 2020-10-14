// MainTabWindow_ChamberDatabase.Interning.cs created by Iron Wolf for Pawnmorph on //2020 
// last updated 10/13/2020  7:45 AM

using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph
{
    public partial class MainTabWindow_ChamberDatabase
    {
        static MainTabWindow_ChamberDatabase()
        {
            _cachedHeaders = new RowHeader[2];
            


            //setup the headers just once 
            string totalHeader = "PMGenebankTotalColumnHeader".Translate();
            string removeHeader = "PMGenebankRemoveColumnHeader".Translate();
            
            _cachedHeaders[(int)Mode.Animal]  = new RowHeader("PMGeneBankAnimalDefHeader".Translate(), totalHeader, removeHeader);
            _cachedHeaders[(int)Mode.Mutations] = new RowHeader("PMGenebankMutationDefInfoColumnHeader".Translate(), totalHeader, removeHeader);


        }

        readonly struct RowHeader
        {
            public readonly string defHeader;
            public readonly string totalHeader;
            public readonly string removeHeader;

            public RowHeader(string defHeader, string totalHeader, string removeHeader)
            {
                this.defHeader = defHeader;
                this.totalHeader = totalHeader;
                this.removeHeader = removeHeader;
            }
        }


        [NotNull]
        private static readonly Dictionary<RowEntry, string> _internDict = new Dictionary<RowEntry, string>();

        [NotNull] private static readonly RowHeader[] _cachedHeaders;

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