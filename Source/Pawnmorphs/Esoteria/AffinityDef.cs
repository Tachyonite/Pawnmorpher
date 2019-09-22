// AffinityDef.cs modified by Iron Wolf for Pawnmorph on 09/22/2019 12:24 PM
// last updated 09/22/2019  12:24 PM

using System;
using System.Collections.Generic;
using Verse;

namespace Pawnmorph
{    

    /// <summary>
    /// def for all affinities 
    /// </summary>
    public class AffinityDef : Def
    {
        public Type affinityType;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            affinityType = affinityType ?? typeof(Affinity);
            if (!typeof(Affinity).IsAssignableFrom(affinityType))
            {
                Log.Error($"in {defName}: affinityType {affinityType.Name} can not be converted to type {nameof(Affinity)}");
            }
        }

        /// <summary>
        /// get the affinity def with the given defName
        /// </summary>
        /// <param name="defName"></param>
        /// <returns></returns>
        public static AffinityDef Named(string defName)
        {
            return DefDatabase<AffinityDef>.GetNamed(defName); 
        }

        public Affinity CreateInstance()
        {
            var affinity = (Affinity) Activator.CreateInstance(affinityType);
            affinity.def = this;
            return affinity; 
        }
        
    }
}