﻿// PMThoughtUtilities.cs modified by Iron Wolf for Pawnmorph on 12/02/2019 9:48 AM
// last updated 12/02/2019  9:48 AM

using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
using Thought_Memory = RimWorld.Thought_Memory;

namespace Pawnmorph
{

    /// <summary>
    /// static container for thought related utilities 
    /// </summary>
    public static class PMThoughtUtilities
    {
        /// <summary>
        /// get the substitute thought for the given pawn 
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="pawn">The pawn.</param>
        /// <returns>the substitute thought if one exists, if not the original thought</returns>
        [NotNull]
        public static ThoughtDef GetSubstitute([NotNull] this ThoughtDef def, [NotNull] Pawn pawn)
        {
            var tGroups = def.modExtensions.MakeSafe().OfType<ThoughtGroupDefExtension>();

            foreach (ThoughtDef thoughtDef in tGroups.SelectMany(g => g.thoughts))
            {
                if (ThoughtUtility.CanGetThought(pawn, thoughtDef)) //take the first one that matches 
                {
                    return thoughtDef; 
                    
                }
            }
            //no matches found 
            return def; 
        }

        /// <summary>
        /// Gets the substitute memory to be used with the given pawn
        /// </summary>
        /// <param name="memory">The memory.</param>
        /// <param name="pawn">The pawn.</param>
        /// <returns>the substitute memory to be used with the given pawn, if no substitute exists it just returns the original pawn</returns>
        [NotNull]
        public static Thought_Memory GetSubstitute([NotNull] this Thought_Memory memory, [NotNull] Pawn pawn)
        {
            var tGroups = memory.def.modExtensions.MakeSafe().OfType<ThoughtGroupDefExtension>();

            foreach (ThoughtDef thoughtDef in tGroups.SelectMany(g => g.thoughts))
            {
                if (ThoughtUtility.CanGetThought(pawn, thoughtDef))
                {
                    var newMemory = ThoughtMaker.MakeThought(thoughtDef) as Thought_Memory;
                    if (newMemory == null)
                    {
                        Log.Error($"in thought {memory.def.defName} group, thought {thoughtDef.defName} is not a memory");
                        continue;
                    }

                    return newMemory; 
                }
            }

            return memory; 
        }
    }
}