// AspectDef.cs modified by Iron Wolf for Pawnmorph on 09/22/2019 12:24 PM
// last updated 09/22/2019  12:24 PM

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Pawnmorph
{    

    /// <summary>
    /// def for all affinities 
    /// </summary>
    public class AspectDef : Def
    {
        public Type aspectType;
        public List<AspectStage> stages = new List<AspectStage>();
        public Color labelColor = Color.white; 

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            aspectType = aspectType ?? typeof(Aspect);
            if (!typeof(Aspect).IsAssignableFrom(aspectType))
            {
                Log.Error($"in {defName}: affinityType {aspectType.Name} can not be converted to type {nameof(Aspect)}");
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors())
            {
                yield return configError;
            }

            if (stages == null) yield return "no stages"; 
        }

        /// <summary>
        /// get the affinity def with the given defName
        /// </summary>
        /// <param name="defName"></param>
        /// <returns></returns>
        public static AspectDef Named(string defName)
        {
            return DefDatabase<AspectDef>.GetNamed(defName); 
        }

        public Aspect CreateInstance()
        {
            var affinity = (Aspect) Activator.CreateInstance(aspectType);
            affinity.def = this;
            return affinity; 
        }
    }
}