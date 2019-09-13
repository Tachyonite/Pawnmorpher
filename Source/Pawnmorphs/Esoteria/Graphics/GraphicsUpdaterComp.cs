// GraphicsUpdaterComp.cs created by Iron Wolf for Pawnmorph on 09/13/2019 8:14 AM
// last updated 09/13/2019  8:14 AM

using AlienRace;
using Pawnmorph.Hybrids;
using UnityEngine;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils; 

namespace Pawnmorph.GraphicSys
{
    public class GraphicsUpdaterComp : ThingComp 
    {
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Assert(parent is Pawn, "parent is Pawn");

            

        }

        private Pawn Pawn => (Pawn) parent; 



        public override void PostExposeData()
        {
            base.PostExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                var tracker = Pawn.GetMutationTracker();
                Assert(tracker != null, "tracker != null");
                Assert(Pawn.GetComp<AlienPartGenerator.AlienComp>() != null, "Pawn.GetComp<AlienPartGenerator.AlienComp>() != null");
                Assert(InitialGraphics != null, "InitialGraphics != null"); 
                // ReSharper disable once PossibleNullReferenceException
                tracker.MutationAdded += OnMutationAdded; //handled by above assertion 
                tracker.MutationRemoved += OnMutationRemoved; 
            }
        }


        private AlienPartGenerator.AlienComp GComp => Pawn.GetComp<AlienPartGenerator.AlienComp>();

        private InitialGraphicsComp InitialGraphics => Pawn.GetComp<InitialGraphicsComp>(); 


        bool UpdateSkinColor(MutationTracker tracker)
        {
            var highestInfluence = Pawn.GetHighestInfluence();
            var curMorph = Pawn.def.GetMorphOfRace();
            if (highestInfluence == null || highestInfluence == curMorph) return false; //if there is not influence or if the highest influence is 
                                                                        //that of their current race do nothing 

            float lerpVal = tracker.GetNormalizedInfluence(highestInfluence);
            var baseColor = curMorph.GetSkinColorOverride() ?? InitialGraphics.skinColor;
            var morphColor = highestInfluence.GetSkinColorOverride() ?? InitialGraphics.skinColor;

            if (baseColor == morphColor) return false; //if they're the same color don't  do anything 


            var col = Color.Lerp(baseColor, morphColor, lerpVal); //blend the 2 by the normalized colors 

            GComp.skinColor = col;
            return true; 
        }


        private void OnMutationAdded(MutationTracker sender, Pawn pawn, Hediff_AddedMutation mutation)
        {
            RefreshGraphics(sender, pawn);
        }

        private void RefreshGraphics(MutationTracker sender, Pawn pawn)
        {
            bool needsUpdate = UpdateSkinColor(sender);


            if (needsUpdate) //make sure to only refresh the graphics if they've been modified 
            {
                pawn.RefreshGraphics();
            }
        }

        private void OnMutationRemoved(MutationTracker sender, Pawn pawn, Hediff_AddedMutation mutation)
        {
            RefreshGraphics(sender, pawn); 
        }
    }
}