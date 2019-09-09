// MorphTrackingComp.cs modified by Iron Wolf for Pawnmorph on //2019 
// last updated 09/09/2019  7:38 PM

using JetBrains.Annotations;
using Pawnmorph.Hybrids;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    ///     component for tracking the morph related updates of a single pawn
    /// </summary>
    public class MorphTrackingComp : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            var comp = parent.Map?.GetComponent<MorphTracker>();
            if (comp != null)
            {
                comp.NotifySpawned((Pawn) parent);
                comp.MorphCountChanged += MorphCountChanged;
            }

            if (respawningAfterLoad)
            {
                HediffDef hediffDef = parent.def.GetMorphOfRace()?.group?.hediff;
                var pawn = (Pawn) parent;
                Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                if (firstHediffOfDef == null)
                {
                    Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                    MorphGroupDef group = pawn.GetMorphGroup();

                    if (group == null)
                        hediff.Severity = 0;
                    else
                        hediff.Severity = (comp?.GetGroupCount(group) ?? 0) + EPSILON; //TODO caravans 
                    //add an small offset so minSeverity in hediffStages works as expected 
                    pawn.health.AddHediff(hediff);
                }
            }
        }

        private const float EPSILON = 0.001f;


        private void MorphCountChanged(MorphTracker sender, MorphDef morph)
        {
            MorphDef myMorph = parent.def.GetMorphOfRace();
            if (myMorph != morph) return;

            var pawn = (Pawn) parent;

            HediffDef groupHediff = morph.group?.hediff;
            if (groupHediff == null) return;

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(groupHediff);
            if (hediff == null) //if the hediff is missing for some reason add it again 
            {
                hediff = HediffMaker.MakeHediff(groupHediff, pawn);
                pawn.health.AddHediff(hediff);
            }

            var comp = pawn.Map?.GetComponent<MorphTracker>();
            hediff.Severity =
                (comp?.GetGroupCount(morph.group) ?? 0) + EPSILON; //add a small offset so minSeverity acts as expected 
            //severity should always be equal to the number of morphs in the group active in the same map 
        }

        /// <summary>
        /// notify that the parent has changed races 
        /// </summary>
        /// <param name="oldMorph">the morph the parent used to be </param>
        public void NotifyRaceChanged([CanBeNull] MorphDef oldMorph)
        {
            parent.Map?.GetComponent<MorphTracker>().NotifyPawnRaceChanged((Pawn) parent, oldMorph);
        }


        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            var comp = map.GetComponent<MorphTracker>();
            comp.NotifyDespawned((Pawn) parent);
            comp.MorphCountChanged -= MorphCountChanged;
        }
    }
}