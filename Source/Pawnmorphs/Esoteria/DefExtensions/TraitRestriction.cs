// TraitRestriction.cs modified by Iron Wolf for Pawnmorph on 12/11/2019 8:44 PM
// last updated 12/11/2019  8:44 PM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	/// restriction extension for only allowing pawns with specific traits to use the attached def 
	/// </summary>
	/// <seealso cref="Pawnmorph.DefExtensions.RestrictionExtension" />
	public class TraitRestriction : RestrictionExtension
	{
		/// <summary>
		/// simple class for combining traitDef and degree info 
		/// </summary>
		public class Entry
		{
			/// <summary>
			/// The trait
			/// </summary>
			[NotNull]
			public TraitDef trait;
			/// <summary>
			/// The trait degree
			/// if less then 0 any degree will do 
			/// </summary>
			public int traitDegree = -1;
		}




		/// <summary>
		/// The entries
		/// </summary>
		[NotNull]
		public List<Entry> entries = new List<Entry>();


		/// <summary>
		/// checks if the given pawn passes the restriction.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		/// if the def can be used with the given pawn
		/// </returns>
		/// <exception cref="ArgumentNullException">pawn</exception>
		protected override bool PassesRestrictionImpl(Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			TraitSet storyTraits = pawn.story?.traits;
			if (storyTraits == null) return false;

			foreach (Entry entry in entries) //check all entries 
			{
				var tInfo = storyTraits.GetTrait(entry.trait);
				if (tInfo == null) continue;
				if (entry.traitDegree < 0) return true; //if the entry has a traitDegree less then zero, this means any degree will pass 
				if (entry.traitDegree == tInfo.Degree) return true;  //otherwise they must match 
			}

			return false;

		}
	}
}