// GraphicsUpdaterComp.cs created by Iron Wolf for Pawnmorph on 09/13/2019 8:14 AM
// last updated 09/13/2019  8:14 AM

#pragma warning disable 1591
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.Hybrids;
using UnityEngine;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils;

namespace Pawnmorph.GraphicSys
{
    /// <summary>
    /// Thing comp to update the graphics of a pawn as they gain/lose mutations. <br/>
    /// Requires that the pawn have a MutationTracker comp too.
    /// </summary>
    [UsedImplicitly]
    public class GraphicsUpdaterComp : ThingComp, IMutationEventReceiver
    {
        private bool _subOnce;

        [NotNull]
        private Pawn Pawn => (Pawn) parent;

        private AlienPartGenerator.AlienComp GComp => Pawn.GetComp<AlienPartGenerator.AlienComp>();

        private InitialGraphicsComp InitialGraphics => Pawn.GetComp<InitialGraphicsComp>();
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Assert(parent is Pawn, "parent is Pawn");
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Assert(_subOnce == false, "_subOnce == false");
                _subOnce = true;

                var tracker = Pawn.GetMutationTracker();
                Assert(tracker != null, "tracker != null");
                Assert(Pawn.GetComp<AlienPartGenerator.AlienComp>() != null, "Pawn.GetComp<AlienPartGenerator.AlienComp>() != null");
                Assert(InitialGraphics != null, "InitialGraphics != null");
            }
        }

        bool UpdateSkinColor([NotNull] MutationTracker tracker, bool force = false)
        {
            var highestInfluence = Pawn.GetHighestInfluence();
            var curMorph = Pawn.def.GetMorphOfRace();
            if (highestInfluence == null || (!force && highestInfluence == curMorph)) return false; // If there is not influence or if the highest influence is that of their current race do nothing.
            if (GComp == null || InitialGraphics == null) return false;

            float lerpVal = tracker.GetDirectNormalizedInfluence(highestInfluence);
            var baseColor = curMorph?.GetSkinColorOverride(tracker.Pawn) ?? InitialGraphics.SkinColor;
            var morphColor = highestInfluence.GetSkinColorOverride(tracker.Pawn) ?? InitialGraphics.SkinColor;

            if (!force && baseColor == morphColor)
            {
                return false; // If they're the same color don't do anything.
            }

            var col = Color.Lerp(baseColor, morphColor, lerpVal); // Blend the 2 by the normalized colors.

            GComp.skinColor = GComp.ColorChannels["skin"].first = col;

            return true;
        }

        bool UpdateHairColor([NotNull] MutationTracker tracker, bool force = false)
        {
            var highestInfluence = Pawn.GetHighestInfluence();
            var curMorph = Pawn.def.GetMorphOfRace();
            if (highestInfluence == null || (!force && highestInfluence == curMorph)) return false; // If there is not influence or if the highest influence is that of their current race do nothing.
            if (GComp == null || InitialGraphics == null || Pawn.story == null) return false;

            float lerpVal = tracker.GetDirectNormalizedInfluence(highestInfluence);
            var baseColor = curMorph?.GetHairColorOverride(tracker.Pawn) ?? InitialGraphics.HairColor;
            var morphColor = highestInfluence.GetHairColorOverride(tracker.Pawn) ?? InitialGraphics.HairColor;

            if (!force && baseColor == morphColor)
            {
                return false; //if they're the same color don't  do anything 
            }

            var col = Color.Lerp(baseColor, morphColor, lerpVal); //blend the 2 by the normalized colors 

            Pawn.story.hairColor = GComp.ColorChannels["hair"].first = col;

            return true;
        }

        void IMutationEventReceiver.MutationAdded(Hediff_AddedMutation mutation, MutationTracker tracker)
        {
            RefreshGraphics(tracker, Pawn);
        }

        internal void RefreshGraphics([NotNull] MutationTracker sender, [NotNull] Pawn pawn, bool force = false)
        {
            bool needsUpdate = UpdateSkinColor(sender, force);
            needsUpdate |= UpdateHairColor(sender, force);

            if (needsUpdate || force) //make sure to only refresh the graphics if they've been modified 
            {
                pawn.RefreshGraphics();
            }
        }

        void IMutationEventReceiver.MutationRemoved(Hediff_AddedMutation mutation,  MutationTracker tracker)
        {
            RefreshGraphics(tracker, Pawn);
        }
    }
}