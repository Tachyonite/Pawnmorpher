// GraphicsUpdaterComp.cs created by Iron Wolf for Pawnmorph on 09/13/2019 8:14 AM
// last updated 09/13/2019  8:14 AM

using System;
using System.Diagnostics;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.Hybrids;
using Pawnmorph.Utilities;
using UnityEngine;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils;
using Debug = System.Diagnostics.Debug;

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

        private Pawn Pawn => (Pawn)parent;

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

        bool UpdateSkinColor(MutationTracker tracker)
        {
            var highestInfluence = Pawn.GetHighestInfluence();
            var curMorph = Pawn.def.GetMorphOfRace();
            if (highestInfluence == null || highestInfluence == curMorph) return false; // If there is not influence or if the highest influence is that of their current race do nothing.

            float lerpVal = tracker.GetNormalizedInfluence(highestInfluence);
            var baseColor = curMorph?.GetSkinColorOverride() ?? InitialGraphics.SkinColor;
            var morphColor = highestInfluence.GetSkinColorOverride() ?? InitialGraphics.SkinColor;

            if (baseColor == morphColor)
            {
                Log.Warning($"morphColor and baseColor are the same for morph {highestInfluence.defName} and {curMorph}");

                return false; // If they're the same color don't  do anything.
            }

            var col = Color.Lerp(baseColor, morphColor, lerpVal); // Blend the 2 by the normalized colors.

            GComp.skinColor = GComp.ColorChannels["skin"].first = col;

            return true;
        }

        bool UpdateHairColor(MutationTracker tracker)
        {
            var highestInfluence = Pawn.GetHighestInfluence();
            var curMorph = Pawn.def.GetMorphOfRace();
            if (highestInfluence == null || highestInfluence == curMorph) return false; //if there is not influence or if the highest influence is 
            //that of their current race do nothing 

            float lerpVal = tracker.GetNormalizedInfluence(highestInfluence);
            var baseColor = curMorph?.GetHairColorOverride() ?? InitialGraphics.HairColor;
            var morphColor = highestInfluence.GetHairColorOverride() ?? InitialGraphics.HairColor;

            if (baseColor == morphColor) return false; //if they're the same color don't  do anything 


            var col = Color.Lerp(baseColor, morphColor, lerpVal); //blend the 2 by the normalized colors 

            Pawn.story.hairColor = GComp.ColorChannels["hair"].first = col;

            return true;
        }

        void IMutationEventReceiver.MutationAdded(Hediff_AddedMutation mutation, MutationTracker tracker)
        {
            RefreshGraphics(tracker, Pawn);
        }

        private void RefreshGraphics(MutationTracker sender, Pawn pawn)
        {
            bool needsUpdate = UpdateSkinColor(sender);
            needsUpdate |= UpdateHairColor(sender);

            if (needsUpdate) //make sure to only refresh the graphics if they've been modified 
            {
                pawn.RefreshGraphics();
            }
        }

        void IMutationEventReceiver.MutationRemoved(Hediff_AddedMutation mutation, MutationTracker tracker)
        {
            RefreshGraphics(tracker, Pawn);
        }
    }
}