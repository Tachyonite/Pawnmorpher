using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class HediffCompProperties_Production : HediffCompProperties
    {
        public float daysToProduce = 1f;
        public int amount = 1;
        public int chance = 0;
        public ThoughtDef thought = null;
        public ThoughtDef wrongGenderThought = null;
        public ThoughtDef etherBondThought = null;
        public ThoughtDef etherBrokenThought = null;
        public Gender genderAversion = Gender.None;
        public string resource;
        public string rareResource;
        public List<HediffComp_Staged> stages = null;

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (var configError in base.ConfigErrors(parentDef))
            {
                yield return configError; 
            }

            if (!string.IsNullOrEmpty(resource) && ThingDef.Named(resource) == null) yield return $"no resource called {resource}";
        }

        [CanBeNull]
        public ThingDef Resource
        {
            get
            {
                if (string.IsNullOrEmpty(resource)) return null; 
                return ThingDef.Named(resource);
            }
        }

        [CanBeNull]
        public ThingDef RareResource
        {
            get
            {
                if (string.IsNullOrEmpty(rareResource)) return null;

                return ThingDef.Named(rareResource); 
            }
        }



        public HediffCompProperties_Production()
        {
            compClass = typeof(HediffComp_Production);
        }
    }
}
