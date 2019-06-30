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

namespace Pawnmorph
{
    public class PawnmorphGameComp : WorldComponent
    {
        public HashSet<PawnMorphInstance> pawnmorphs = new HashSet<PawnMorphInstance>(){};

        public PawnmorphGameComp(World world) : base(world)
        {
        }

        public void addPawn(PawnMorphInstance pm)
        {
            this.pawnmorphs.Add(pm);
        }
        public PawnMorphInstance retrieve(Pawn animal)
        {
            PawnMorphInstance pm = this.pawnmorphs.FirstOrDefault(instance => instance.replacement == animal);
            return pm;
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref this.pawnmorphs, "pawnmorphs", LookMode.Deep);
        }
    }
}
