// MutationTracker.cs created by Nick M(Iron Wolf)  Pawnmorph on 09/12/2019 9:11 AM
// last updated 09/12/2019  9:11 AM

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using UnityEngine;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils; 
namespace Pawnmorph
{
    /// <summary>
    /// tracker comp for tracking the current influence a pawn has of a given morph 
    /// </summary>
    public class MutationTracker : ThingComp, IEnumerable<KeyValuePair<MorphDef, float>>
    {



        private Dictionary<MorphDef, float> _influenceLookup = new Dictionary<MorphDef, float>();


        /// <summary>
        /// get the current influence associated with the given key 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public float this[MorphDef key] => _influenceLookup.TryGetValue(key);

        /// <summary>
        /// an enumerable collection of influences normalized against each other and the remaining human influence 
        /// </summary>
        public IEnumerable<VTuple<MorphDef, float>> NormalizedInfluences
        {
            get
            {
                float accum = 0;
                foreach (KeyValuePair<MorphDef, float> keyValuePair in _influenceLookup)
                {
                    accum += keyValuePair.Value / keyValuePair.Key.TotalInfluence; //use the normalized value so we're comparing apples to apples 
                                                                                //some morphs have more unique parts then others after all 
                }

                accum += Pawn.GetHumanInfluence(true) * MorphUtilities.HUMAN_CHANGE_FACTOR;  //add in the remaining human influence, if any


                foreach (KeyValuePair<MorphDef, float> keyValuePair in _influenceLookup)
                {
                    float nVal;
                    if (accum < 0.0001f) nVal = 0; //prevent division by zero 
                    else
                        nVal = keyValuePair.Value / (accum * keyValuePair.Key.TotalInfluence); //now normalize the morph influences against the total 
                    yield return new VTuple<MorphDef, float>(keyValuePair.Key, nVal);  
                }

            }
        }

      


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

        public float GetNormalizedInfluence([NotNull] MorphDef morph)
        {
            if (morph == null) throw new ArgumentNullException(nameof(morph));
            return this[morph] / Mathf.Max(0.001f, morph.TotalInfluence); //prevent division by zero 
        }

        public Pawn Pawn => (Pawn) parent;

        /// <summary>
        /// called to notify this tracker that a mutation has been added 
        /// </summary>
            /// <param name="mutation"></param>
        public void NotifyMutationAdded([NotNull] Hediff_AddedMutation mutation)
        {
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));

            var comp = mutation.TryGetComp<Comp_MorphInfluence>();
            if (comp != null)
            {
                _influenceLookup[comp.Morph] = _influenceLookup.TryGetValue(comp.Morph) + comp.Influence; 


            }

            NotifyCompsAdded(mutation);
        }

        void NotifyCompsAdded(Hediff_AddedMutation mutation)
        {

            foreach (ThingComp parentAllComp in parent.AllComps)
            {
                if(parentAllComp == this) continue;
                if(!(parentAllComp is IMutationEventReceiver receiver)) continue;
                receiver.MutationAdded(mutation, this); 
            }

        }

        void NotifyCompsRemoved(Hediff_AddedMutation mutation)
        {
            foreach (ThingComp parentAllComp in parent.AllComps)
            {
                if (parentAllComp == this) continue;
                if (!(parentAllComp is IMutationEventReceiver receiver)) continue;
                receiver.MutationRemoved(mutation, this); 
            }
        }


        /// <summary>
        /// called to notify this tracker that a mutation has been removed 
        /// </summary>
        /// <param name="mutation"></param>
        public void NotifyMutationRemoved([NotNull] Hediff_AddedMutation mutation)
        {
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));

            var comp = mutation.TryGetComp<Comp_MorphInfluence>();
            if (comp != null)
            {
                if (!_influenceLookup.ContainsKey(comp.Morph)) return;  
                var val = _influenceLookup[comp.Morph] - comp.Influence;
                if (Mathf.Abs(val) < 0.1f)
                {
                    _influenceLookup.Remove(comp.Morph); 
                }
                else
                {
                    _influenceLookup[comp.Morph] = val; 
                }

            }

            NotifyCompsRemoved(mutation); 
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