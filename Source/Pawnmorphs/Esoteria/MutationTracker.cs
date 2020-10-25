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
    /// <summary> Tracker comp for tracking the current influence a pawn has of a given morph. </summary>
    public class MutationTracker : ThingComp, IEnumerable<KeyValuePair<AnimalClassBase, float>>, IRaceChangeEventReceiver
    {

        [NotNull] private readonly List<Hediff_AddedMutation> _mutationList = new List<Hediff_AddedMutation>();

        [NotNull] private readonly Dictionary<AnimalClassBase, float> _influenceDict = new Dictionary<AnimalClassBase, float>();

        /// <summary>
        /// Gets or sets a value indicating whether debug messages are enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if debug messages are enabled; otherwise, <c>false</c>.
        /// </value>
        public bool DebugMessagesEnabled { get; set; }


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
        public float TotalNormalizedInfluence => TotalInfluence / MorphUtilities.GetMaxInfluenceOfRace(parent.def);  

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




        /// <summary>
        /// Gets the highest influence on this pawn 
        /// </summary>
        /// <value>
        /// The highest influence.
        /// </value>
        public AnimalClassBase HighestInfluence
        {
            get
            {
                AnimalClassBase h = null;
                float max = float.NegativeInfinity;
                foreach (KeyValuePair<AnimalClassBase, float> keyValuePair in _influenceDict)
                    if (max < keyValuePair.Value)
                    {
                        h = keyValuePair.Key;
                        max = keyValuePair.Value;
                    }

                return h;
            }
        }

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
            if (!MutagenDefOf.defaultMutagen.CanInfect(Pawn)) return; //tracker is added on some kinds of pawns that can't get mutations, like mechanoids 
            if (InfluencesDirty)
            {
                RecalcInfluences();
                InfluencesDirty = false; 
            }

            if (Pawn.IsHashIntervalTick(MutationRuleDef.CHECK_RATE))
            {
                MutationRuleUtilities.TryExecuteRulesOn(Pawn); 
            }


            if (Pawn.IsHashIntervalTick(4400) && CanRaceCheckNow)
            {
                CheckPawnRace();

            }

        }

        /// <summary>
        /// Checks if the pawn's race should change .
        /// </summary>
        /// <returns></returns>
        public bool CheckPawnRace()
        {
            RecalcIfNeeded();
            if (TotalNormalizedInfluence < 0 || TotalNormalizedInfluence > 1)
            {
                Log.Warning(nameof(TotalNormalizedInfluence) + $" is {TotalNormalizedInfluence}, recalculating mutation influences for {Pawn.Name}");
                RecalculateMutationInfluences();
            }

            var oldRace = Pawn.def; 

            Pawn.CheckRace(false); //check the race every so often, but not too often 
            return oldRace != Pawn.def; 
        }

        private bool CanRaceCheckNow => !Pawn.health.hediffSet.hediffs.OfType<TransformationBase>().Any(t => t.BlocksRaceCheck); 

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
            return _influenceDict.TryGetValue(@class) / MorphUtilities.GetMaxInfluenceOfRace(parent.def); 
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

        private const float EPSILON = 0.01f;

        /// <summary>
        /// called just before spawning in the pawn 
        /// </summary>
        /// <param name="respawningAfterLoad">if set to <c>true</c> [respawning after load].</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (InfluencesDirty)
            {
                RecalcInfluences();
                InfluencesDirty = false; 
            }
        }

        /// <summary>
        /// Recalculates the mutation influences if needed.
        /// </summary>
        public void RecalcIfNeeded()
        {
            int counter=0; 
            foreach (Hediff hediff in Pawn.health.hediffSet.hediffs)
            {
                if (hediff is Hediff_AddedMutation) counter++; 
            }

            if (counter != MutationsCount)
            {
                Log.Warning($"{Pawn.Name} mutation tracker has only {MutationsCount} tracked mutations but pawn actually has {counter}");
                RecalculateMutationInfluences();
                return; 
            }

            bool anyNegative=false; 
            float totalInfluence=0; 
            foreach (KeyValuePair<AnimalClassBase, float> keyValuePair in _influenceDict)
            {
                if (keyValuePair.Value < 0) anyNegative = true; 
                totalInfluence += keyValuePair.Value; 
            }

            var maxInfluence = MorphUtilities.GetMaxInfluenceOfRace(parent.def); 
            if (anyNegative)
            {
                Log.Warning($"{Pawn.Name} has negative mutation influence");
                RecalculateMutationInfluences();
            }
            if (Mathf.Abs(totalInfluence - TotalInfluence) > EPSILON)
            {
                Log.Warning($"{Pawn.Name} mutation tracker total is incorrect calculated:{totalInfluence} vs cached:{TotalInfluence}");
                RecalculateMutationInfluences();
            }
            else if (totalInfluence > maxInfluence)
            {
                Log.Warning($"{Pawn.Name} mutation tracker total is incorrect calculated:{totalInfluence}  greater then max:{maxInfluence}");
                RecalculateMutationInfluences();
            }

        }

        /// <summary>
        ///     preforms a full recalculation of all mutation influences
        /// </summary>
        public void RecalculateMutationInfluences()
        {
            _mutationList.Clear();
            _influenceDict.Clear();

            _mutationList.AddRange(Pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>());
            MutationsCount = _mutationList.Count;
            AnimalClassUtilities.FillInfluenceDict(_mutationList, _influenceDict);
            TotalInfluence = _influenceDict.Sum(s => s.Value);

        }

        /// <summary>
        /// Gets or sets a value indicating whether the morph influences caches are dirty.
        /// if true the influences will be recalculated on the next tick 
        /// </summary>
        /// <value>
        ///   <c>true</c> if [influences dirty]; otherwise, <c>false</c>.
        /// </value>
        public bool InfluencesDirty { get; set; }

        /// <summary> Called to notify this tracker that a mutation has been added. </summary>
        public void NotifyMutationAdded([NotNull] Hediff_AddedMutation mutation)
        {
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));
            _mutationList.Add(mutation);

            InfluencesDirty = true; 

            NotifyCompsAdded(mutation);
        }

        private void RecalcInfluences()
        {
            AnimalClassUtilities.FillInfluenceDict(_mutationList, _influenceDict);
            TotalInfluence = _influenceDict.Select(s => s.Value).Sum();
            MutationsCount += 1;
        }

        /// <summary> Called to notify this tracker that a mutation has been removed. </summary>
        public void NotifyMutationRemoved([NotNull] Hediff_AddedMutation mutation)
        {
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));
            _mutationList.Remove(mutation);
            MutationsCount -= 1;

            InfluencesDirty = true;

            NotifyCompsRemoved(mutation);
        }

        /// <summary>
        ///     exposes this instances data after the parent.
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                // Generate lookup dict manually during load for backwards compatibility.
            {
                if (!MutagenDefOf.defaultMutagen.CanInfect(Pawn)) return; //tracker is added on some kinds of pawns that can't get mutations, like mechanoids 

                RecalculateMutationInfluences();
            }
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

        /// <summary>
        /// Called when the pawn's race changes.
        /// </summary>
        /// <param name="oldRace">The old race.</param>
        void IRaceChangeEventReceiver.OnRaceChange(ThingDef oldRace)
        {
            if (oldRace.race.body != parent.def.race.body)
            {
                RecalculateMutationInfluences();
            }
        }
    }
}