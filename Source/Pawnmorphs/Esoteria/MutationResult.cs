// MutationResult.cs created by Iron Wolf for Pawnmorph on 03/04/2020 5:32 PM
// last updated 03/04/2020  5:32 PM

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// struct that holds information about the result of a call to MutationUtility.AddMutation
	/// </summary>
	/// 
	public readonly struct MutationResult : IReadOnlyList<Hediff_AddedMutation>
	{
		/// <summary>
		/// an 'empty' mutation result 
		/// </summary>
		/// <value>
		/// The empty.
		/// </value>
		public static MutationResult Empty => new MutationResult();

		private readonly IReadOnlyList<Hediff_AddedMutation> _addedMutations;

		/// <summary>
		/// Gets the parts.
		/// </summary>
		/// <value>
		/// The parts.
		/// </value>
		[NotNull]
		public IEnumerable<BodyPartRecord> Parts
		{
			get
			{
				if (_addedMutations == null) yield break;
				foreach (Hediff_AddedMutation mutation in _addedMutations)
				{
					if (mutation.Part == null) continue;
					yield return mutation.Part;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MutationResult"/> struct.
		/// </summary>
		/// <param name="mutations">The mutations.</param>
		public MutationResult(IEnumerable<Hediff_AddedMutation> mutations)
		{
			_addedMutations = mutations.MakeSafe().ToList();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MutationResult"/> struct.
		/// </summary>
		/// <param name="mutation">The mutation.</param>
		/// <exception cref="System.ArgumentNullException">mutation</exception>
		public MutationResult([NotNull] Hediff_AddedMutation mutation)
		{
			if (mutation == null) throw new ArgumentNullException(nameof(mutation));
			_addedMutations = new List<Hediff_AddedMutation>()
				{ mutation};
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MutationResult"/> struct.
		/// </summary>
		/// <param name="mutations">The mutations.</param>
		public MutationResult(params Hediff_AddedMutation[] mutations)
		{
			_addedMutations = mutations;
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="MutationResult"/> to <see cref="System.Boolean"/>.
		/// </summary>
		/// <param name="result">The result.</param>
		/// <returns>
		/// true if any mutations were added, false otherwise 
		/// </returns>
		public static implicit operator bool(MutationResult result)
		{
			return result.Count > 0;
		}


		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<Hediff_AddedMutation> GetEnumerator()
		{
			if (_addedMutations == null) yield break;
			foreach (Hediff_AddedMutation mutation in _addedMutations)
			{
				yield return mutation;
			}
		}

		/// <summary>Returns an enumerator that iterates through a collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>Gets the number of elements in the collection.</summary>
		/// <returns>The number of elements in the collection. </returns>
		public int Count => _addedMutations?.Count ?? 0;

		/// <summary>Gets the element at the specified index in the read-only list.</summary>
		/// <param name="index">The zero-based index of the element to get. </param>
		/// <returns>The element at the specified index in the read-only list.</returns>
		public Hediff_AddedMutation this[int index]
		{
			get
			{
				if (_addedMutations == null)
				{
					throw new ArgumentOutOfRangeException(nameof(index));
				}

				return _addedMutations[index];
			}
		}
	}
}