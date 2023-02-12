// MorphTransformationStage.cs modified by Iron Wolf for Pawnmorph on 01/12/2020 2:00 PM
// last updated 01/12/2020  2:00 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// transformation stage that gets all it's mutations from a morph at runtime 
	/// </summary>
	/// <seealso cref="Pawnmorph.Hediffs.TransformationStageBase" />
	public class MorphTransformationStage : TransformationStageBase
	{
		/// <summary>
		/// The morph or animal class def to get mutations from
		/// this cannot be null, and must be set in the xml 
		/// </summary>
		[UsedImplicitly]
		public AnimalClassBase morph;

		/// <summary>
		/// optional black list 
		/// </summary>
		[NotNull]
		public List<MutationDef> blackList = new List<MutationDef>();

		/// <summary>
		/// an override to use for the chance to add mutations from the given morph 
		/// </summary>
		public float? addChance;


		[Unsaved] private List<MutationEntry> _entries;

		/// <summary>
		/// returns all configuration errors in this stage
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors()
		{
			if (morph == null) yield return "morph def not set!";
		}


		/// <summary>
		/// Gets the entries for the given pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="source"></param>
		/// <returns></returns>
		public override IEnumerable<MutationEntry> GetEntries(Pawn pawn, Hediff source)
		{
			return Entries;
		}

		/// <summary>
		/// Gets all mutation entries in this stage 
		/// </summary>
		/// <value>
		/// The entries.
		/// </value>
		public IEnumerable<MutationEntry> Entries
		{
			get
			{
				if (_entries == null) //use lazy initialization 
				{
					if (morph == null)
					{
						throw new ArgumentNullException(nameof(morph));
					}

					_entries = new List<MutationEntry>();
					foreach (MutationDef mutation in morph.GetAllMorphsInClass().SelectMany(m => m.AllAssociatedMutations))
					{
						if (blackList.Contains(mutation)) continue;
						_entries.Add(new MutationEntry
						{
							mutation = mutation,
							addChance = addChance ?? mutation.defaultAddChance,
							blocks = mutation.defaultBlocks
						});
					}

				}

				return _entries;


			}
		}
	}
}