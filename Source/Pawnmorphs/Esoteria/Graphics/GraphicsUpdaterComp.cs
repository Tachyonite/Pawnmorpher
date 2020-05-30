// GraphicsUpdaterComp.cs created by Iron Wolf for Pawnmorph on 09/13/2019 8:14 AM
// last updated 09/13/2019  8:14 AM

#pragma warning disable 1591
using System.Linq;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.Hybrids;
using RimWorld;
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

        public bool IsDirty { get; set; }

        [NotNull]
        private Pawn Pawn => (Pawn) parent;

        private AlienPartGenerator.AlienComp GComp => Pawn.GetComp<AlienPartGenerator.AlienComp>();

        private InitialGraphicsComp InitialGraphics => Pawn.GetComp<InitialGraphicsComp>();
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Assert(parent is Pawn, "parent is Pawn");
        }


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (IsDirty)
            {
                RefreshGraphics();
            }
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

        public override void CompTick()
        {
            base.CompTick();
            if (IsDirty)
            {
                IsDirty = false;
                RefreshGraphics();
            }
        }

        private void RefreshGraphics()
        {
            var pawn = (Pawn)parent;
            var mTracker = pawn.GetMutationTracker();
            if (mTracker != null)
            {
                var colorationAspect = pawn.GetAspectTracker()?.GetAspect<Aspects.ColorationAspect>();
                bool force = colorationAspect != null;
                UpdateSkinColor(mTracker, force);
                UpdateHairColor(mTracker, force);
            }
            pawn?.RefreshGraphics();
            IsDirty = false; 
        }

        bool UpdateSkinColor([NotNull] MutationTracker tracker, bool force = false)
        {
            if (GComp == null || InitialGraphics == null) return false;

            var colorationAspect = tracker.Pawn.GetAspectTracker()?.GetAspect<Aspects.ColorationAspect>();
            if (colorationAspect != null && colorationAspect.IsFullOverride)
            {
                var color = colorationAspect.ColorSet.skinColor;
                if (color.HasValue)
                {
                    GComp.skinColor = color.Value;
                    return true;
                }
            }

            var highestInfluence = Pawn.GetHighestInfluence();
            var curMorph = Pawn.def.GetMorphOfRace();
            if (highestInfluence == null || (!force && highestInfluence == curMorph))
            {
                if (GComp.skinColor == InitialGraphics.SkinColor)
                {
                    return false; // If there is not influence or if the highest influence is that of their current race do nothing.
                }
                else 
                {
                    GComp.skinColor = InitialGraphics.SkinColor;
                    return true;
                }
            }


            float lerpVal = tracker.GetDirectNormalizedInfluence(highestInfluence);
            var baseColor = curMorph?.GetSkinColorOverride(tracker.Pawn) ?? InitialGraphics.SkinColor;
            var morphColor = highestInfluence.GetSkinColorOverride(tracker.Pawn) ?? InitialGraphics.SkinColor;

            if (!force && baseColor == morphColor)
            {
                return false; // If they're the same color don't do anything.
            }

            var col = Color.Lerp(baseColor, morphColor, Mathf.Sqrt(lerpVal)); // Blend the 2 by the normalized colors.

            GComp.skinColor = GComp.ColorChannels["skin"].first = col;

            return true;
        }

        bool UpdateHairColor([NotNull] MutationTracker tracker, bool force = false)
        {
            if (GComp == null || InitialGraphics == null || Pawn.story == null) return false;

            var colorationAspect = tracker.Pawn.GetAspectTracker()?.GetAspect<Aspects.ColorationAspect>();
            if (colorationAspect != null && colorationAspect.IsFullOverride)
            {
                var color = colorationAspect.ColorSet.hairColor;
                if (color.HasValue)
                {
                    Pawn.story.hairColor = color.Value;
                    return true;
                }
            }

            var highestInfluence = Pawn.GetHighestInfluence();
            var curMorph = Pawn.def.GetMorphOfRace();

            if (highestInfluence == null || (!force && highestInfluence == curMorph))
            {
                if (Pawn.story.hairColor == InitialGraphics.HairColor)
                {
                    return false; // If there is not influence or if the highest influence is that of their current race do nothing.
                }
                else
                {
                    Pawn.story.hairColor = InitialGraphics.HairColor;
                    return true;
                }
            }
                
            float lerpVal = tracker.GetDirectNormalizedInfluence(highestInfluence);
            var baseColor = curMorph?.GetHairColorOverride(tracker.Pawn) ?? InitialGraphics.HairColor;
            var morphColor = highestInfluence.GetHairColorOverride(tracker.Pawn) ?? InitialGraphics.HairColor;

            if (!force && baseColor == morphColor)
            {
                return false; //if they're the same color don't  do anything 
            }

            var col = Color.Lerp(baseColor, morphColor, Mathf.Sqrt(lerpVal)); //blend the 2 by the normalized colors 

            Pawn.story.hairColor = GComp.ColorChannels["hair"].first = col;

            return true;
        }

        void IMutationEventReceiver.MutationAdded(Hediff_AddedMutation mutation, MutationTracker tracker)
        {
            IsDirty = true; 
        }

     
        void IMutationEventReceiver.MutationRemoved(Hediff_AddedMutation mutation,  MutationTracker tracker)
        {
            IsDirty = true; 
        }
    }
}