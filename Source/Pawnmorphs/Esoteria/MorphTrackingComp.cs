// MorphTrackingComp.cs modified by Iron Wolf for Pawnmorph on //2019 
// last updated 09/09/2019  7:38 PM

using JetBrains.Annotations;
using Pawnmorph.Hybrids;
using Verse;

namespace Pawnmorph
{
    /// <summary> Component for tracking the morph related updates of a single pawn. </summary>
    public class MorphTrackingComp : ThingComp
    {
        private bool _isAwake;

        private Pawn Pawn => (Pawn)parent;

        void Awake()
        {
            var comp = parent.Map?.GetComponent<MorphTracker>();

            if (comp == null)
            {
                MorphGroupDef group = parent.def.GetMorphOfRace()?.@group;
                var aTracker = Pawn.GetAspectTracker();
                if (aTracker == null) return;
                var aspectDef = group?.aspectDef;
                if (aspectDef == null) return;

                var aspect = aTracker.GetAspect(aspectDef);
                if (aspect == null)
                {
                    aspect = aspectDef.CreateInstance();

                    //add an small offset so minSeverity in hediffStages works as expected 
                    aTracker.Add(aspect);
                }
            }
        }

#pragma warning disable 0618
        private void RemoveObsoleteHediffs()
        {
            var group = parent.def.GetMorphOfRace()?.group;
            var hDef = group?.hediff;
            if (hDef == null) return;
            var h = Pawn.health.hediffSet.GetFirstHediffOfDef(hDef);
            if (h != null)
                Pawn.health.RemoveHediff(h);
        }
#pragma warning restore 0618

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            if (!_isAwake)
            {
                _isAwake = true;
                Awake();
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (!_isAwake)
            {
                _isAwake = true;
                Awake();
            }

            var comp = parent.Map?.GetComponent<MorphTracker>();

            if (comp != null)
            {
                comp.NotifySpawned((Pawn)parent);
                comp.MorphCountChanged -= MorphCountChanged; //make sure we only subscribe once 
                comp.MorphCountChanged += MorphCountChanged;
                RecalculateMorphCount(comp);
            }

            if (respawningAfterLoad)
                RemoveObsoleteHediffs();
        }

        private void RecalculateMorphCount(MorphTracker tracker)
        {
            MorphDef myMorph = parent.def.GetMorphOfRace();
            AspectTracker aspectTracker = Pawn.GetAspectTracker();
            if (aspectTracker == null) return;
            MorphGroupDef group = myMorph?.group;
            AspectDef aspectDef = group?.aspectDef;
            if (aspectDef == null) return;

            Aspect aspect = aspectTracker.GetAspect(aspectDef);
            if (aspect == null) //if the hediff is missing for some reason add it again 
            {
                aspect = aspectDef.CreateInstance();
                aspectTracker.Add(aspect);
            }

            aspect.StageIndex = tracker.GetGroupCount(group) - 1;
        }

        private void MorphCountChanged(MorphTracker sender, MorphDef morph)
        {
            MorphDef myMorph = parent.def.GetMorphOfRace();
            if (myMorph?.group == null) return;
            if (myMorph.group != morph?.group) return;

            var pawn = (Pawn)parent;
            AspectTracker aspectTracker = pawn.GetAspectTracker();
            if (aspectTracker == null) return;
            AspectDef aspectDef = morph?.group?.aspectDef;

            if (aspectDef == null) return;

            Aspect aspect = aspectTracker.GetAspect(aspectDef);
            if (aspect == null) //if the aspect is missing for some reason add it again 
            {
                aspect = aspectDef.CreateInstance();
                aspectTracker.Add(aspect);
            }

            var comp = pawn.Map?.GetComponent<MorphTracker>();
            aspect.StageIndex = (comp?.GetGroupCount(morph.group) ?? 0) - 1;
            //stage should always be equal to the number of morphs in the group active in the same map 
        }

        /// <summary> Notify that the parent has changed races. </summary>
        /// <param name="oldMorph"> The morph the parent used to be. </param>
        public void NotifyRaceChanged([CanBeNull] MorphDef oldMorph)
        {
            parent.Map?.GetComponent<MorphTracker>().NotifyPawnRaceChanged((Pawn)parent, oldMorph);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            var comp = map.GetComponent<MorphTracker>();
            comp.NotifyDespawned((Pawn)parent);
            comp.MorphCountChanged -= MorphCountChanged;
        }
    }
}
