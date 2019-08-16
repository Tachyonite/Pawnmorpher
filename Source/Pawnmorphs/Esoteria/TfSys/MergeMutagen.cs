// MergeMutagen.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/14/2019 3:15 PM
// last updated 08/14/2019  3:15 PM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Thoughts;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
using Verse.Noise;

namespace Pawnmorph.TfSys
{
    /// <summary>
    /// implementation of mutagen that merges 2 or more pawns into a single meld 
    /// </summary>
    /// <seealso cref="Pawnmorph.TfSys.Mutagen{Pawnmorph.TfSys.MergedPawns}" />
    public class MergeMutagen : Mutagen<MergedPawns>
    {
        private const string FORMER_HUMAN_HEDIFF = "2xMergedHuman";//can't put this in a hediffDefOf because of the name 

        private List<Pawn> _scratchList = new List<Pawn>(); 


        /// <summary>
        /// Transforms the pawns into a TransformedPawn instance of the given ace .
        /// </summary>
        /// <param name="originals">The originals.</param>
        /// <param name="outputPawnKind"></param>
        /// <param name="forcedGender"></param>
        /// <param name="forcedGenderChance"></param>
        /// <param name="cause">The cause.</param>
        /// <param name="tale"></param>
        /// <returns></returns>
        protected override MergedPawns TransformPawnsImpl(IEnumerable<Pawn> originals, PawnKindDef outputPawnKind,
            TFGender forcedGender,
            float forcedGenderChance, Hediff_Morph cause, TaleDef tale)
        {
            _scratchList.Clear();
            _scratchList.AddRange(originals);
            if (_scratchList.Count != 2)
            {
                Log.Warning($"trying to merge {_scratchList.Count} pawns, this is not currently supported!");
                return null; 
            }

            Pawn firstPawn = _scratchList[0];
            Pawn secondPawn = _scratchList[1];
            var averageAge = firstPawn.ageTracker.AgeBiologicalYearsFloat
                           + secondPawn.ageTracker.AgeBiologicalYearsFloat;
            averageAge /= 2;


            var newAge = averageAge * outputPawnKind.race.race.lifeExpectancy / firstPawn.RaceProps.lifeExpectancy;

            var request = new PawnGenerationRequest(outputPawnKind, Faction.OfPlayer,
                                                    PawnGenerationContext.NonPlayer, -1, false,
                                                    false, false, false, true, false, 1f,
                                                    false, true, true, false, false, false,
                                                    false, null, null, null,
                                                    new float?(newAge), averageAge);


            var meldToSpawn = PawnGenerator.GeneratePawn(request);

            var needs = meldToSpawn.needs;
            needs.food.CurLevel = firstPawn.needs.food.CurLevel;
            needs.rest.CurLevel = firstPawn.needs.rest.CurLevel;
            meldToSpawn.training.SetWantedRecursive(TrainableDefOf.Obedience, true);
            meldToSpawn.training.Train(TrainableDefOf.Obedience, null, true);
            meldToSpawn.Name = firstPawn.Name;
            var meld = (Pawn) GenSpawn.Spawn(meldToSpawn, firstPawn.PositionHeld, firstPawn.MapHeld, 0);
            for (int i = 0; i < 10; i++)
            {
                IntermittentMagicSprayer.ThrowMagicPuffDown(meld.Position.ToVector3(), meld.MapHeld);
                IntermittentMagicSprayer.ThrowMagicPuffUp(meld.Position.ToVector3(), meld.MapHeld);
            }

            meld.SetFaction(Faction.OfPlayer);

            var hediffToAdd = HediffDef.Named(FORMER_HUMAN_HEDIFF);

            var hediff = HediffMaker.MakeHediff(hediffToAdd, meld);
            hediff.Severity = Rand.Range(0, 1f);
            meld.health.AddHediff(hediff);
            
            ReactionsHelper.OnPawnsMerged(firstPawn, firstPawn.IsPrisoner, secondPawn, secondPawn.IsPrisoner, meld);



            TransformerUtility.CleanUpHumanPawnPostTf(firstPawn, null);
            TransformerUtility.CleanUpHumanPawnPostTf(secondPawn, null); 

            var inst = new MergedPawns()
            {
                originals = _scratchList.ToList(),
                meld = meld,
                mutagenDef = def
            };
            return inst; 
            
        }

        /// <summary>
        /// Determines whether this instance can revert pawn the specified transformed pawn.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns>
        ///   <c>true</c> if this instance can revert pawn  the specified transformed pawn; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanRevertPawnImp(MergedPawns transformedPawn)
        {
            if (!transformedPawn.IsValid) return false;

            var def = HediffDef.Named(FORMER_HUMAN_HEDIFF);

            return transformedPawn.meld.health.hediffSet.HasHediff(def); 

        }

        /// <summary>
        /// Tries to revert the transformed pawn instance, implementation.
        /// </summary>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <returns></returns>
        protected override bool TryRevertImpl(MergedPawns transformedPawn)
        {
            if (!transformedPawn.IsValid) return false; 


            _scratchList.Clear();
            if (transformedPawn.originals.Count != 2) return false;
            var hDef = HediffDef.Named(FORMER_HUMAN_HEDIFF); 
            var meld = transformedPawn.meld;

            var formerHumanHediff = meld.health.hediffSet.hediffs.FirstOrDefault(h => h.def == hDef);
            if (formerHumanHediff == null) return false; 



            var mergMateDef = DefDatabase<PawnRelationDef>.GetNamed("MergeMate");
            var mergeMateEx = DefDatabase<PawnRelationDef>.GetNamed("ExMerged"); //TODO put these in a DefOf 


            var firstO = (Pawn) GenSpawn.Spawn(transformedPawn.originals[0], meld.PositionHeld, meld.MapHeld);
            var secondO = (Pawn) GenSpawn.Spawn(transformedPawn.originals[1], meld.PositionHeld, meld.MapHeld);

            var firstThought =  AddRandomThought(firstO, formerHumanHediff.Severity);
            AddRandomThought(secondO, formerHumanHediff.Severity);

            for (int i = 0; i < 10; i++)
            {
                IntermittentMagicSprayer.ThrowMagicPuffDown(meld.Position.ToVector3(), meld.MapHeld);
                IntermittentMagicSprayer.ThrowMagicPuffUp(meld.Position.ToVector3(), meld.MapHeld);
            }


            PawnRelationDef addDef; 

            addDef = firstThought.def == def.reversionThoughts[0] ? mergMateDef : mergeMateEx; //first element is "WasMerged"

            firstO.relations.AddDirectRelation(addDef, secondO);

            firstO.SetFaction(Faction.OfPlayer);
            secondO.SetFaction(Faction.OfPlayer);


            ReactionsHelper.OnPawnReverted(firstO, meld);
            ReactionsHelper.OnPawnReverted(secondO, meld); 
            meld.DeSpawn();

            return true; 

           
        }

        Hediff AddRandomThought(Pawn p, float formerHumanSeverity)
        {
            var thougth = def.reversionThoughts.RandElement();
            var hediff = HediffMaker.MakeHediff(thougth, p);

            hediff.Severity = formerHumanSeverity;
            p.health.AddHediff(hediff);

            return hediff; 

        }

    }
}