using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// comp properties for the hediff comp production 
	/// </summary>
	public class HediffCompProperties_Production : HediffCompProperties
	{
		/// <summary>
		/// how many days it takes to produce 
		/// </summary>
		public float daysToProduce = 1f;
		/// <summary>
		/// the amount of resources to produce 
		/// </summary>
		public int amount = 1;
		/// <summary>
		/// chance to produce rare resources instead of regular resources 
		/// </summary>
		public int chance = 0;
		/// <summary>
		/// the default thought to add when producing resources
		/// </summary>
		[CanBeNull] public ThoughtDef thought = null;

		/// <summary>
		/// the thought to add when the pawn's gender matches the <see cref="genderAversion"/>
		/// </summary>
		[CanBeNull] public ThoughtDef wrongGenderThought = null;

		/// <summary>
		/// the thought to add when the pawn is ether bonded 
		/// </summary>
		[CanBeNull] public ThoughtDef etherBondThought = null;

		/// <summary>
		///     the thought to add when the pawn is ether broken 
		/// </summary>
		[CanBeNull] public ThoughtDef etherBrokenThought = null;

		/// <summary>
		/// if the pawns gender matches this, the pawn is considered 'gender adverse' to producing 
		/// </summary>
		public Gender genderAversion = Gender.None;
		/// <summary>
		/// the defName of the resource to produce
		/// </summary>
		public string resource;
		/// <summary>
		/// the defName of the rare resource to produce
		/// </summary>
		public string rareResource;
		/// <summary>
		/// the stages the comp can go through 
		/// </summary>
		[CanBeNull] public List<HediffComp_Staged> stages = null;

		/// <summary>
		/// the job giver to use to make the pawn produce, if null the products will just be spawned in 
		/// </summary>
		public Type jobGiver;

		/// <summary>
		/// get all configuration errors with this instance 
		/// </summary>
		/// <param name="parentDef"></param>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			foreach (var configError in base.ConfigErrors(parentDef))
			{
				yield return configError;
			}

			if (!string.IsNullOrEmpty(resource) && ThingDef.Named(resource) == null) yield return $"no resource called {resource}";
		}

		/// <summary>
		/// the resource to produce
		/// </summary>
		[CanBeNull]
		public ThingDef Resource
		{
			get
			{
				if (string.IsNullOrEmpty(resource)) return null;
				return ThingDef.Named(resource);
			}
		}

		/// <summary>
		/// the rare resource to produce 
		/// </summary>
		[CanBeNull]
		public ThingDef RareResource
		{
			get
			{
				if (string.IsNullOrEmpty(rareResource)) return null;

				return ThingDef.Named(rareResource);
			}
		}

		/// <summary>
		/// create a new instance of this type 
		/// </summary>
		public HediffCompProperties_Production()
		{
			compClass = typeof(HediffComp_Production);
		}
	}
}
