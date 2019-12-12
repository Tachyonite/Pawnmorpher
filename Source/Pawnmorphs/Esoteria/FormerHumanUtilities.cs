// FormerHumanUtilities.cs modified by Iron Wolf for Pawnmorph on 12/08/2019 7:56 AM
// last updated 12/08/2019  7:56 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    ///     static class containing various former human utilities
    /// </summary>
    public static class FormerHumanUtilities
    {
        /// <summary>
        ///     Gets all former humans on all maps
        /// </summary>
        /// <value>
        ///     All maps player former humans.
        /// </value>
        [NotNull]
        public static IEnumerable<Pawn> AllMaps_FormerHumans
        {
            get
            {
                foreach (Pawn allMap in PawnsFinder.AllMaps)
                    if (allMap.GetFormerHumanStatus() != null)
                        yield return allMap;
            }
        }

        /// <summary>Gets the sapience level of this pawn</summary>
        /// <param name="formerHuman">The former human.</param>
        /// <returns>the sapience level. If feral this is 0, if the given pawn is not a former human returns null</returns>
        public static float? GetSapienceLevel([NotNull] this Pawn formerHuman)
        {
            var fHumanStatus = formerHuman.GetFormerHumanStatus();
            switch (fHumanStatus)
            {
                case FormerHumanStatus.Sapient:
                    return formerHuman.needs.TryGetNeed<Need_Control>()?.CurLevel;
                case FormerHumanStatus.Feral:
                    return 0;
                case FormerHumanStatus.PermanentlyFeral:
                    return 0;
                case null:
                    return null; 
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [NotNull]
        private static readonly float[] _sapienceThresholds; //these are the minimum sapience levels needed to fall withing a given enum level 

        static FormerHumanUtilities()
        {
            var values = new SapienceLevel[]
            {
                SapienceLevel.Sapient,
                SapienceLevel.MostlySapient,
                SapienceLevel.Conflicted,
                SapienceLevel.MostlyFeral,
                SapienceLevel.Feral
            };

            float delta = 1f/values.Length;
            float counter = 1;
            _sapienceThresholds = new float[values.Length];
            foreach (SapienceLevel sapienceLevel in values) //split up the level thresholds evenly between 1,0 starting at sapient 
            {
                counter -= delta;
                _sapienceThresholds[(int) sapienceLevel] = counter; 
            }


        }


        /// <summary>Gets the quantized sapience level.</summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>returns null if the pawn isn't a former human</returns>
        public static SapienceLevel? GetQuantizedSapienceLevel([NotNull] this Pawn pawn)
        {
            var sLevel = GetSapienceLevel(pawn);
            if (sLevel == null) return null;
            if (pawn.GetFormerHumanStatus() == FormerHumanStatus.PermanentlyFeral) return SapienceLevel.PermanentlyFeral;
            for (var index = 0; index < _sapienceThresholds.Length; index++)
            {
                float sapienceThreshold = _sapienceThresholds[index];
                if (sLevel > sapienceThreshold) return (SapienceLevel) index; 
            }

            return SapienceLevel.Feral;
        }

        /// <summary>
        /// Gets the original pawn of the given former human.
        /// </summary>
        /// <param name="formerHuman">The former human.</param>
        /// <returns>the original pawn if it exists, otherwise null</returns>
        [CanBeNull]
        public static Pawn GetOriginalPawnOfFormerHuman([NotNull] Pawn formerHuman)
        {
            foreach (var tfPawn in Find.World.GetComponent<PawnmorphGameComp>().TransformedPawns)
            {
                if (tfPawn.TransformedPawns.Contains(formerHuman)) return tfPawn.OriginalPawns.FirstOrDefault();
            }

            return null; 
        }

        /// <summary>
        ///     Gets all former humans on all maps, caravans and traveling transport pods that are alive
        /// </summary>
        /// <value>
        ///     all former humans on all maps, caravans and traveling transport pods that are alive
        /// </value>
        [NotNull]
        public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive
        {
            get
            {
                foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
                    if (pawn.GetFormerHumanStatus() != null)
                        yield return pawn;
            }
        }

        /// <summary>
        ///     Gets all former humans belonging to the player
        /// </summary>
        /// <value>
        ///     All player former humans.
        /// </value>
        [NotNull]
        public static IEnumerable<Pawn> AllPlayerFormerHumans
        {
            get
            {
                foreach (Pawn pawn in AllMapsCaravansAndTravelingTransportPods_Alive)
                    if (pawn.Faction == Faction.OfPlayer)
                        yield return pawn;
            }
        }


        /// <summary>
        ///     Gets all sapient animals that are at risk of a minor break .
        /// </summary>
        /// <value>
        ///     All sapient animals minor break risk.
        /// </value>
        [NotNull]
        public static IEnumerable<Pawn> AllSapientAnimalsMinorBreakRisk
        {
            get
            {
                foreach (Pawn allPlayerFormerHuman in AllPlayerFormerHumans)
                {
                    Comp_SapientAnimal saComp = allPlayerFormerHuman.GetSapientAnimalComp();
                    if (saComp?.MentalBreaker?.BreakMinorIsImminent == true) yield return allPlayerFormerHuman;
                }
            }
        }

        /// <summary>
        ///     Gets all sapient animals that are at risk of a major break .
        /// </summary>
        /// <value>
        ///     All sapient animals that are at risk of a major break .
        /// </value>
        [NotNull]
        public static IEnumerable<Pawn> AllSapientAnimalsMajorBreakRisk
        {
            get
            {
                foreach (Pawn allPlayerFormerHuman in AllPlayerFormerHumans)
                {
                    Comp_SapientAnimal saComp = allPlayerFormerHuman.GetSapientAnimalComp();
                    if (saComp?.MentalBreaker?.BreakMajorIsImminent == true) yield return allPlayerFormerHuman;
                }
            }
        }


        /// <summary>
        ///     Gets all sapient animals at risk of an extreme break.
        /// </summary>
        /// <value>
        ///     All sapient animals at risk of an extreme break.
        /// </value>
        [NotNull]
        public static IEnumerable<Pawn> AllSapientAnimalsExtremeBreakRisk
        {
            get
            {
                foreach (Pawn allPlayerFormerHuman in AllPlayerFormerHumans)
                {
                    Comp_SapientAnimal saComp = allPlayerFormerHuman.GetSapientAnimalComp();
                    if (saComp?.MentalBreaker?.BreakExtremeIsImminent == true) yield return allPlayerFormerHuman;
                }
            }
        }

        /// <summary>
        ///     Gets the break alert label for sapient animals
        /// </summary>
        /// <value>
        ///     The break alert label.
        /// </value>
        [NotNull]
        public static string BreakAlertLabel
        {
            get
            {
                int num = AllSapientAnimalsExtremeBreakRisk.Count();
                string text;
                if (num > 0)
                {
                    text = "BreakRiskExtreme".Translate();
                }
                else
                {
                    num = AllSapientAnimalsMajorBreakRisk.Count();
                    if (num > 0)
                    {
                        text = "BreakRiskMajor".Translate();
                    }
                    else
                    {
                        num = AllSapientAnimalsMinorBreakRisk.Count();
                        text = "BreakRiskMinor".Translate();
                    }
                }

                if (num > 1) text = text + " x" + num.ToStringCached();
                return text;
            }
        }

        /// <summary>
        ///     Gets the break alert explanation for sapient animals .
        /// </summary>
        /// <value>
        ///     The break alert explanation.
        /// </value>
        [NotNull]
        public static string BreakAlertExplanation
        {
            get
            {
                var stringBuilder = new StringBuilder();
                if (AllSapientAnimalsExtremeBreakRisk.Any())
                {
                    var stringBuilder2 = new StringBuilder();
                    foreach (Pawn current in AllSapientAnimalsExtremeBreakRisk)
                        stringBuilder2.AppendLine("    " + current.LabelShort);
                    stringBuilder.Append("BreakRiskExtremeDesc".Translate(stringBuilder2));
                }

                if (AllSapientAnimalsMajorBreakRisk.Any())
                {
                    if (stringBuilder.Length != 0) stringBuilder.AppendLine();
                    var stringBuilder3 = new StringBuilder();
                    foreach (Pawn current2 in AllSapientAnimalsMajorBreakRisk)
                        stringBuilder3.AppendLine("    " + current2.LabelShort);
                    stringBuilder.Append("BreakRiskMajorDesc".Translate(stringBuilder3));
                }

                if (AllSapientAnimalsMinorBreakRisk.Any())
                {
                    if (stringBuilder.Length != 0) stringBuilder.AppendLine();
                    var stringBuilder4 = new StringBuilder();
                    foreach (Pawn current3 in AllSapientAnimalsMinorBreakRisk)
                        stringBuilder4.AppendLine("    " + current3.LabelShort);
                    stringBuilder.Append("BreakRiskMinorDesc".Translate(stringBuilder4));
                }

                stringBuilder.AppendLine();
                stringBuilder.Append("BreakRiskDescEnding".Translate());
                return stringBuilder.ToString();
            }
        }
    }
}