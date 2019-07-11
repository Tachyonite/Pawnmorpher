using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using Multiplayer.API;

namespace Pawnmorph
{
    public class PawnRelationWorker_ExMerged : PawnRelationWorker
    {

        public PawnRelationDef MergeMate = DefDatabase<PawnRelationDef>.GetNamed("ExMerged");

        public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
        {
            return LovePartnerRelationUtility.LovePartnerRelationGenerationChance(generated, other, request, ex: true) * BaseGenerationChanceFactor(generated, other, request);
        }

        public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
        {
            generated.relations.AddDirectRelation(MergeMate, other);
        }
    }
    public class PawnRelationWorker_MergeMate : PawnRelationWorker
    {

        public PawnRelationDef MergeMate = DefDatabase<PawnRelationDef>.GetNamed("MergeMate");

        public override float GenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request)
        {
            return LovePartnerRelationUtility.LovePartnerRelationGenerationChance(generated, other, request, ex: true) * BaseGenerationChanceFactor(generated, other, request);
        }

        public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
        {
            generated.relations.AddDirectRelation(MergeMate, other);
        }
    }
}
