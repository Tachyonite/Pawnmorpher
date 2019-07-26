using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using static RimWorld.MoteMaker;
using RimWorld;
using Multiplayer.API;

namespace Pawnmorph
{
    public class PawnMorphInstance : IExposable
    {
        public Pawn origin;
        public Pawn replacement;

        public PawnMorphInstance()
        {
        }

        public PawnMorphInstance(Pawn original, Pawn polymorph)
        {
            origin = original;
            replacement = polymorph;
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref origin, true, "origin");
            Scribe_References.Look(ref replacement, "replacement", true);
        }
    }

    public class PawnMorphInstanceMerged : IExposable
    {
        public Pawn origin;
        public Pawn origin2;
        public Pawn replacement;
        public PawnMorphInstanceMerged() { }
        public PawnMorphInstanceMerged(Pawn original, Pawn original2, Pawn polymorph)
        {
            origin = original;
            origin2 = original2;
            replacement = polymorph;
        }
        public void ExposeData()
        {
            Scribe_Deep.Look(ref this.origin, true, "originmerged");
            Scribe_Deep.Look(ref this.origin2, true, "originmerged2");
            Scribe_References.Look(ref this.replacement, "replacement", true);
        }
    }
}
