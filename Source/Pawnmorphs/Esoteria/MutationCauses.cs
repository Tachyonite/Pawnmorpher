﻿// MutationCauses.cs created by Iron Wolf for Pawnmorph on 09/04/2021 7:24 AM
// last updated 09/04/2021  7:24 AM

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
using Verse.Grammar;

namespace Pawnmorph
{
    /// <summary>
    ///     class representing a composite of causes for mutations. meant to construct entries with rule packs
    /// </summary>
    public partial class MutationCauses : IExposable, IEnumerable<MutationCauses.CauseEntry>
    {
        /// <summary>
        ///     The weapon prefix
        /// </summary>
        public const string WEAPON_PREFIX = "weapon";

        /// <summary>
        ///     The hediff prefix
        /// </summary>
        public const string HEDIFF_PREFIX = "hediff";

        /// <summary>
        ///     The mutagen cause prefix
        /// </summary>
        public const string MUTAGEN_PREFIX = "mutagen_cause";

        /// <summary>
        /// The precept prefix
        /// </summary>
        public const string PRECEPT_PREFIX = "precept"; 





        [NotNull] private List<CauseEntry> _entries;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MutationCauses" /> class.
        /// </summary>
        public MutationCauses()
        {
            _entries = new List<CauseEntry>();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _entries).GetEnumerator();
        }


        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<CauseEntry> GetEnumerator()
        {
            return _entries.GetEnumerator();
        }

        /// <summary>
        ///     Exposes the data.
        /// </summary>
        public void ExposeData()
        {
            Scribe_Collections.Look(ref _entries, "entries", LookMode.Deep);
        }

        /// <summary>
        /// Determines whether the def is one of the causes stored.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <returns>
        ///   <c>true</c> if the def is one of the causes stored; otherwise, <c>false</c>.
        /// </returns>
        public bool HasDefCause(Def def)
        {
            foreach (CauseEntry causeEntry in _entries)
            {
                if (causeEntry.Def == def) return true; 
            }

            return false; 
        }

        /// <summary>
        /// Determines whether the precept is one of the causes stored.
        /// </summary>
        /// <param name="precept">The precept.</param>
        /// <returns>
        ///   <c>true</c> if the precept is one of the causes stored; otherwise, <c>false</c>.
        /// </returns>
        public bool HasPreceptCause(Precept precept)
        {
            foreach (PreceptEntry preceptEntry in _entries.OfType<PreceptEntry>())
            {
                if (preceptEntry.precept == precept) return true; 
            }

            return false; 
        }

        /// <summary>
        ///     Adds the specified cause with the specified prefix.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="causeDef">The cause definition.</param>
        public void Add<T>(string prefix, [NotNull] T causeDef) where T : Def, new()
        {
            _entries.Add(new SpecificDefCause<T> {prefix = prefix, causeDef = causeDef});
        }

        /// <summary>
        /// Adds the specified precept cause 
        /// </summary>
        /// <param name="precept">The precept.</param>
        /// <param name="prefix">The prefix.</param>
        /// <exception cref="System.ArgumentNullException">precept</exception>
        public void Add([NotNull] Precept precept, string prefix = PRECEPT_PREFIX)
        {
            if (precept == null) throw new ArgumentNullException(nameof(precept));
            _entries.Add(new PreceptEntry(){prefix = prefix, precept = precept});
        }

        /// <summary>
        ///     Adds the specified cause.
        /// </summary>
        /// <param name="cause">The cause.</param>
        /// <exception cref="ArgumentNullException">cause</exception>
        public void Add([NotNull] CauseEntry cause)
        {
            if (cause == null) throw new ArgumentNullException(nameof(cause));
            _entries.Add(cause);
        }

        /// <summary>
        ///     Adds the specified causes.
        /// </summary>
        /// <param name="causes">The causes.</param>
        public void Add([NotNull] IEnumerable<CauseEntry> causes)
        {
            foreach (CauseEntry entry in causes) _entries.Add(entry);
        }

        /// <summary>
        ///     Generates the rules for this collection of causes
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <returns></returns>
        [NotNull]
        public IEnumerable<Rule> GenerateRules(string prefix = "")
        {
            //could these be cached? 
            return _entries.MakeSafe().SelectMany(e => e?.GenerateRules(prefix) ?? Enumerable.Empty<Rule>());
        }

        /// <summary>
        ///     Gets all causes of the specified def type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [NotNull]
        public IEnumerable<SpecificDefCause<T>> GetAllCauses<T>() where T : Def, new()
        {
            return _entries.OfType<SpecificDefCause<T>>();
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return "[" + string.Join(",", _entries.Select(e => e.ToString())) + "]";
        }
    }
}