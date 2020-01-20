// MutationTracker.cs created by Nick M(Iron Wolf)  Pawnmorph on 09/12/2019 9:11 AM
// last updated 09/12/2019  9:11 AM

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils;

namespace Pawnmorph
{
    /// <summary> Tracker comp for tracking the current influence a pawn has of a given morph. </summary>
    public class MutationTracker : ThingComp, IEnumerable<KeyValuePair<AnimalClassBase, float>>
    {

        [NotNull] private readonly List<Hediff_AddedMutation> _mutationList = new List<Hediff_AddedMutation>();

        [NotNull] private readonly Dictionary<AnimalClassBase, float> _influenceDict = new Dictionary<AnimalClassBase, float>();

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the
        ///     collection.
        /// </returns>
        public IEnumerator<KeyValuePair<AnimalClassBase, float>> GetEnumerator()
        {
            return _influenceDict.GetEnumerator(); 
        }

        /// <summary>
        /// Gets the total normalized non human influence 
        /// </summary>
        /// <value>
        /// The total normalized influence.
        /// </value>
        public float TotalNormalizedInfluence { get; private set; }

        /// <summary>
        /// Gets the non human influence on the pawn.
        /// </summary>
        /// <value>
        /// The total influence.
        /// </value>
        public float TotalInfluence { get; private set; }

        /// <summary>
        ///     Gets the total number of mutations on the pawn being tracked.
        /// </summary>
        /// <value>
        ///     The mutations count.
        /// </value>
        public int MutationsCount { get; private set; }

        /// <summary> Get the current influence associated with the given key. </summary>
        public float this[MorphDef key] => _influenceDict.TryGetValue(key);


        /// <summary> The morph with the most influence on this pawn, not necessarily the morph the pawn currently is. </summary>
        [CanBeNull, Obsolete("use " + nameof(HighestInfluence) + " instead")]
        public MorphDef HighestMorphInfluence { get; private set; }


        /// <summary>
        /// Gets the highest influence on this pawn 
        /// </summary>
        /// <value>
        /// The highest influence.
        /// </value>
        public AnimalClassBase HighestInfluence { get; private set; }

        /// <summary> All mutations the pawn has. </summary>
        [NotNull]
        public IEnumerable<Hediff_AddedMutation> AllMutations =>
            _mutationList.MakeSafe();

        /// <summary>
        ///     Gets the pawn this is tracking mutations for.
        /// </summary>
        /// <value>
        ///     The pawn.
        /// </value>
        public Pawn Pawn => (Pawn) parent;

        /// <summary>
        ///     called every tick
        /// </summary>
        public override void CompTick()
        {
            if (Pawn.IsHashIntervalTick(1400) && !Pawn.health.hediffSet.hediffs.OfType<Hediff_Morph>().Any())
                Pawn.CheckRace(false); //check the race every so often, but not too often 
        }

        /// <summary>
        ///     Gets the normalized direct influence of the given morph
        /// </summary>
        /// this does not take into account influence the children of the given class might have on the pawn
        /// <param name="class">The morph.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">morph</exception>
        public float GetDirectNormalizedInfluence([NotNull] AnimalClassBase @class)
        {
            if (@class == null) throw new ArgumentNullException(nameof(@class));
            return _influenceDict.TryGetValue(@class) / MorphUtilities.MaxHumanInfluence;
        }

        /// <summary>
        ///     Initializes this instance with given props.
        /// </summary>
        /// this is call just after it is added to the parent, so other comps may or may not be added yet
        /// <param name="props">The props.</param>
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Assert(parent is Pawn, "parent is Pawn");
        }

        /// <summary> Called to notify this tracker that a mutation has been added. </summary>
        public void NotifyMutationAdded([NotNull] Hediff_AddedMutation mutation)
        {
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));
            _mutationList.Add(mutation);

            AnimalClassUtilities.FillInfluenceDict(_mutationList, _influenceDict);
            TotalNormalizedInfluence = _influenceDict.Select(s => s.Value).Sum() / MorphUtilities.MaxHumanInfluence;
            TotalInfluence = _influenceDict.Select(s => s.Value).Sum();
            MutationsCount += 1;

#pragma warning disable 0612
#pragma warning disable 0618

            HighestMorphInfluence = GetHighestMorphInfluence();
#pragma warning restore 0618
#pragma warning restore 0612
            NotifyCompsAdded(mutation);
        }

        /// <summary> Called to notify this tracker that a mutation has been removed. </summary>
        public void NotifyMutationRemoved([NotNull] Hediff_AddedMutation mutation)
        {
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));
            _mutationList.Remove(mutation);
            MutationsCount -= 1;

            AnimalClassUtilities.FillInfluenceDict(_mutationList, _influenceDict);
#pragma warning disable 0612
#pragma warning disable 0618
            TotalNormalizedInfluence = _influenceDict.Select(s => s.Value).Sum() / MorphUtilities.MaxHumanInfluence;
            TotalInfluence = _influenceDict.Select(s => s.Value).Sum(); 
            HighestMorphInfluence = GetHighestMorphInfluence();
            HighestInfluence = GetHighestInfluence(); 
            NotifyCompsRemoved(mutation);
#pragma warning restore 0618
#pragma warning restore 0612
        }

        private AnimalClassBase GetHighestInfluence()
        {
            float max = 0;
            AnimalClassBase influence = null; 
            foreach (KeyValuePair<AnimalClassBase, float> keyValuePair in _influenceDict)
            {
                if (keyValuePair.Value > max)
                {
                    influence = keyValuePair.Key;
                    max = keyValuePair.Value; 
                }
            }

            return influence; 
        }

        /// <summary>
        ///     exposes this instances data after the parent.
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                // Generate lookup dict manually during load for backwards compatibility.

                RecalculateCaches();
        }

        [Obsolete]
        private MorphDef GetHighestMorphInfluence()
        {
            MorphDef hMorph = null;
            float max = float.NegativeInfinity;
            foreach (KeyValuePair<AnimalClassBase, float> keyValuePair in _influenceDict)
                if (max < keyValuePair.Value)
                {
                    hMorph = keyValuePair.Key as MorphDef;
                    max = keyValuePair.Value;
                }

            return hMorph;
        }

        private void NotifyCompsAdded(Hediff_AddedMutation mutation)
        {
            foreach (ThingComp parentAllComp in parent.AllComps)
            {
                if (parentAllComp == this) continue;
                if (!(parentAllComp is IMutationEventReceiver receiver)) continue;
                receiver.MutationAdded(mutation, this);
            }
        }

        private void NotifyCompsRemoved(Hediff_AddedMutation mutation)
        {
            foreach (ThingComp parentAllComp in parent.AllComps)
            {
                if (parentAllComp == this) continue;
                if (!(parentAllComp is IMutationEventReceiver receiver)) continue;
                receiver.MutationRemoved(mutation, this);
            }
        }

        private void RecalculateCaches()
        {
            foreach (Hediff_AddedMutation mutation in Pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>())
                _mutationList.Add(mutation);

            AnimalClassUtilities.FillInfluenceDict(_mutationList, _influenceDict);
            TotalNormalizedInfluence = _influenceDict.Select(s => s.Value).Sum() / MorphUtilities.MaxHumanInfluence;
            // Now find the highest influence.
#pragma warning disable 0612
#pragma warning disable 0618

            MorphDef hMorph = GetHighestMorphInfluence();
            TotalInfluence = _influenceDict.Select(s => s.Value).Sum();
            HighestMorphInfluence = hMorph;
#pragma warning restore 0612
#pragma warning restore 0618

            MutationsCount = Pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>().Count();
        }
    }
}