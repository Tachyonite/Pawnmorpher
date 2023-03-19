// AnimalClassDef.cs modified by Iron Wolf for Pawnmorph on 01/10/2020 5:11 PM
// last updated 01/10/2020  5:11 PM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	///     def for an 'animal classification', like canid, feline, etc.
	/// </summary>
	/// <seealso cref="Verse.Def" />
	public class AnimalClassDef : AnimalClassBase
	{
		/// <summary>
		///     The parent classification
		/// </summary>
		public AnimalClassDef parent;

		[Unsaved] private List<AnimalClassBase> _subClasses;

		[Unsaved] private List<AnimalClassDef> _subDefs;

		[Unsaved] private List<MorphDef> _morphs;

		[Unsaved] private List<MutationDef> _mutations;

		/// <summary>
		/// Gets the children.
		/// </summary>
		/// <value>
		/// The children.
		/// </value>
		public override IEnumerable<AnimalClassBase> Children => _subClasses.MakeSafe();


		/// <summary>
		/// Gets the label.
		/// </summary>
		/// <value>
		/// The label.
		/// </value>
		public override string Label => label;
		/// <summary>
		/// Determines whether this instance contains the object.
		/// </summary>
		/// <param name="aClass">a class.</param>
		/// <returns>
		///   <c>true</c> if contains the specified a class; otherwise, <c>false</c>.
		/// </returns>
		public override bool Contains(AnimalClassBase aClass)
		{
			if (aClass == this) return true;
			return
				_subClasses.Any(c => c == aClass || c.Contains(aClass)); //check if any of the children are a type or contain it 
		}

		/// <summary>
		/// Gets the parent class.
		/// </summary>
		/// <value>
		/// The parent class.
		/// </value>
		public override AnimalClassDef ParentClass => parent;

		/// <summary>
		///     Gets the sub classes of this classification
		/// </summary>
		/// <value>
		///     The sub classes.
		/// </value>
		[NotNull]
		public IEnumerable<AnimalClassDef> SubClasses => _subDefs.MakeSafe();

		/// <summary>
		///     Gets the morphs that are in this classification
		/// </summary>
		/// <value>
		///     The morphs.
		/// </value>
		[NotNull]
		public IEnumerable<MorphDef> Morphs => _morphs.MakeSafe();

		/// <summary>
		/// all mutations that directly give influence for this class
		/// </summary>
		/// this does not include mutations that give influence for any of this class's children 
		/// <value>
		/// The direct mutations.
		/// </value>
		public IEnumerable<MutationDef> DirectMutations => _mutations.MakeSafe();

		/// <summary>
		///     Determines whether this instance contains the morph.
		/// </summary>
		/// <param name="morph">The morph.</param>
		/// <returns>
		///     <c>true</c> if contains the specified morph; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">morph</exception>
		public bool Contains([NotNull] MorphDef morph)
		{
			if (morph == null) throw new ArgumentNullException(nameof(morph));
			var c = (AnimalClassBase)this;
			return c.Contains(morph);
		}

		/// <summary>
		///     Determines whether this instance contains the object.
		/// </summary>
		/// <param name="animalClass">The animal class.</param>
		/// <returns>
		///     <c>true</c> if contains the specified animal class; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">animalClass</exception>
		public bool Contains([NotNull] AnimalClassDef animalClass)
		{
			if (animalClass == null) throw new ArgumentNullException(nameof(animalClass));
			var c = (AnimalClassBase)this;
			return c.Contains(animalClass);
		}


		internal void FindChildren()
		{
			//find the children of this classification 
			_subClasses = new List<AnimalClassBase>();
			_subDefs = new List<AnimalClassDef>();
			_morphs = new List<MorphDef>();
			foreach (AnimalClassDef animalClassDef in DefDatabase<AnimalClassDef>.AllDefs)
			{
				if (animalClassDef == this) continue;

				if (animalClassDef.parent == this)
				{
					_subClasses.Add(animalClassDef);
					_subDefs.Add(animalClassDef);
				}
			}

			foreach (MorphDef morphDef in MorphDef.AllDefs)
				if (morphDef.classification == this)
				{
					_subClasses.Add(morphDef);
					_morphs.Add(morphDef);
				}

			_mutations = new List<MutationDef>();
			//no get mutation that give this influence 
			foreach (MutationDef mutationDef in DefDatabase<MutationDef>.AllDefsListForReading)
			{
				if (mutationDef.ClassInfluences.Contains(this))
				{
					_mutations.Add(mutationDef);
				}
			}
		}
	}


	/// <summary>
	/// interface for both MorphDefs and AnimalClassDef
	/// </summary>
	/// this should generally not be implemented outside of these 2 defs 
	public abstract class AnimalClassBase : Def
	{

		/// <summary>
		/// a list of mutations to specifically exclude from the heirarchy 
		/// </summary>
		protected List<MutationDef> mutationExclusionList;


		/// <summary>
		/// Gets the mutation exclusion list.
		/// </summary>
		/// <value>
		/// The mutation exclusion list.
		/// </value>
		[NotNull]
		public IReadOnlyList<MutationDef> MutationExclusionList
		{
			get
			{
				if (mutationExclusionList == null) return Array.Empty<MutationDef>();
				return mutationExclusionList;
			}
		}


		/// <summary>
		/// Gets the parent class.
		/// </summary>
		/// <value>
		/// The parent class.
		/// </value>
		public abstract AnimalClassDef ParentClass { get; }

		/// <summary>
		/// Gets the label.
		/// </summary>
		/// <value>
		/// The label.
		/// </value>
		public abstract string Label { get; }

		/// <summary>
		/// Gets the children.
		/// </summary>
		/// <value>
		/// The children.
		/// </value>
		[NotNull] public abstract IEnumerable<AnimalClassBase> Children { get; }

		/// <summary>
		/// Determines whether this instance contains the given class.
		/// </summary>
		/// <param name="aClass">a class.</param>
		/// <returns>
		///   <c>true</c> if contains the specified a class; otherwise, <c>false</c>.
		/// </returns>
		public abstract bool Contains([NotNull] AnimalClassBase aClass);
	}
}