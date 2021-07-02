// SheepChef.cs created by Iron Wolf for Pawnmorph on 01/05/2021 4:30 PM
// last updated 01/05/2021  4:30 PM

using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.IncidentWorkers
{
    /// <summary>
    /// </summary>
    /// <seealso cref="RimWorld.IncidentWorker" />
    public class SheepChef : IncidentWorker
    {
        /// <summary>
        ///     Determines whether this instance with the specified parms [can fire now sub]
        /// </summary>
        /// <param name="parms">The parms.</param>
        /// <returns>
        ///     <c>true</c> if this instance with the specified parms  [can fire now sub]  otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (parms?.forced == true) return true;

            bool fired = Find.World?.GetComponent<PawnmorphGameComp>()?.sheepChefEventFired ?? false;

            return !fired;
        }

        /// <summary>
        ///     Tries the execute worker.
        /// </summary>
        /// <param name="parms">The parms.</param>
        /// <returns></returns>
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map) parms.target;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 result, map, CellFinder.EdgeRoadChance_Animal)) return false;

            PawnKindDef kind = PMPawnKindDefOf.Sheep;


            IntVec3 loc = CellFinder.RandomClosewalkCellNear(result, map, 12);
            Pawn pawn = PawnGenerator.GeneratePawn(kind);
            GenSpawn.Spawn(pawn, loc, map, Rot4.Random);
            pawn.SetFaction(Faction.OfPlayer);
            var oPawn = GenerateGordon(pawn);
            

            FormerHumanUtilities.MakeAnimalSapient(oPawn, pawn);

            pawn.Name = oPawn.Name ?? pawn.Name; 
            if (pawn.story != null)
            {
                pawn.story.adulthood = PMBackstoryDefOf.PM_SheepChef.backstory; 
            }
            
            if(pawn.skills != null)
            {
                var record = pawn.skills.GetSkill(SkillDefOf.Cooking);
                record.passion = Passion.Major;
                record.Level = Mathf.Max(5, record.Level); 
            }

            SendStandardLetter("PMSheepChefLetterLabel".Translate(kind.label).CapitalizeFirst(),
                               "PMSheepChefLetter".Translate(kind.label), LetterDefOf.PositiveEvent, parms,
                               new TargetInfo(result, map));
            var wComp = Find.World.GetComponent<PawnmorphGameComp>();
            wComp.sheepChefEventFired = true;


            var pm = TfSys.TransformedPawn.Create(oPawn, pawn); 
            Find.World.GetComponent<PawnmorphGameComp>().AddTransformedPawn(pm);

            return true;
        }

        private static TraitDef[] _forcedTraitDef = null; 

        [NotNull]
        private static TraitDef[] ForcedTraits {
            get
            {
                if (_forcedTraitDef == null)
                {
                    _forcedTraitDef = new[] {TraitDefOf.Abrasive, TraitDef.Named("Gourmand")};
                }

                return _forcedTraitDef; 
            }  } 

        [NotNull]
        Pawn GenerateGordon(Pawn animal)
        {
            PawnKindDef kind = PawnKindDefOf.Colonist; //TODO get these randomly ;
            Faction faction = Faction.OfPlayer;
            bool useFirst = Rand.Bool;
            string firstName, lastName;
            firstName = lastName = null;
            if (useFirst)
                firstName = "Gordon";
            else
                lastName = "Ramsey";

  

            float convertedAge = Mathf.Max(TransformerUtility.ConvertAge(animal, ThingDefOf.Human.race), 17);
            float chronoAge = animal.ageTracker.AgeChronologicalYears * convertedAge / animal.ageTracker.AgeBiologicalYears;
            var local = new PawnGenerationRequest(kind, faction, PawnGenerationContext.NonPlayer, -1,
                                                  fixedChronologicalAge: chronoAge,
                                                  fixedBiologicalAge: convertedAge, fixedBirthName:firstName,
                                                  fixedLastName: lastName, fixedGender: Gender.Male, forcedTraits: ForcedTraits) {ForcedTraits = ForcedTraits, ValidatorPreGear = GordenValidator};
            


            
            Pawn lPawn = PawnGenerator.GeneratePawn(local);

            var name = lPawn.Name as NameTriple;
            lPawn.Name = new NameTriple(firstName, name?.Nick ?? firstName, lastName); 


            if (!BackstoryDatabase.TryGetWithIdentifier("chef", out Backstory back))
            {
            }
            else
            {
                lPawn.story.adulthood = back; 
            }

            AssignMutations(lPawn); 
            return lPawn; 
        }

        private bool GordenValidator(Pawn obj)
        {
            TraitSet storyTraits = obj?.story?.traits;
            return storyTraits?.HasTrait(TraitDefOf.Abrasive) == true && storyTraits.HasTrait(TraitDef.Named("Gourmand")) ;
        }

        private void AssignMutations(Pawn gordon)
        {
            
            MutationUtilities.AddAllMorphMutations(gordon, MorphDefOfs.SheepMorph, MutationUtilities.AncillaryMutationEffects.None).SetAllToNaturalMax();
        }
    }
}