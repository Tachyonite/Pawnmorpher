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
    public class AnimalClassDef : Def, IAnimalClass
    {
        /// <summary>
        ///     The parent classification
        /// </summary>
        public AnimalClassDef parent;

        [Unsaved] private List<IAnimalClass> _subClasses;

        [Unsaved] private List<AnimalClassDef> _subDefs;

        [Unsaved] private List<MorphDef> _morphs;

        [Unsaved] private List<MutationDef> _mutations;

        IEnumerable<IAnimalClass> IAnimalClass.Children => _subClasses.MakeSafe();


        bool IAnimalClass.Contains(IAnimalClass aClass)
        {
            if (aClass == this) return true;
            return
                _subClasses.Any(c => c == aClass || c.Contains(aClass)); //check if any of the children are a type or contain it 
        }

        AnimalClassDef IAnimalClass.ParentClass => parent;

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
            var c = (IAnimalClass) this;
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
            var c = (IAnimalClass) this;
            return c.Contains(animalClass);
        }


        internal void FindChildren()
        {
            //find the children of this classification 
            _subClasses = new List<IAnimalClass>();
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
                if (mutationDef.classInfluence == this)
                {
                    _mutations.Add(mutationDef); 
                }
            }
        }
    }


    internal interface
        IAnimalClass //implementation detail to make morphs a kind of animal class,  since species is a classification 
    {
        AnimalClassDef ParentClass { get; }

        [NotNull] IEnumerable<IAnimalClass> Children { get; }

        bool Contains([NotNull] IAnimalClass aClass);
    }
}