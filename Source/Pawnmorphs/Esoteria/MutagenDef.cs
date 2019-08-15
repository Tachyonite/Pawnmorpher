// MutagenDef.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/13/2019 4:02 PM
// last updated 08/13/2019  4:02 PM

using System;
using System.Collections.Generic;
using Pawnmorph.TfSys;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// def for a mutagen strain
    /// </summary>
    /// A mutagen is a collection of transformation related hediff's ingestionOutcomeDoers, ect. that all share
    /// a common IFF system 
    /// <seealso cref="Verse.Def" />
    public class MutagenDef : Def
    {
        public bool canInfectAnimals;
        public bool canInfectMechanoids;
        public Type mutagenType;

        public List<HediffDef> reversionThoughts = new List<HediffDef>();  

        public override IEnumerable<string> ConfigErrors()
        {


            foreach (var configError in base.ConfigErrors())
            {
                yield return configError; 
            }

            if (mutagenType == null)
                yield return "no mutagen type"; 
            else if (!typeof(Mutagen).IsAssignableFrom(mutagenType))
                yield return $"type {mutagenType.Name} is not a subtype of Mutagen"; 
        }

        [Unsaved] private Mutagen _mutagenCached;

        
        public Mutagen MutagenCached => _mutagenCached ?? (_mutagenCached = (Mutagen) Activator.CreateInstance(mutagenType));


        public bool CanInfect(Pawn pawn)
        {
            return MutagenCached.CanInfect(pawn); 
        }
    }
}