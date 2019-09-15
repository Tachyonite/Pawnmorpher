// Pawnmorpher_DebugDialogue.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 7:29 PM
// last updated 08/02/2019  7:29 PM

using System.Collections.Generic;
using System.Linq;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.Hybrids;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.DebugUtils
{
    public class Pawnmorpher_DebugDialogue : Dialog_DebugOptionLister
    {
        protected override void DoListingItems()
        {
            if (KeyBindingDefOf.Dev_ToggleDebugActionsMenu.KeyDownEvent)
            {
                Event.current.Use();
                Close(true); 
            }

            if (Current.ProgramState == ProgramState.Playing)
            {
                if (Find.CurrentMap != null)
                {
                    ListPlayOptions();
                }
            }
        }

        void ListPawnInitialGraphics(Pawn pawn)
        {
            var initialComp = pawn.GetComp<GraphicSys.InitialGraphicsComp>();
            if (initialComp == null) return ;

            Log.Message(initialComp.GetDebugStr()); 
        }

        public Pawnmorpher_DebugDialogue()
        {
            forcePause = true; 
        }

        void ListPlayOptions()
        {
            DebugAction("shift race", () => { Find.WindowStack.Add(new Dialog_DebugOptionListLister(GetRaceChangeOptions())); });
            DebugToolMapForPawns("give random mutations", GetRandomMutationsOptions);
            DebugToolMapForPawns("force full transformation", ForceTransformation);
            DebugToolMapForPawns("get initial graphics", ListPawnInitialGraphics); 
        }

        void ForceTransformation(Pawn pawn)
        {
            var morphHediff = pawn?.health.hediffSet.hediffs.FirstOrDefault(h => h is Hediff_Morph);
            if (morphHediff != null)
            {
                var giverTf = morphHediff.def.stages?.SelectMany(s => s.hediffGivers ?? Enumerable.Empty<HediffGiver>())
                                         .OfType<HediffGiver_TF>()
                                         .FirstOrDefault();

                giverTf?.TryTf(pawn, morphHediff);

            }


        }


        void GivePawnRandomMutations(Pawn pawn, [CanBeNull] MorphDef morph)
        {

            var mutations = morph?.AssociatedMutations;
            if (mutations == null)
            {
                mutations = DefDatabase<HediffDef>.AllDefs
                                                  .Where(d => typeof(Hediff_Morph).IsAssignableFrom(d.hediffClass))
                                                  .SelectMany(d => d.GetAllHediffGivers())
                                                  .OfType<HediffGiver_Mutation>(); 


            }

            bool CanReceiveGiver(HediffGiver_Mutation mutation)
            {
                var hediffs = pawn.health.hediffSet.hediffs.Where(h => h.def == mutation.hediff);

                return hediffs.Count() < mutation.countToAffect;
            }

            List<HediffGiver_Mutation> mutList = mutations.Where(CanReceiveGiver).ToList();
            if (mutList.Count == 0) return;

            int num = Rand.Range(1, Mathf.Min(10, mutList.Count));

            var i = 0;
            while (i < num && mutList.Count > 0)
            {
                HediffGiver_Mutation giver = mutList.RandElement();
                mutList.Remove(giver);


                giver.TryApply(pawn, MutagenDefOf.defaultMutagen);

                i++;
            }



        }


        void GetRandomMutationsOptions(Pawn pawn)
        {
            List<DebugMenuOption> options = new List<DebugMenuOption>()
                {new DebugMenuOption("none", DebugMenuOptionMode.Tool, () => GivePawnRandomMutations(pawn, null))};


            foreach (MorphDef morphDef in MorphDef.AllDefs)
            {
                var option = new DebugMenuOption(morphDef.label, DebugMenuOptionMode.Tool,
                                                 () => GivePawnRandomMutations(pawn, morphDef)); 
                options.Add(option);



            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));


        }


        List<DebugMenuOption> GetRaceChangeOptions()
        {
            //var races = RaceGenerator.ImplicitRaces;
            List<DebugMenuOption> lst = new List<DebugMenuOption>(); 
            foreach (var morph in DefDatabase<MorphDef>.AllDefs)
            {
                var local = morph; 

                lst.Add(new DebugMenuOption(local.label, DebugMenuOptionMode.Tool, () =>
                {
                    var pawn = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).OfType<Pawn>().FirstOrDefault();
                    if (pawn != null && pawn.RaceProps.intelligence == Intelligence.Humanlike)
                    {
                        RaceShiftUtilities.ChangePawnToMorph(pawn, local); 
                    }
                    
                }));

            }

            return lst; 

        }


    }
}