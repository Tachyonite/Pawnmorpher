using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using static RimWorld.MoteMaker;
using RimWorld;

namespace Pawnmorph
{
    public class PawnMorphInstance : IExposable
    {
        public Pawn origin;
        public Pawn replacement;
        public PawnMorphInstance(){}
        public PawnMorphInstance(Pawn original, Pawn polymorph)
        {
            origin = original;
            replacement = polymorph;
        }
        public void ExposeData()
        {
            Scribe_Deep.Look(ref this.origin, true, "origin");
            Scribe_References.Look(ref this.replacement, "replacement", true);
        }
    }
}
