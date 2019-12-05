// Worker_BeastMaster.cs modified by Iron Wolf for Pawnmorph on 12/04/2019 8:48 PM
// last updated 12/04/2019  8:48 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.Thoughts
{
    /// <summary>
    ///     thought worker for the beast master aspect
    /// </summary>
    /// <seealso cref="RimWorld.ThoughtWorker" />
    public class Worker_BeastMaster : ThoughtWorker
    {
        [NotNull] private readonly List<string> _animalNames = new List<string>();

        /// <summary>
        ///     Gets the current thought state
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            _animalNames.Clear();
            GetAnimals(p, _animalNames);
            if (_animalNames.Any())
                return false;
            if (_animalNames.Count == 1)
                return ThoughtState.ActiveAtStage(0, _animalNames[0]);
            return ThoughtState.ActiveAtStage(1, _animalNames.ToCommaList(true));
        }

        private bool AnimalMasterCheck(Pawn p, Pawn animal)
        {
            return animal.playerSettings.RespectedMaster == p;
        }

        private void GetAnimals([NotNull] Pawn p, [NotNull] List<string> outAnimals)
        {
            outAnimals.Clear();
            foreach (Pawn pawn in PawnsFinder.AllMapsWorldAndTemporary_Alive)
            {
                if (pawn.playerSettings?.Master != p) continue;
                if (pawn.GetFormerHumanStatus() == null) continue;

                if (pawn.Faction != p.Faction) continue;

                if (AnimalMasterCheck(p, pawn))
                    outAnimals.Add(pawn.Name?.ToStringFull ?? pawn.LabelShort);
            }
        }


        private int GetAnimalsCount(Pawn p)
        {
            _animalNames.Clear();
            GetAnimals(p, _animalNames);
            return _animalNames.Count;
        }
    }
}