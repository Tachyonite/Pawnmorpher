// BodyUtilities.cs created by Iron Wolf for Pawnmorph on 03/16/2020 7:15 PM
// last updated 03/16/2020  7:15 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// a collection of various body related utilities 
    /// </summary>
    public static class BodyUtilities
    {
        [NotNull] private static readonly Dictionary<BodyPartRecord, PartAddress> _internDict =
            new Dictionary<BodyPartRecord, PartAddress>();

        /// <summary>
        ///     Gets the part address for this body part record.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">record</exception>
        [NotNull]
        public static PartAddress GetAddress([NotNull] this BodyPartRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            //save this so we don't have to do unneeded calculations, part records should never change
            if (_internDict.TryGetValue(record, out PartAddress addr)) return addr;


            var lst = new List<string>();
            BodyPartRecord node = record;
            while (node != null)
            {
                lst.Add(node.Label);
                node = node.parent;
            }

            lst.Reverse();
            return new PartAddress(lst);
        }


        /// <summary>
        ///     Gets the body part record at the given part address
        /// </summary>
        /// <param name="bodyDef">The body definition.</param>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     bodyDef
        ///     or
        ///     address
        /// </exception>
        [CanBeNull]
        public static BodyPartRecord GetRecordAt([NotNull] this BodyDef bodyDef, [NotNull] PartAddress address)
        {
            if (bodyDef == null) throw new ArgumentNullException(nameof(bodyDef));
            if (address == null) throw new ArgumentNullException(nameof(address));

            if (address.Count == 0) return null;
            BodyPartRecord curRecord = bodyDef.corePart;
            if (curRecord.Label != address[0]) return null;

            for (var i = 1; i < address.Count; i++)
            {
                curRecord = curRecord.parts.MakeSafe().FirstOrDefault(p => p.Label == address[i]);
                if (curRecord == null) break;
            }

            return curRecord;
        }

    }
}