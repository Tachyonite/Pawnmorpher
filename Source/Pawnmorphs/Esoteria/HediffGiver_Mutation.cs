// HediffGiver_Mutation.cs modified by Iron Wolf for Pawnmorph on 08/07/2019 10:19 AM
// last updated 08/07/2019  10:19 AM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// hediff giver for giving a mutation 
    /// </summary>
    /// <seealso cref="Verse.HediffGiver" />
    public class HediffGiver_Mutation : HediffGiver
    {
        /// <summary> The mean time between when the parent Hedif is applied and this HediffGiver performs its opperations. </summary>
        public float mtbDays;

        /// <summary> The gender to preferentially apply this hediff to.</summary>
        public Gender gender;

        /// <summary> The chance (out of 100) that the hediff will be applied. </summary>
        public int chance = 100;

        /// <summary> The tale to add to the art pool. </summary>
        public TaleDef tale;

        /// <summary> The thought to add to the pawn if they acquire the hediff. </summary>
        public ThoughtDef memory;

        /// <summary> Whether or not the thought should be added despite what the mod's settings dictate. </summary>
        public bool ignoreThoughtLimit;

        /// <summary>
        /// The MTB units
        /// </summary>
        public float mtbUnits = 60000f;

        /// <summary>
        /// Whether or not the curent HediffGiver has tried to add this hediff.<br />
        /// Used to prevent the chance from activating if spammed repeatedly.
        /// </summary>
        private readonly Dictionary<Hediff, bool> _triggered = new Dictionary<Hediff, bool>();

        /// <summary> Clears the triggeredHediff from this giver so it can trigger again on the same hediff. </summary>
        /// <param name="triggeredHediff">The triggered hediff.</param>
        public void ClearHediff(Hediff triggeredHediff)
        {
            _triggered.Remove(triggeredHediff);
        }

        /// <summary> The function that does the heavy lifting for a HediffGiver. </summary>
        /// <param name="pawn"> The pawn the parent hediff is applied to. </param>
        /// <param name="cause"> The parent hediff where this HediffGiver is located in. </param>
        public override void OnIntervalPassed(Pawn pawn, [NotNull] Hediff cause)
        {
            // Push a multiplay-safe randomization seed.
            RandUtilities.PushState();

            // After roughly this duration, try to apply the hediff if the pawn is of human-like intelligence.
            if (Rand.MTBEventOccurs(mtbDays, mtbUnits, 30f) && pawn.RaceProps.intelligence == Intelligence.Humanlike)
            {
                MutagenDef mutagen = (cause as Hediff_Morph)?.GetMutagenDef() ?? MutagenDefOf.defaultMutagen;

                // Check if this HediffGiver has the HediffComp_Single property (basically a dummy property that only comes into play in this function).
                var comp = cause.TryGetComp<HediffComp_Single>();

                // If we haven't already tried to apply this giver's hediff and the pawn either passes a percentile roll or are of the right gender, try and apply the hediff.
                if (!_triggered.TryGetValue(cause) && (gender == pawn.gender || Rand.RangeInclusive(0, 100) <= chance) && TryApply(pawn, mutagen, null, cause))
                {
                    _triggered[cause] = true;
                    IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);

                    if (tale != null) TaleRecorder.RecordTale(tale, pawn);
                    if (memory != null) TryAddMemory(pawn);

                    // If the parent has the single comp, decrement it's current count and remove it if it's out of charges.
                    if (comp != null)
                    {
                        comp.stacks--;
                        if (comp.stacks <= 0) pawn.health.RemoveHediff(cause);
                    }
                }
                else
                {
                    _triggered[cause] = true;
                    // If the giver had the single comp, clear the list so it can try and apply the hediff again. 
                    // While this does prevent the partial hediffs from getting "stuck", it does mean that they
                    // can apply a hediff that already exists or failed because of the %chance rule. Need to find
                    // a better solution to this problem eventually.
                    if (comp != null) ClearHediff(cause);
                }
            }

            // Pop the seed to restore balance to the universe (and the game).
            RandUtilities.PopState();
        }


        /// <summary>Tries the apply the mutation to the given pawn</summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="mutagenDef">The mutagen definition. used to determine if it's a valid target or not</param>
        /// <param name="outAddedHediffs">The out added hediffs.</param>
        /// <param name="cause">The cause.</param>
        /// <returns>if the mutation was added or not</returns>
        public bool TryApply(Pawn pawn, MutagenDef mutagenDef, List<Hediff> outAddedHediffs = null, Hediff cause = null)
        {
            if (!mutagenDef.CanInfect(pawn)) return false;

            bool added = PawnmorphHediffGiverUtility.TryApply(pawn, hediff, partsToAffect, canAffectAnyLivePart, countToAffect, outAddedHediffs);
            if (added && partsToAffect != null)
            {
                var log = new MutationLogEntry(pawn, hediff, partsToAffect);
                Find.PlayLog.Add(log); 
            }
            return added;
        }

        private void TryAddMemory(Pawn pawn)
        {
            ThoughtHandler thoughts = pawn.needs?.mood?.thoughts;
            if (thoughts == null) return;

            if (!ThoughtUtility.CanGetThought(pawn, memory)) return;

            var counter = 0;
            int max = PMUtilities.GetSettings().maxMutationThoughts;

            if (!ignoreThoughtLimit)
                foreach (Thought_Memory thought in thoughts.memories.Memories)
                {
                    if (MutationUtilities.AllMutationMemories.Contains(thought.def)) counter++;

                    counter++;
                    if (counter >= max) return; //make sure to only add so many thoughts at once 
                }

            pawn.TryGainMemory(memory);
        }
    }
}