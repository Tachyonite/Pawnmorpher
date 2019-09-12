// MutationTracker.cs created by Nick M(Iron Wolf)  Pawnmorph on 09/12/2019 9:11 AM
// last updated 09/12/2019  9:11 AM

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils; 
namespace Pawnmorph
{
    /// <summary>
    /// tracker comp for tracking the current influence a pawn has of a given morph 
    /// </summary>
    public class MutationTracker : ThingComp, IEnumerable<KeyValuePair<MorphDef, float>>
    {
        public delegate void MutationChangeHandle(MutationTracker sender, [NotNull] Pawn pawn,
            Hediff_AddedMutation mutation);


        /// <summary>
        ///     event raised whenever a mutation is added
        /// </summary>
        public event MutationChangeHandle MutationAdded;

        /// <summary>
        ///     event raised whenever a mutation is removed
        /// </summary>
        public event MutationChangeHandle MutationRemoved;


        private Dictionary<MorphDef, float> _influenceLookup = new Dictionary<MorphDef,float>();

        /// <summary>
        /// get the current influence associated with the given key 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public float this[MorphDef key] => _influenceLookup.TryGetValue(key);



        /// <summary>
        /// all mutations the pawn has 
        /// </summary>
        public IEnumerable<Hediff_AddedMutation> AllMutations =>
            Pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>();  

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Assert(parent is Pawn, "parent is Pawn"); 
        }

        public Pawn Pawn => (Pawn) parent; 

        public void NotifyMutationAdded([NotNull] Hediff_AddedMutation mutation)
        {
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));

            var comp = mutation.TryGetComp<Comp_MorphInfluence>();
            if (comp != null)
            {
                _influenceLookup[comp.Morph] = _influenceLookup.TryGetValue(comp.Morph) + comp.Influence; 


            }

            MutationAdded?.Invoke(this, Pawn,  mutation);
        }

        public void NotifyMutationRemoved([NotNull] Hediff_AddedMutation mutation)
        {
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));

            var comp = mutation.TryGetComp<Comp_MorphInfluence>();
            if (comp != null)
            {
                Assert(_influenceLookup.ContainsKey(comp.Morph), "_influenceLookup.ContainsKey(comp.Morph)");

                _influenceLookup[comp.Morph] -= comp.Influence; 

            }

            MutationRemoved?.Invoke(this, Pawn, mutation); 
        }

        public override void PostExposeData()
        {
            base.PostExposeData();


            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                //generate lookup dict manually during load for backwards compatibility

                foreach (var comp in AllMutations.Select(mut => mut.TryGetComp<Comp_MorphInfluence>()))
                {
                  if(comp == null) continue;

                  var morph = comp.Morph;
                  _influenceLookup[morph] = _influenceLookup.TryGetValue(morph) + comp.Influence; 

                }


            }
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<MorphDef, float>> GetEnumerator()
        {
            return _influenceLookup.GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _influenceLookup).GetEnumerator();
        }
    }
}