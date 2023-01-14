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
        private Color _effectiveSkinColor;
        private Color _effectiveHairColor;

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
                    GComp.SetSkinColor(color.Value); 
                    return true;
                }
            }

            var highestInfluence = Pawn.GetHighestInfluence();
            var curMorph = Pawn.def.GetMorphOfRace();
            if (highestInfluence == null)
            {
                if (GComp.GetSkinColor() == InitialGraphics.SkinColor && !force)
                {
                    return false; // If there is not influence or if the highest influence is that of their current race do nothing.
                }
                else 
                {
                    GComp.ColorChannels["skin"].first = InitialGraphics.SkinColor;
                    return true;
                }
            }

            // Apply PM color if no override is defined or override is PM's
            if (force || Pawn.story.skinColorOverride == null || Pawn.story.skinColorOverride == _effectiveSkinColor)
            {
                float lerpVal = tracker.GetDirectNormalizedInfluence(highestInfluence);
                var baseColor = curMorph?.GetSkinColorOverride(tracker.Pawn) ?? InitialGraphics.SkinColor;
                var morphColor = highestInfluence.GetSkinColorOverride(tracker.Pawn) ?? InitialGraphics.SkinColor;

                _effectiveSkinColor = Color.Lerp(baseColor, morphColor, Mathf.Sqrt(lerpVal)); // Blend the 2 by the normalized colors.
                Pawn.story.skinColorOverride = _effectiveSkinColor;
			}
            GComp.OverwriteColorChannel("skin", Pawn.story.SkinColor, Pawn.story.SkinColor); // Assign effective skin color to body parts.
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
                    Pawn.story.HairColor = color.Value;
                    return true;
                }
            }

            var highestInfluence = Pawn.GetHighestInfluence();
            var curMorph = Pawn.def.GetMorphOfRace();

            if (highestInfluence == null)
            {
                if (Pawn.story.HairColor == InitialGraphics.HairColor && !force)
                {
                    return false; // If there is not influence or if the highest influence is that of their current race do nothing.
                }
                else
                {
                    Pawn.story.HairColor = InitialGraphics.HairColor;
                    return true;
                }
            }
                

            Color currentHairColor = Pawn.story.HairColor;
            // If forced, hair color is set here or hair color is natural then update. (Avoid updating if overriden by genes)
            if (force || currentHairColor == _effectiveHairColor || currentHairColor == InitialGraphics.HairColor)
            {
                float lerpVal = tracker.GetDirectNormalizedInfluence(highestInfluence);
                var baseColor = curMorph?.GetHairColorOverride(tracker.Pawn) ?? InitialGraphics.HairColor;
                var morphColor = highestInfluence.GetHairColorOverride(tracker.Pawn) ?? InitialGraphics.HairColor;

				// If base is transparent don't do anything
				if (baseColor.a == 0)
					return false;

				Color col;
				if (morphColor.a == 0)
				{
					// If target is transparent, keep using base.
					col = baseColor;
				}
				else
					col = Color.Lerp(baseColor, morphColor, Mathf.Sqrt(lerpVal)); //blend the 2 by the normalized colors 

				_effectiveHairColor = col;
                Pawn.story.HairColor = col;
			}


			GComp.ColorChannels["hair"].first = Pawn.story.HairColor;

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