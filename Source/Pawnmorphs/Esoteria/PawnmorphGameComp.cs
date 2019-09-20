// PawnmorphGameComp.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/07/2019 2:13 PM
// last updated 08/14/2019  7:01 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.TfSys;
using Pawnmorph.Utilities;
using RimWorld.Planet;
using Verse;

namespace Pawnmorph
{
    public class PawnmorphGameComp : WorldComponent
    {
        [Obsolete]
        public HashSet<PawnMorphInstance> pawnmorphs = new HashSet<PawnMorphInstance>();

        [Obsolete]
        public HashSet<PawnMorphInstanceMerged>
            mergedpawnmorphs = new HashSet<PawnMorphInstanceMerged>(); //why are we using hashsets? 

        
        public HashSet<PawnKindDef> taggedAnimals = new HashSet<PawnKindDef>();

        private List<TransformedPawn> _transformedPawns = new List<TransformedPawn>();

        public PawnmorphGameComp(World world) : base(world)
        {
        }

        private List<TransformedPawn> TransformedPawnsLst //scribe can set _transformedPawns to null in an old save 
        {
            get
            {
                if (_transformedPawns == null) _transformedPawns = new List<TransformedPawn>();

                return _transformedPawns;
            }
        }

        [Obsolete]
        public IEnumerable<PawnMorphInstance> MorphInstances => pawnmorphs;
        [Obsolete]
        public IEnumerable<PawnMorphInstanceMerged> MergeInstances => mergedpawnmorphs;


        public IEnumerable<TransformedPawn> TransformedPawns => TransformedPawnsLst;

        [Obsolete("use AddTransformedPawn instead")]
        public void addPawn(PawnMorphInstance pm)
        {
            pawnmorphs.Add(pm);
        }

        [Obsolete("use AddTransformedPawn instead")]
        public void addPawnMerged(PawnMorphInstanceMerged pmm)
        {
            mergedpawnmorphs.Add(pmm);
        }

        public void AddTransformedPawn([NotNull] TransformedPawn tfPair)
        {
            if (tfPair == null) throw new ArgumentNullException(nameof(tfPair));
            if (!tfPair.IsValid)
            {
                Log.Error($"tried to add invalid transformed pawn! {tfPair.ToDebugString()}\n");
                return;
            }

            _transformedPawns.Add(tfPair);
        }
#pragma warning disable 612

        public override void ExposeData()
        {
           


            Scribe_Collections.Look(ref pawnmorphs, "pawnmorphs", LookMode.Deep);
            Scribe_Collections.Look(ref mergedpawnmorphs, "pawnmorphs", LookMode.Deep);
            Scribe_Collections.Look(ref taggedAnimals, "taggedAnimals");
            Scribe_Collections.Look(ref _transformedPawns, "transformedPawns", LookMode.Deep);
            taggedAnimals = taggedAnimals ?? new HashSet<PawnKindDef>();
            mergedpawnmorphs = mergedpawnmorphs ?? new HashSet<PawnMorphInstanceMerged>();
            pawnmorphs = pawnmorphs ?? new HashSet<PawnMorphInstance>();
            _transformedPawns =
                _transformedPawns ??
                new List<TransformedPawn>(); //Scribe can set the references to null if it's an old save 


          


           

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                //transfer the old ones to the new system 
                foreach (var pawnMorphInstance in pawnmorphs)
                {
                    _transformedPawns.Add(TfSys.TransformedPawn.Create(pawnMorphInstance));
                }

                foreach (var pawnMorphInstanceMerged in mergedpawnmorphs)
                {
                    _transformedPawns.Add(TfSys.TransformedPawn.Create(pawnMorphInstanceMerged));
                }

                //now clear 


                pawnmorphs.Clear();
                mergedpawnmorphs.Clear();

                //make sure they're all valid 
                ValidateTransformedPawns();
            }

        }
#pragma warning restore 612

        /// <summary>
        /// Validates the transformed pawns.
        /// </summary>
        void ValidateTransformedPawns()
        {

            StringBuilder builder = new StringBuilder(); 
            for (int i = TransformedPawnsLst.Count - 1; i >= 0; i--)
            {
                var inst = TransformedPawnsLst[i];
                if (!inst.IsValid)
                {
                    builder.AppendLine($"encountered invalid transformed pawn instance: {inst}");
                    foreach (var pawn in inst.OriginalPawns.Where(p => !p.DestroyedOrNull()))
                    {
                        if(!pawn.Spawned)
                            pawn.Destroy();
                    }

                    TransformedPawnsLst.RemoveAt(i);
                }
            }

            if (builder.Length > 0)
            {
                Log.Error($"encountered invalid transformed pawns instances:\n{builder}");
            }

            


        }

        [Obsolete("use " + nameof(GetTransformedPawnContaining) + " instead")]
        [CanBeNull]
        public PawnMorphInstance GetInstanceWithOriginal(Pawn original)
        {
            return pawnmorphs.FirstOrDefault(i => i.origin == original);
        }

        [Obsolete("use " + nameof(GetTransformedPawnContaining) + " instead")]
        [CanBeNull]
        public PawnMorphInstanceMerged GetMergeInstanceWithOriginal(Pawn original)
        {
            return mergedpawnmorphs.FirstOrDefault(i => i.origin == original || i.origin2 == original);
        }

        /// <summary>
        ///     Gets the pawn transformation status.
        /// </summary>
        /// <param name="p">The pawn.</param>
        /// <returns></returns>
        public TransformedStatus? GetPawnStatus(Pawn p)
        {
            foreach (var transformedPawn in TransformedPawnsLst)
            {
                var status = transformedPawn.GetStatus(p);
                if (status != null) return status;
            }

            return null;
        }

        /// <summary>
        ///     Gets the transformed pawn containing the given pawn
        /// </summary>
        /// <param name="pawn">The pawn</param>
        /// <returns>the transformedPawn instance as well as the pawn's status to that instance</returns>
        [CanBeNull]
        public Tuple<TransformedPawn, TransformedStatus> GetTransformedPawnContaining(Pawn pawn)
        {
            foreach (var transformedPawn in TransformedPawnsLst)
            {
                var status = transformedPawn.GetStatus(pawn);
                if (status != null) return new Tuple<TransformedPawn, TransformedStatus>(transformedPawn, status.Value);
            }

            return null;
        }

        /// <summary>
        ///     Removes the transformed instance from the list.
        /// </summary>
        /// <param name="tfPawn">The tf pawn.</param>
        public void RemoveInstance(TransformedPawn tfPawn)
        {
            TransformedPawnsLst.Remove(tfPawn);
        }

        [Obsolete("use " + nameof(RemoveInstance) + " instead")]
        public void removePawn(PawnMorphInstance pm)
        {
            pawnmorphs.Remove(pm);
        }

        [Obsolete("use " + nameof(RemoveInstance) + " instead")]
        public void removePawnMerged(PawnMorphInstanceMerged pmm)
        {
            mergedpawnmorphs.Remove(pmm);
        }

        [Obsolete("use " + nameof(GetTransformedPawnContaining) + " instead")]
        public PawnMorphInstance retrieve(Pawn animal)
        {
            var pm = pawnmorphs.FirstOrDefault(instance => instance.replacement == animal);
            return pm;
        }

        [Obsolete("use " + nameof(GetTransformedPawnContaining) + " instead")]
        public PawnMorphInstanceMerged retrieveMerged(Pawn animal)
        {
            var pm = mergedpawnmorphs.FirstOrDefault(instance => instance.replacement == animal);
            return pm;
        }

        public void tagPawn(PawnKindDef pawnkind)
        {
            if (!taggedAnimals.Contains(pawnkind)) taggedAnimals.Add(pawnkind);
        }
    }
}