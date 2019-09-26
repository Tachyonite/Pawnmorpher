// SpreadingMutationComp.cs modified by Iron Wolf for Pawnmorph on 09/25/2019 6:47 PM
// last updated 09/25/2019  6:47 PM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils;
namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// hediff comp for making a mutation spread over a body 
    /// </summary>
    public class SpreadingMutationComp : HediffCompBase<SpreadingMutationCompProperties>
    {
        private bool _finishedSearching;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (_finishedSearching) return;

            if (!Rand.MTBEventOccurs(Props.mtb, 60000f, 30f)) return;


            if (TryInfectPart(parent.Part, false)) return; //try infecting downward first 
            if (TryInfectPart(parent.Part)) return; //try infecting upward

            _finishedSearching = true; //searched all available parts, don't bother looking anymore 

        }


        bool IsBlocker(Hediff h)
        {
            if (h.def == Def)
            {
                Log.Message($"{h.Label} is a blocker for {parent.Label}");

                return true;
            }
            var tComp = parent.TryGetComp<RemoveFromPartComp>();
            Assert(tComp != null, "tComp != null");
            var oComp = h.TryGetComp<RemoveFromPartComp>();
            if (oComp == null) return false; //can't be a blocker if it doesn't have a remover comp 


            if ((oComp.Layer & tComp.Layer) != 0)
            {
                return oComp.AddedTick > tComp.AddedTick;
                
            }

            return false; 

            //to add the hediff must be on a different layer or be younger then the other hediff, thus capable of removing it 

        }

        /// <summary>
        /// try to infect a single part 
        /// </summary>
        /// <param name="record"></param>
        /// <param name="upward"></param>
        /// <param name="depth"></param>
        /// <returns>true if a part could be successfully infected </returns>
        bool TryInfectPart(BodyPartRecord record, bool upward = true, int depth = 0)
        {
            if (CanInfect(record))
            {
                var hediff = HediffMaker.MakeHediff(Def, Pawn, record);
                Pawn.health.AddHediff(hediff, record); 
                return true; 
            }

            if (depth >= Props.maxTreeSearchDepth)
            {
                return false; //at max depth, don't go any farther 
            }

            //recurse up or down the tree 
            if (upward)
            {
                if (record.parent == null) return false; //no parent just return false 
                int add = MorphUtilities.AllMutableRecords.Contains(record.parent) && IsSkinPart(record.parent) ? 1 : 0;

                return TryInfectPart(record.parent, true, depth + add); 
            }
            else if(record.parts != null)
            {
                foreach (BodyPartRecord bodyPartRecord in record.parts)
                {
                    int add = MorphUtilities.AllMutableRecords.Contains(bodyPartRecord) && IsSkinPart(bodyPartRecord) ? 1 : 0; 
                    if (TryInfectPart(bodyPartRecord, false, depth + add))
                    {
                        return true; //abort on the first successful add 
                    }
                }
            }

            return false; 


        }


        bool IsSkinPart(BodyPartRecord record)
        {
            return record?.def?.IsSkinCovered(record, Pawn.health.hediffSet) ?? false;
        }



        bool CanInfect(BodyPartRecord record)
        {
            if (!IsSkinPart(record)) return false; 

            if (!MorphUtilities.AllMutableRecords.Contains(record))
            {

                return false;
            }


            return !Pawn.health.hediffSet.hediffs.Any(h => h.Part == record && IsBlocker(h)); //only true if the part does not already have a part on it 
        }


        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref _finishedSearching, nameof(_finishedSearching)); 
        }
    }

    public class SpreadingMutationCompProperties : HediffCompPropertiesBase<SpreadingMutationComp>
    {
        public int maxTreeSearchDepth = 2; //how far from the parent's part will this comp search for a part to spread to
        public float mtb = 160; 
    }
}