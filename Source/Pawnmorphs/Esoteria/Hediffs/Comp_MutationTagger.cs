// MutationTagger.cs created by Iron Wolf for Pawnmorph on 02/12/2021 7:12 AM
// last updated 02/12/2021  7:12 AM

using System;
using JetBrains.Annotations;
using Pawnmorph.Chambers;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// hediff comp to tag mutations that added during the duration of the hediff 
    /// </summary>
    /// <seealso cref="Verse.HediffComp" />
    /// <seealso cref="Pawnmorph.IMutationEventReceiver" />
    public class Comp_MutationTagger : HediffComp, IMutationEventReceiver
    {
        private ChamberDatabase _db;

        
        [CanBeNull] private SimpleCurve Curve => (props as CompProps_MutationTagger)?.tagChancePerValue; 

        bool CanTag(MutationDef mDef)
        {
            var curve = Curve;
            float chance; 
            if (curve != null)
            {
                chance = curve.Evaluate(mDef.value); 
            }
            else
            {
                chance = CompProps_MutationTagger.DEFAULT_TAG_CHANCE; 
            }

            return Rand.Chance(chance);
        }


        private ChamberDatabase DB
        {
            get
                => Find.World.GetComponent<ChamberDatabase>();
        }


        /// <summary>called when a mutation is added</summary>
        /// <param name="mutation">The mutation.</param>
        /// <param name="tracker">The tracker.</param>
        public void MutationAdded(Hediff_AddedMutation mutation, MutationTracker tracker)
        {
            try
            {
                var mutationDef = (MutationDef) mutation.def;


                if (CanTag(mutationDef))
                {
                    if (!DB.CanAddToDatabase(mutationDef, out string reason))
                    {
                        Messages.Message(reason, MessageTypeDefOf.RejectInput);
                        return; 
                    }
                    
                    DB.TryAddToDatabase(mutationDef);
                }
            }
            catch (InvalidCastException e)
            {
                Log.Error($"in {mutation.Label}/{mutation.def.defName} cannot convert {mutation.def.GetType().Name} to {nameof(MutationDef)}!\n{e}");
            }
        }

        /// <summary>called when a mutation is removed</summary>
        /// <param name="mutation">The mutation.</param>
        /// <param name="tracker">The tracker.</param>
        public void MutationRemoved(Hediff_AddedMutation mutation, MutationTracker tracker)
        {
            //empty 
        }
    }


    /// <summary>
    /// comp properties for the mutation tagger comp 
    /// </summary>
    /// <seealso cref="Verse.HediffCompProperties" />
    public class CompProps_MutationTagger : HediffCompProperties
    {
        /// <summary>
        /// The default tag chance 
        /// </summary>
        public const float DEFAULT_TAG_CHANCE = 0.5f;

        /// <summary>
        /// The tag chance per value
        /// </summary>
        public SimpleCurve tagChancePerValue; 

        /// <summary>
        /// Initializes a new instance of the <see cref="CompProps_MutationTagger"/> class.
        /// </summary>
        public CompProps_MutationTagger()
        {
            compClass = typeof(Comp_MutationTagger); 
        }
    }

}