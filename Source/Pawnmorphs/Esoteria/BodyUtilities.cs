// BodyUtilities.cs created by Iron Wolf for Pawnmorph on 03/16/2020 7:15 PM
// last updated 03/16/2020  7:15 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// a collection of various body related utilities 
    /// </summary>
    [StaticConstructorOnStartup]
    public static class BodyUtilities
    {
        [NotNull] private static readonly Dictionary<BodyPartRecord, PartAddress> _internDict =
            new Dictionary<BodyPartRecord, PartAddress>();


        [NotNull] private static readonly Dictionary<BodyPartGroupDef, BodyPartGroupExtension> _extCache;

        [NotNull] private static readonly Dictionary<BodyDef, List<BodyPartGroupDef>> _groinPartsCache =
            new Dictionary<BodyDef, List<BodyPartGroupDef>>();


        [NotNull]
        private static IReadOnlyList<BodyPartGroupDef> GetGroinPartsFor([NotNull] BodyDef bDef)
        {
            if (_groinPartsCache.TryGetValue(bDef, out List<BodyPartGroupDef> lst)) return lst;

            lst = bDef.AllParts.SelectMany(r => r.groups.MakeSafe()).Where(pg => pg.CountsAsGroin()).Distinct().ToList();
            _groinPartsCache[bDef] = lst;
            return lst;
        }


        static BodyUtilities()
        {
            _extCache = new Dictionary<BodyPartGroupDef, BodyPartGroupExtension>();

            foreach (BodyPartGroupDef bGroup in DefDatabase<BodyPartGroupDef>.AllDefs)
            {
                var ext = bGroup.GetModExtension<BodyPartGroupExtension>();
                if (ext != null) _extCache[bGroup] = ext;
            }
        }


        /// <summary>
        /// Tries to get extra body part group properties on this instance .
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <returns>the extra properties, null if it has none </returns>
        [CanBeNull]
        public static BodyPartGroupExtension TryGetExtraProperties([NotNull] this BodyPartGroupDef def)
        {
            return _extCache.TryGetValue(def); 
        }


        /// <summary>
        /// if this body part group def counts as a groin for nudity checks.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <returns></returns>
        public static bool CountsAsGroin([NotNull] this BodyPartGroupDef def)
        {
            return def == BodyPartGroupDefOf.Legs || def.TryGetExtraProperties()?.countsAsGroin == true; 
        }

        /// <summary>
        /// the different parts of the body that can be considered nude for various checks 
        /// </summary>
        [Flags]
        public enum NudityValues
        {
            /// <summary>
            /// The groin is covered
            /// </summary>
            Groin = 1,
            /// <summary>
            /// The torso is covered 
            /// </summary>
            Torso = 1 << 1,
            /// <summary>
            /// The head is covered 
            /// </summary>
            Head = 1 << 2,

            /// <summary>
            /// The groin or chest
            /// </summary>
            GroinOrChest = Torso | Groin,
            /// <summary>
            /// groin head or face group 
            /// </summary>
            GroinHeadOrFace = Groin | Torso | Head,
            /// <summary>
            /// everything is covered 
            /// </summary>
            All = ~0
        }




        /// <summary>
        /// get what parts are nude on the given pawn.
        /// </summary>
        /// <param name="p">The pawn.</param>
        /// <returns></returns>
        public static NudityValues GetNudityValues(Pawn p)
        {
            Pawn_ApparelTracker apparelTracker = p.apparel;
            if (apparelTracker == null) return 0;
            List<Apparel> wornApparel = apparelTracker.WornApparel;
            NudityValues retVal = 0;
            IReadOnlyList<BodyPartGroupDef> groinList = GetGroinPartsFor(p.RaceProps.body);

            bool eyesCovered=false, upperHeadCovered=false; 

            foreach (Apparel apparel in wornApparel)
            {
                if (p.kindDef.apparelRequired != null && p.kindDef.apparelRequired.Contains(apparel.def)) continue;

                var pgGroups = apparel.def.apparel?.bodyPartGroups; 
                if(pgGroups == null) continue;
                foreach (BodyPartGroupDef apparelGroup in pgGroups)
                {     if (apparelGroup == BodyPartGroupDefOf.Torso)
                        retVal |= NudityValues.Torso;
                    else if (groinList.Contains(apparelGroup)) retVal |= NudityValues.Groin;
                    else if (apparelGroup == BodyPartGroupDefOf.FullHead)
                    {
                        retVal |= NudityValues.Head;
                    }else if ((retVal & NudityValues.Head) == 0)
                    {
                        if (apparelGroup == BodyPartGroupDefOf.Eyes) eyesCovered = true; 
                        else if (apparelGroup == BodyPartGroupDefOf.UpperHead) upperHeadCovered = true;
                        if (eyesCovered && upperHeadCovered) retVal |= NudityValues.Head; 
                    }
                }
                
            }

            return retVal;
        }


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

        /// <summary>
        /// Gets all non missing parts of the given part defs 
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="partDefs">The part defs.</param>
        /// <returns></returns>
        [NotNull]
        public static IEnumerable<BodyPartRecord> GetAllNonMissingParts([NotNull] this Pawn pawn,
                                                                        [NotNull] IEnumerable<BodyPartDef> partDefs)
        {
            var lookup = partDefs.ToList(); 
            foreach (BodyPartRecord record in pawn.health.hediffSet.GetAllNonMissingWithoutProsthetics())
            {
                if (lookup.Contains(record.def))
                {
                    yield return record; 
                }
            }
        }


        /// <summary>
        /// Gets all non missing parts on this pawn
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">pawn</exception>
        [NotNull]
        public static IEnumerable<BodyPartRecord> GetAllNonMissingParts([NotNull] this Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            return (pawn.health?.hediffSet?.GetAllNonMissingWithoutProsthetics()).MakeSafe();
        }

        /// <summary>
        /// Gets all non missing parts of the given part defs 
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="partDefs">The part defs.</param>
        /// <returns></returns>
        [NotNull]
        public static IEnumerable<BodyPartRecord> GetAllNonMissingParts([NotNull] this Pawn pawn,
                                                                        [NotNull] IReadOnlyList<BodyPartDef> partDefs)
        {
           
            foreach (BodyPartRecord record in pawn.health.hediffSet.GetAllNonMissingWithoutProsthetics())
            {
                if (partDefs.Contains(record.def))
                {
                    yield return record;
                }
            }
        }
    }
}