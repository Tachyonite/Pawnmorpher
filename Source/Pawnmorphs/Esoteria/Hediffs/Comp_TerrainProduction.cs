// Comp_TerrainProduction.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/12/2019 5:52 PM
// last updated 08/12/2019  6:34 PM

using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    ///     hediff component that produces resources when over a certain area
    /// </summary>
    /// <seealso cref="Pawnmorph.Utilities.HediffCompBase{Pawnmorph.Hediffs.CompProperties_TerrainProduction}" />
    public class Comp_TerrainProduction : HediffCompBase<CompProperties_TerrainProduction>
    {
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Find.CurrentMap == null || Pawn.Map != Find.CurrentMap) return; //prevent ticking on caravan trips 
            var pos = Pawn.Position;

            var terrain = Find.CurrentMap.terrainGrid.TerrainAt(pos);

            var lst = Props.Dict.TryGetValue(terrain);
            if (lst == null) return;
            foreach (var dictEntry in lst)
                if (Rand.MTBEventOccurs(dictEntry.Mtb, 6e4f, 30f))
                    for (var i = 0; i < dictEntry.Amount; i++)
                        GenSpawn.Spawn(dictEntry.Resource, pos, Pawn.Map);
        }
    }

    /// <summary>
    ///     CompProperties for a component that produces resources based on the terrain over a certain area
    /// </summary>
    /// <seealso cref="Pawnmorph.Utilities.HediffCompPropertiesBase{Pawnmorph.Hediffs.Comp_TerrainProduction}" />
    public class CompProperties_TerrainProduction : HediffCompPropertiesBase<Comp_TerrainProduction>
    {
        public List<Entry> entries = new List<Entry>();

        [Unsaved]
        private Dictionary<TerrainDef, List<DictEntry>>
            _dict; //using a list means more then one resource can spawn in a terrain type 

        public Dictionary<TerrainDef, List<DictEntry>> Dict
        {
            get
            {

                if (_dict == null)
                {
                    GenDict();
                }

                return _dict;
            }
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (var configError in base.ConfigErrors(parentDef)) yield return configError;

            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (entry.terrain == null) yield return $"in entries[{index}] terrainDef is null";

                if (entry.resource == null) yield return $"in entries[{index}] resourceDef is null";
            }
        }

        

        void GenDict()
        {

            StringBuilder builder = new StringBuilder(); 
            _dict = new Dictionary<TerrainDef, List<DictEntry>>();
            foreach (var entry in entries)
            {
                if (!_dict.TryGetValue(entry.terrain, out var lst))
                {
                    lst = new List<DictEntry>();
                    _dict[entry.terrain] = lst;
                }

                builder.AppendLine($"loading {entry.resource.defName} on {entry.terrain.defName}"); 

                lst.Add(new DictEntry(entry.resource, entry.mtb, entry.amount, entry.thought));
            }

            Log.Message(builder.ToString()); 
        }

        public class Entry
        {
            public int amount = 1;
            public float mtb = 5; //still not sure how mtb works, seems like an ok default 
            public ThingDef resource;
            public TerrainDef terrain;
            public ThoughtDef thought;
        }


        public class DictEntry
        {
            public DictEntry(ThingDef resource, float mtb, int amount, ThoughtDef thought)
            {
                Resource = resource;
                Mtb = mtb;
                Thought = thought;
                Amount = amount;
            }

            public ThingDef Resource { get; }
            public float Mtb { get; }

            [CanBeNull] public ThoughtDef Thought { get; }

            public int Amount { get; }
        }
    }
}