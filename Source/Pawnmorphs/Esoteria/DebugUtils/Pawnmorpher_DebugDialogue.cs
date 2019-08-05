// Pawnmorpher_DebugDialogue.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 7:29 PM
// last updated 08/02/2019  7:29 PM

using System.Collections.Generic;
using System.Linq;
using AlienRace;
using Pawnmorph.Hybrids;
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

        public Pawnmorpher_DebugDialogue()
        {
            forcePause = true; 
        }

        void ListPlayOptions()
        {
            DebugAction("shift race", () => { Find.WindowStack.Add(new Dialog_DebugOptionListLister(GetRaceChangeOptions())); }); 
        }

        List<DebugMenuOption> GetRaceChangeOptions()
        {
            var races = RaceGenerator.ImplicitRaces;
            List<DebugMenuOption> lst = new List<DebugMenuOption>(); 
            foreach (ThingDef_AlienRace thingDefAlienRace in races)
            {
                var local = thingDefAlienRace; 

                lst.Add(new DebugMenuOption(local.label, DebugMenuOptionMode.Tool, () =>
                {
                    var pawn = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).OfType<Pawn>().FirstOrDefault();
                    if (pawn != null && pawn.RaceProps.intelligence == Intelligence.Humanlike)
                    {
                        RaceShiftUtilities.ChangePawnRace(pawn, local, true); 
                    }




                }));

            }

            return lst; 

        }


    }
}