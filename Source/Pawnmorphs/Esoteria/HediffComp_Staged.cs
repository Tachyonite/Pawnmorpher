using System.Collections.Generic;
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

		/// <summary>
		/// The hediff givers on this stage, if any
		/// </summary>
		public List<HediffGiver> hediffGivers;

		/// <summary>
		/// The minimum production boost needed to trigger this stage. Provided by Production Aspect.
		/// </summary>
		public float minProductionBoost;

		/// <summary>
		/// The minimum mutation severity needed to trigger this stage if any.
		/// </summary>
		public float? minSeverity;

		/// <summary>
		/// An additional factor for hunger rate
		/// </summary>
		public float hungerRateFactor = 1f;

		/// <summary>
		/// all stat offsets that will be active during this stage 
		/// </summary>
		[CanBeNull] public List<StatModifier> statOffsets;

		/// <summary>Gets the resource.</summary>
		/// <value>The resource.</value>
		public ThingDef Resource
		{
			get
			{
				if (string.IsNullOrEmpty(resource)) return null;
				return ThingDef.Named(resource);
			}
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
