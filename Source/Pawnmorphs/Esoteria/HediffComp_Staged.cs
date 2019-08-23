using JetBrains.Annotations;
using RimWorld;
using Verse;


namespace Pawnmorph
{
    public class HediffComp_Staged
    {
        public float daysToProduce = 1;
        public int amount = 1;
        public string resource;
        public float chance = 50;
        public string rareResource;
        public ThoughtDef thought = null;


        public ThingDef Resource
        {
            get { return ThingDef.Named(resource); }
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


    }
}
