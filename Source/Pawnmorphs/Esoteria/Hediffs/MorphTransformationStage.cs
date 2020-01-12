// MorphTransformationStage.cs modified by Iron Wolf for Pawnmorph on 01/12/2020 2:00 PM
// last updated 01/12/2020  2:00 PM

using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib.Utils;
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
        /// The morph def to get mutations from
        /// this cannot be null, and must be set in the xml 
        /// </summary>
        [UsedImplicitly]
        public MorphDef morph;

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
        /// Gets all mutation entries in this stage 
        /// </summary>
        /// <value>
        /// The entries.
        /// </value>
        public override IEnumerable<MutationEntry> Entries {
            get
            {
                if (_entries == null) //use lazy initialization 
                {
                    if (morph == null)
                    {
                        throw new ArgumentNullException(nameof(morph)); 
                    }

                    _entries = new List<MutationEntry>(); 
                    foreach (MutationDef mutation in morph.AllAssociatedMutations)
                    {
                        _entries.Add(new MutationEntry
                        {
                            mutation = mutation
                        });
                    }

                    string outStr = _entries.Select(m => m.mutation.defName).Join(",");
                    Log.Message("for " + morph.defName + ": " + outStr);
                }
                
                return _entries; 


            } }
    }
}