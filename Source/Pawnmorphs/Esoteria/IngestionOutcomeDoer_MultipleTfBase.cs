// IngestionOutcomeDoer_MultipleTfBase.cs modified by Iron Wolf for Pawnmorph on //2019 
// last updated 08/25/2019  12:03 PM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Hediffs;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// base class for all ingestion outcome doers that pick from more then one tf hediff to add 
    /// </summary>
    /// <seealso cref="RimWorld.IngestionOutcomeDoer" />
    public abstract class IngestionOutcomeDoer_MultipleTfBase : IngestionOutcomeDoer
    {
        public List<HediffDef> hediffDefs = new List<HediffDef>();
        public List<HediffDef> hediffDefsComplete = new List<HediffDef>();

        public class RuntimeGetSettings
        {
            public MorphTransformationTypes types;
            public bool isBlackList = true; //if the category list is a black list 
            public List<MorphCategoryDef> categories = new List<MorphCategoryDef>();
        }


        public RuntimeGetSettings runtime;

        [Unsaved] private List<HediffDef> _allCompleteDefs;

        [Unsaved] private List<HediffDef> _allPartialDefs;

        private bool IsValidTfDef(HediffDef def)
        {
            MorphTransformationTypes type = def.GetTransformationType();
            if ((type & runtime.types) == 0) return false;

            var hasAnyCats = false;
            foreach (MorphDef morphDef in MorphUtilities.GetAssociatedMorph(def))
            {
                bool hasCat =
                    morphDef.categories.Any(c => runtime
                                                .categories.Contains(c)); //check if the morph has any of the listed categories 

                if (runtime.isBlackList && hasCat) return false;
                if (hasCat) hasAnyCats = true;
            }

            if (!runtime.isBlackList && !hasAnyCats)
                return false; //if it's a white list make sure at least one category is present  
            return true;
        }

        /// <summary>
        /// Gets all complete defs.
        /// </summary>
        /// <value>
        /// All complete defs.
        /// </value>
        public List<HediffDef> AllCompleteDefs
        {
            get
            {
                if (_allCompleteDefs == null) GetAllHediffs();

                return _allCompleteDefs;
            }
        }

        /// <summary>
        /// Gets all partial defs.
        /// </summary>
        /// <value>
        /// All partial defs.
        /// </value>
        public List<HediffDef> AllPartialDefs
        {
            get
            {
                if (_allPartialDefs == null) GetAllHediffs();

                return _allPartialDefs;
            }
        }

        private void GetAllHediffs()
        {
            if (runtime == null) //if that setting is null don't get any more at runtime 
            {
                _allCompleteDefs = hediffDefsComplete;
                _allPartialDefs = hediffDefs;
                return;
            }

            var completeSet = new HashSet<HediffDef>(hediffDefsComplete); //use hash sets so we don't have any duplicates 
            var partialSet = new HashSet<HediffDef>(hediffDefs);

            IEnumerable<HediffDef> allTfHediffs = MorphTransformationDefOf.AllMorphs;

            foreach (HediffDef morphTf in allTfHediffs)
                if (IsValidTfDef(morphTf))
                {
                    MorphTransformationTypes type = morphTf.GetTransformationType();
                    if ((type & MorphTransformationTypes.Full) != 0) completeSet.Add(morphTf);

                    if ((type & MorphTransformationTypes.Partial) != 0) partialSet.Add(morphTf);
                }

            _allCompleteDefs = completeSet.ToList(); //now convert them back to lists 
            _allPartialDefs = completeSet.ToList();
        }
    }
}