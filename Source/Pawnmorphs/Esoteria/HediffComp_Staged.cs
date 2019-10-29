using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// a stage for the production comp 
    /// </summary>
    public class HediffComp_Staged
    {
        /// <summary>The days to produce</summary>
        public float daysToProduce = 1;
        /// <summary>The amount to produce</summary>
        public int amount = 1;
        /// <summary>The resource to produce</summary>
        public string resource;
        /// <summary>The chance for a rare resource to be produced instead of the regular resource </summary>
        public float chance = 50;
        /// <summary>The rare resource</summary>
        public string rareResource;
        /// <summary>The thought to add when the resource is produced</summary>
        public ThoughtDef thought = null;

        /// <summary>Gets the resource.</summary>
        /// <value>The resource.</value>
        public ThingDef Resource
        {
            get { return ThingDef.Named(resource); }
        }

        /// <summary>Gets the rare resource.</summary>
        /// <value>The rare resource.</value>
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
