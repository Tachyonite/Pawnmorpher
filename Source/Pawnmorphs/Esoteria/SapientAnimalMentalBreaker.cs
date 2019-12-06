// SapientAnimalMentalBreaker.cs modified by Iron Wolf for Pawnmorph on 12/05/2019 7:39 PM
// last updated 12/05/2019  7:39 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// class for giving sapient animals mental breaks 
    /// </summary>
    [StaticConstructorOnStartup]
    public class SapientAnimalMentalBreaker : IExposable
    {
        static SapientAnimalMentalBreaker()
        {
            AllSapientAnimalMentalBreaks = DefDatabase<MentalBreakDef>
                                          .AllDefsListForReading
                                          .Where(d => d.GetModExtension<SapientAnimalRestriction>()?.isForbidden == false)
                                          .ToList(); //grab all mental breaks marked for sapient animals and store them for later 
        }


        /// <summary>
        /// Gets all sapient animal mental breaks.
        /// </summary>
        /// <value>
        /// All sapient animal mental breaks.
        /// </value>
        public static IEnumerable<MentalBreakDef> AllSapientAnimalMentalBreaks { get; } 

        [NotNull]
        private readonly Pawn _pawn;

        /// <summary>
        /// Initializes a new instance of the <see cref="SapientAnimalMentalBreaker"/> class.
        /// </summary>
        // ReSharper disable once NotNullMemberIsNotInitialized
        public SapientAnimalMentalBreaker()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SapientAnimalMentalBreaker"/> class.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        public SapientAnimalMentalBreaker([NotNull] Pawn pawn)
        {
            _pawn = pawn ?? throw new ArgumentNullException(nameof(pawn)); 
        }
        
        void IExposable.ExposeData()
        {
            
        }
    }
}