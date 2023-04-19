// IngestionOutcomeDoer_MultipleTfBase.cs modified by Iron Wolf for Pawnmorph on //2019 
// last updated 08/25/2019  12:03 PM

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary> Base class for all ingestion outcome doers that pick from more then one tf hediff to add. </summary>
	/// <seealso cref="RimWorld.IngestionOutcomeDoer" />
	public abstract class IngestionOutcomeDoer_MultipleTfBase : IngestionOutcomeDoer
	{
		/// <summary>the partial hediffs to add</summary>
		public List<HediffDef> hediffDefs = new List<HediffDef>();
		/// <summary>
		/// the complete hediffs to add
		/// </summary>
		public List<HediffDef> hediffDefsComplete = new List<HediffDef>();
		/// <summary>setting for getting hediffDefs at runtime </summary>
		public RuntimeGetSettings runtime;
		[Unsaved] private List<HediffDef> _allCompleteDefs;
		[Unsaved] private List<HediffDef> _allPartialDefs;

		/// <summary> Gets all complete defs. </summary>
		/// <value> All complete defs. </value>
		public List<HediffDef> AllCompleteDefs
		{
			get
			{
				if (_allCompleteDefs == null) GetAllHediffs();

				return _allCompleteDefs;
			}
		}

		/// <summary> Gets all partial defs. </summary>
		/// <value> All partial defs. </value>
		public List<HediffDef> AllPartialDefs
		{
			get
			{
				if (_allPartialDefs == null) GetAllHediffs();

				return _allPartialDefs;
			}
		}

		private bool IsValidTfDef(HediffDef def)
		{
			MorphTransformationTypes type = def.GetTransformationType();
			if ((type & runtime.types) == 0) return false;

			var hasAnyCats = false;
			foreach (MorphDef morphDef in MorphUtilities.GetAssociatedMorph(def))
			{
				bool hasCat =
					morphDef.categories.Any(c => runtime.categories.Contains(c)); // Check if the morph has any of the listed categories.

				if (runtime.isBlackList && hasCat) return false;
				if (hasCat) hasAnyCats = true;
			}

			if (!runtime.isBlackList && !hasAnyCats)
				return false; //if it's a white list make sure at least one category is present  
			return true;
		}

		private void GetAllHediffs()
		{
			if (runtime == null) // If that setting is null don't get any more at runtime.
			{
				_allCompleteDefs = hediffDefsComplete;
				_allPartialDefs = hediffDefs;
				return;
			}

			var completeSet = new HashSet<HediffDef>(hediffDefsComplete); // Use hash sets so we don't have any duplicates.
			var partialSet = new HashSet<HediffDef>(hediffDefs);

			IEnumerable<HediffDef> allTfHediffs = TransformerUtility.AllMorphTfs;

			foreach (HediffDef morphTf in allTfHediffs)
				if (IsValidTfDef(morphTf))
				{
					MorphTransformationTypes type = morphTf.GetTransformationType();
					if ((type & MorphTransformationTypes.Full) != 0) completeSet.Add(morphTf);

					if ((type & MorphTransformationTypes.Partial) != 0) partialSet.Add(morphTf);
				}

			_allCompleteDefs = completeSet.ToList(); // Now convert them back to lists.
			_allPartialDefs = completeSet.ToList();
		}

		/// <summary>
		/// class representing the settings for getting hediff defs at runtime 
		/// </summary>
		public class RuntimeGetSettings
		{
			/// <summary>The types to get</summary>
			public MorphTransformationTypes types;
			/// <summary>if true, the categories will exclude, not include things</summary>
			public bool isBlackList = true; // If the category list is a black list            
			/// <summary>The categories to get hediffDefs from </summary>
			public List<MorphCategoryDef> categories = new List<MorphCategoryDef>();
		}
	}
}
