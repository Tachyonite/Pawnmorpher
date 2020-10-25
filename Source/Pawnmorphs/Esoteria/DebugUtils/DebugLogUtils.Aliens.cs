// DebugLogUtils.Aliens.cs created by Iron Wolf for Pawnmorph on //2020 
// last updated 10/24/2020  12:16 PM

using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using RimWorld;
using UnityEngine.Video;
using Verse;

namespace Pawnmorph.DebugUtils
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class DebugLogUtils
    {
        private const string ALIEN_DEBUG_HEADER = MAIN_CATEGORY_NAME + "- Aliens";

        [DebugOutput(category = ALIEN_DEBUG_HEADER)]
        static void ListMutationsForRaces()
        {
            StringBuilder builder = new StringBuilder(); 
            foreach (ThingDef_AlienRace alien in DefDatabase<ThingDef>.AllDefs.OfType<ThingDef_AlienRace>())
            {
                var ext = alien.GetModExtension<RaceMutationSettingsExtension>();
                if(ext?.mutationRetrievers == null || ext.immuneToAll) continue;

                var mutations = ext.mutationRetrievers.GetMutationsFor(alien, null);

                builder.AppendLine($"{alien.defName} receives the following mutations:");
                foreach (MutationDef mutationDef in mutations)
                {
                    builder.AppendLine(mutationDef.defName); 
                }

                Log.Message(builder.ToString());
                builder.Clear();
            }


        }

        [DebugOutput(category = ALIEN_DEBUG_HEADER)]
        static void CheckInfectibilityOfRaces()
        {
            var mutagen = MutagenDefOf.defaultMutagen;
            foreach (ThingDef_AlienRace alien in DefDatabase<ThingDef>.AllDefsListForReading.OfType<ThingDef_AlienRace>())
            {
                var str = mutagen.MutagenCached.CanInfectDebug(alien);
                if (str.Length == 0)
                {
                    Log.Message($"{alien.defName} can be mutated");
                }
                else
                {
                    Log.Message(str); 
                }
            }
        }


        struct StatData
        {
            public float factor;
            public float offset;


            public static StatData operator +(StatData lhs, StatData rhs)
            {
                return new StatData
                {
                    factor = lhs.factor + rhs.factor,
                    offset = lhs.offset + rhs.offset
                };
            }

            /// <summary>Returns the fully qualified type name of this instance.</summary>
            /// <returns>The fully qualified type name.</returns>
            public override string ToString()
            {
                return $"{factor},{offset}"; 
            }
        }

        [NotNull]
        static 
        IEnumerable<(StatDef, StatData)> CollectData([NotNull] HediffStage stage)
        {

            if (stage.statFactors != null)
            {
                foreach (StatModifier stageStatFactor in stage.statFactors)
                {
                    yield return (stageStatFactor.stat, new StatData() {factor = stageStatFactor.value});
                }
            }

            if (stage.statOffsets != null)
            {
                foreach (StatModifier offset in stage.statOffsets)
                {
                    yield return (offset.stat, new StatData() {offset = offset.value});
                }
            }

        }

        [DebugOutput(ALIEN_DEBUG_HEADER)]
        static void GetRaceMutationStats()
        {
            const string HEADER = "stat,base,factor,offset";  
            StringBuilder builder = new StringBuilder();

            var enumerable = DefDatabase<ThingDef>.AllDefs.Where(td => td is ThingDef_AlienRace && td.race != null)
                                                  .Select(t => (t, t.GetModExtension<RaceMutationSettingsExtension>()))
                                                  .Where(tp => tp.Item2?.mutationRetrievers != null);

            Dictionary<StatDef, StatData> dict = new Dictionary<StatDef, StatData>(); 

            foreach ( (ThingDef thingDef, RaceMutationSettingsExtension ext) in enumerable)
            {
                dict.Clear();
                
                
                builder.Clear();
                builder.AppendLine($"For {thingDef.defName}:");
                builder.AppendLine(HEADER);

                var mutations = ext.mutationRetrievers.GetMutationsFor(thingDef, null);

                foreach (MutationDef mutationDef in mutations)
                {
                    var lastStage = mutationDef.stages?.LastOrDefault(); 
                    if(lastStage == null) continue;

                    var dataSet = CollectData(lastStage);
                    foreach ((StatDef stat , StatData data)  in dataSet)
                    {
                        dict[stat] = data + dict.TryGetValue(stat); 
                    }

                }

                var raceProps = thingDef.race; 
                foreach (KeyValuePair<StatDef, StatData> keyValuePair in dict)
                {
                    var baseVal = thingDef.statBases?.FirstOrDefault(s => s.stat == keyValuePair.Key)?.value ?? 0;
                    builder.AppendLine($"{keyValuePair.Key.defName},{baseVal},{keyValuePair.Value}");
                }

                Log.Message(builder.ToString());
                builder.Clear(); 
            }

        }

    }
}