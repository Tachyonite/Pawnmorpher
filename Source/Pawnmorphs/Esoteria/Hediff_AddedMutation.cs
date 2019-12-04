using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// hediff representing a mutation 
    /// </summary>
    /// <seealso cref="Verse.HediffWithComps" />
    public class Hediff_AddedMutation : HediffWithComps
    {
        /// <summary>
        /// The mutation description
        /// </summary>
        public string mutationDescription;

        /// <summary> The influence this mutation exerts on a pawn. </summary>
        [CanBeNull]
        public Comp_MorphInfluence Influence => (comps?.OfType<Comp_MorphInfluence>().FirstOrDefault());

        /// <summary>
        /// Gets a value indicating whether should be removed.
        /// </summary>
        /// <value><c>true</c> if should be removed; otherwise, <c>false</c>.</value>
        public override bool ShouldRemove
        {
            get
            {
                return false;
            }
        }
        /// <summary>Gets the mutation description.</summary>
        /// <value>The mutation description.</value>
        public string MutationDescription
        {
            get
            {
                if (mutationDescription == null)
                {
                    StringBuilder builder = new StringBuilder();
                    CreateDescription(builder);
                    mutationDescription = builder.ToString();
                }
                return mutationDescription;
            }
        }

        /// <summary>Gets the extra tip string .</summary>
        /// <value>The extra tip string .</value>
        public override string TipStringExtra
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(base.TipStringExtra);
                stringBuilder.AppendLine("Efficiency".Translate() + ": " + def.addedPartProps.partEfficiency.ToStringPercent());
                return stringBuilder.ToString();
            }
        }

        /// <summary>Creates the description.</summary>
        /// <param name="builder">The builder.</param>
        public virtual void CreateDescription(StringBuilder builder)
        {
            if (def.description == null)
            {
                
                builder.AppendLine("PawnmorphTooltipNoDescription".Translate());
                return;
            }

            string res = def.description.AdjustedFor(pawn);
            builder.AppendLine(res);
        }

        private bool _waitingForUpdate; 

        /// <summary>called after this instance is added to the pawn.</summary>
        /// <param name="dinfo">The dinfo.</param>
        public override void PostAdd(DamageInfo? dinfo)
        // After the hediff has been applied.
        {
            base.PostAdd(dinfo); // Do the inherited method.
            if (PawnGenerator.IsBeingGenerated(pawn) || !pawn.Spawned) //if the pawn is still being generated do not update graphics until it's done 
            {
                _waitingForUpdate = true;
                return; 
            }
            UpdatePawnInfo();
        }
        /// <summary>
        /// Posts the tick.
        /// </summary>
        public override void PostTick()
        {
            base.PostTick();
            if (_waitingForUpdate)
            {
                UpdatePawnInfo();
                _waitingForUpdate = false; 
            }
        }

        private void UpdatePawnInfo()
        {
            if (Current.ProgramState == ProgramState.Playing)
                IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.Map); // Spawn some fairy dust ;).

            if (Current.ProgramState == ProgramState.Playing && MutationUtilities.AllMutationsWithGraphics.Contains(def) && pawn.IsColonist)
            {
                pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                PortraitsCache.SetDirty(pawn);
            }

            pawn.GetMutationTracker()?.NotifyMutationAdded(this);
        }

        /// <summary>called after this instance is removed from the pawn</summary>
        public override void PostRemoved()
        {
            base.PostRemoved();
            if(!PawnGenerator.IsBeingGenerated(pawn))
                pawn.GetMutationTracker()?.NotifyMutationRemoved(this);
        }

        /// <summary>Exposes the data.</summary>
        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit && Part == null)
            {
                Log.Error($"Hediff_AddedPart [{def.defName},{Label}] has null part after loading.", false);
                pawn.health.hediffSet.hediffs.Remove(this);
                return;
            }
        }
    }
}
