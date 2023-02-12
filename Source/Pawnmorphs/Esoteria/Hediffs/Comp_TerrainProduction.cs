// Comp_TerrainProduction.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/12/2019 5:52 PM
// last updated 08/12/2019  6:34 PM

using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	///     hediff component that produces resources when over a certain area
	/// </summary>
	/// <seealso>
	///     <cref>Pawnmorph.Utilities.HediffCompBase{Pawnmorph.Hediffs.CompProperties_TerrainProduction}</cref>
	/// </seealso>
	public class Comp_TerrainProduction : HediffCompBase<CompProperties_TerrainProduction>
	{

		/// <summary>
		/// produces the corrected produced based on the current position of the pawn
		/// </summary>
		public void ProduceNow()
		{
			var pos = Pawn.PositionHeld;

			var terrain = Find.CurrentMap.terrainGrid.TerrainAt(pos);

			var lst = Props.Dict.TryGetValue(terrain);
			if (lst == null) return;

			var elem = lst.RandElement();

			Produce(pos, elem);

		}

		private void TryProduce()
		{
			var pos = Pawn.PositionHeld;

			var terrain = Find.CurrentMap.terrainGrid.TerrainAt(pos);

			var lst = Props.Dict.TryGetValue(terrain);
			if (lst == null) return;

			var elem = lst.RandElement();

			if (Rand.MTBEventOccurs(elem.Mtb, 6E+4f, 60)) //can't check mtb more then once per tick 
			{
				Produce(pos, elem);
			}
		}

		private void Produce(IntVec3 pos, CompProperties_TerrainProduction.DictEntry productionElement)
		{
			Thing thing = ThingMaker.MakeThing(productionElement.Resource);
			var statValue = Pawn.GetStatValue(StatDefOf.PlantHarvestYield);
			thing.stackCount = Mathf.RoundToInt(productionElement.Amount * statValue);
			if (thing.stackCount > 0)
				GenPlace.TryPlaceThing(thing, pos, Pawn.Map, ThingPlaceMode.Near);
		}
	}

	/// <summary>
	///     CompProperties for a component that produces resources based on the terrain over a certain area
	/// </summary>
	/// <seealso>
	///     <cref>Pawnmorph.Utilities.HediffCompPropertiesBase{Pawnmorph.Hediffs.Comp_TerrainProduction}</cref>
	/// </seealso>
	public class CompProperties_TerrainProduction : HediffCompPropertiesBase<Comp_TerrainProduction>
	{
		/// <summary>
		/// all entries that are used by the comp 
		/// </summary>
		public List<Entry> entries = new List<Entry>();

		[Unsaved]
		private Dictionary<TerrainDef, List<DictEntry>>
			_dict; //using a list means more then one resource can spawn in a terrain type 

		/// <summary>
		/// returns true if this comp can produce something on the given terrain 
		/// </summary>
		/// <param name="terrain"></param>
		/// <returns></returns>
		public bool CanProduceOn([NotNull] TerrainDef terrain)
		{
			return Dict.ContainsKey(terrain);
		}

		internal Dictionary<TerrainDef, List<DictEntry>> Dict
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
		/// <summary>
		/// return all configuration errors with this instance 
		/// </summary>
		/// <param name="parentDef"></param>
		/// <returns></returns>
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
		/// <summary>
		/// a single production entry for
		/// </summary>
		public class Entry
		{
			/// <summary>
			/// the amount to produce 
			/// </summary>
			public int amount = 1;
			/// <summary>
			/// how often to produce this product 
			/// </summary>
			public float mtb = 5; //still not sure how mtb works, seems like an ok default 
			/// <summary>
			/// the resource to produce 
			/// </summary>
			public ThingDef resource;
			/// <summary>
			/// the terrain this is produced by 
			/// </summary>
			public TerrainDef terrain;
			/// <summary>
			/// the thought to add when the resource is produced 
			/// </summary>
			public ThoughtDef thought;
		}


		internal class DictEntry
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