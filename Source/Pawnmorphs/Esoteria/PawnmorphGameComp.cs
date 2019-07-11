using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using static RimWorld.MoteMaker;
using RimWorld.Planet;
using RimWorld;
using Multiplayer.API;

namespace Pawnmorph
{
    public class PawnmorphGameComp : WorldComponent
    {
        public HashSet<PawnMorphInstance> pawnmorphs = new HashSet<PawnMorphInstance>(){};
        public HashSet<PawnMorphInstanceMerged> mergedpawnmorphs = new HashSet<PawnMorphInstanceMerged>() { };

        public PawnmorphGameComp(World world) : base(world)
        {
        }

        public void addPawn(PawnMorphInstance pm)
        {
            this.pawnmorphs.Add(pm);
        }
        public void addPawnMerged(PawnMorphInstanceMerged pmm)
        {
            this.mergedpawnmorphs.Add(pmm);
        }
        public PawnMorphInstance retrieve(Pawn animal)
        {
            PawnMorphInstance pm = this.pawnmorphs.FirstOrDefault(instance => instance.replacement == animal);
            return pm;
        }

        public PawnMorphInstanceMerged retrieveMerged(Pawn animal)
        {
            PawnMorphInstanceMerged pm = this.mergedpawnmorphs.FirstOrDefault(instance => instance.replacement == animal);
            return pm;
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref this.pawnmorphs, "pawnmorphs", LookMode.Deep);
            Scribe_Collections.Look(ref this.mergedpawnmorphs, "pawnmorphs", LookMode.Deep);
        }
    }
}
