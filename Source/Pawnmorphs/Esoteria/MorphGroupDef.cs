// MorphGroupDef.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 09/09/2019 9:07 AM
// last updated 09/09/2019  9:07 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// Def for morph groups. <br/>
	/// i.e. Packs, Herds, ect.
	/// </summary>
	public class MorphGroupDef : Def
	{
		/// <summary> A list of all morph types that are of this group. </summary>
		[Unsaved] private List<MorphDef> _associatedMorphs;

		[Unsaved] private List<ThingDef> _associatedFeralRaces;


		/// <summary>
		/// The barrak thought replacement
		/// </summary>
		[CanBeNull] public ThoughtDef barrakThoughtReplacement;
		/// <summary>
		/// The bedroom thought replacement
		/// </summary>
		[CanBeNull] public ThoughtDef bedroomThoughtReplacement;
		/// <summary>
		/// The room thought for ascetics
		/// </summary>
		[CanBeNull] public ThoughtDef asceticRoomThought;


		/// <summary>
		/// Gets the animal races in this morph group
		/// </summary>
		/// <value>
		/// The animal races.
		/// </value>
		[NotNull]
		public IReadOnlyList<ThingDef> AnimalRaces
		{
			get
			{
				if (_associatedFeralRaces == null)
				{
					var morphAnimals =
						DefDatabase<ThingDef>.AllDefs.Where(d => d.race?.Animal == true && MorphsInGroup.Any(m => m.race == d));

					_associatedFeralRaces = animalRaces.MakeSafe()
													   .Concat(morphAnimals)
													   .Concat(MorphsInGroup.SelectMany(m => m.associatedAnimals.MakeSafe()))
													   .Distinct()
													   .ToList();
				}

				return _associatedFeralRaces;
			}
		}

		/// <summary>
		/// The animal races that count toward this group 
		/// </summary>
		public List<ThingDef> animalRaces;

		///hediff to give to morphs in this group,
		[CanBeNull]
		[Obsolete("use the new aspects")]
		public HediffDef hediff;

		/// <summary>The aspect definition to add to all morphs in this group</summary>
		[CanBeNull] public AspectDef aspectDef;

		/// <summary> An enumerable collection of all morphs in this group.</summary>
		[NotNull]
		public IReadOnlyList<MorphDef> MorphsInGroup => _associatedMorphs ?? (_associatedMorphs = DefDatabase<MorphDef>.AllDefs.Where(def => def.@group == this).ToList());
	}
}