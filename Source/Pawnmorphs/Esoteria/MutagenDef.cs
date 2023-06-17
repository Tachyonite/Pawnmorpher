// MutagenDef.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/13/2019 4:02 PM
// last updated 08/13/2019  4:02 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.TfSys;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
using Verse.Grammar;
using static Pawnmorph.MutationUtilities;

namespace Pawnmorph
{
	/// <summary>
	///     Def for a mutagen strain. <br />
	///     A mutagen is a collection of transformation related hediff's ingestionOutcomeDoers that all share a common IFF
	///     system.
	/// </summary>
	/// <seealso cref="Verse.Def" />
	public class MutagenDef : Def, ICauseRulePackContainer
	{

		/// <summary>
		/// used by the mutation log to add content when this mutagen causes a mutation 
		/// </summary>
		[CanBeNull]
		public RulePack causeRulePack;

		/// <summary>
		///     if this mutagen def applies transformation paralysis
		/// </summary>
		public bool appliesTfParalysis = true;

		/// <summary>if this instance can infect animals</summary>
		public bool canInfectAnimals;

		/// <summary>
		///     if this instance can infect mechanoids
		/// </summary>
		public bool canInfectMechanoids;

		/// <summary>The mutagen type</summary>
		public Type mutagenType;

		/// <summary>
		///     the positive thought to add when a pawn is reverted
		/// </summary>
		public ThoughtDef revertedThoughtGood;

		/// <summary>
		///     The negative thought to add when a pawn is reverted
		/// </summary>
		public ThoughtDef revertedThoughtBad;

		/// <summary>
		///     the average sapience drop when a pawn is transformed by this mutagen
		/// </summary>
		/// note, values returned by this range will be clamped to [0,1]
		public float transformedSapienceDropMean = 0.5f;

		/// <summary>
		///     the standard deviation of the sapience drop when a pawn is transformed by this mutagen
		/// </summary>
		/// note, values returned by this range will be clamped to [0,1]
		public float transformedSapienceDropStd = 0.05f;

		/// <summary>
		///     the reversion thought for pawns with primal wish
		/// </summary>
		public ThoughtDef revertedPrimalWish;

		/// <summary>
		///     sapience state for when a pawn is transformed by this mutagen
		/// </summary>
		/// default is FormerHuman
		public SapienceStateDef transformedSapienceState;

		/// <summary>
		///     sapience state for when a pawn is reverted from being transformed by this mutagen
		/// </summary>
		public SapienceStateDef revertedSapienceState;

		/// <summary>
		///     The damage properties
		/// </summary>
		[NotNull] public MutagenDamageProperties damageProperties = new MutagenDamageProperties();


		/// <summary>
		///     a list of hediffs that make a pawn immune to the effects of this mutagen source
		/// </summary>
		[NotNull] public List<HediffDef> immunizingHediffs = new List<HediffDef>();

		/// <summary>
		///     The aspect givers
		/// </summary>
		[NotNull] public List<AspectGiver> aspectGivers = new List<AspectGiver>();

		/// <summary>
		///     list of aspect givers that are tried when a pawn transforms
		/// </summary>
		[NotNull] public List<AspectGiver> tfAspectGivers = new List<AspectGiver>();

		[Unsaved] private Mutagen _mutagenCached;

		/// <summary>Gets the cached mutagen </summary>
		/// <value>The cached mutagen </value>
		[NotNull]
		public Mutagen MutagenCached
		{
			get
			{
				Mutagen cached = _mutagenCached;
				if (cached != null) return cached;

				_mutagenCached = (Mutagen)Activator.CreateInstance(mutagenType);
				_mutagenCached.def = this;
				return _mutagenCached;
			}
		}

		//TODO figure out a better way to handle all the variation without a bunch of overloads 


		/// <summary>
		/// Adds the mutation and aspects to the given pawn using the aspects attached to this mutagen
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="mutation">The mutation.</param>
		/// <param name="countToAdd">The count to add.</param>
		/// <param name="cause">The cause.</param>
		/// <param name="ancillaryEffects">The ancillary effects.</param>
		/// <param name="force">if set to <c>true</c> [force].</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">pawn
		/// or
		/// mutation</exception>
		public MutationResult AddMutationAndAspects([NotNull] Pawn pawn, [NotNull] MutationDef mutation,
													int countToAdd = int.MaxValue, Hediff cause = null,
													AncillaryMutationEffects? ancillaryEffects = null, bool force = false)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			if (mutation == null) throw new ArgumentNullException(nameof(mutation));
			MutationResult res = AddMutation(pawn, mutation, countToAdd, ancillaryEffects, force);
			HandlePostMutationEffects(pawn, res, ancillaryEffects, cause);
			return res;
		}

		/// <summary>
		/// Adds the mutation and aspects to the given pawn using the aspects attached to this mutagen
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="mutation">The mutation.</param>
		/// <param name="parts">The parts.</param>
		/// <param name="cause">The cause.</param>
		/// <param name="countToAdd">The count to add.</param>
		/// <param name="ancillaryEffects">The ancillary effects.</param>
		/// <param name="force">if set to <c>true</c> [force].</param>
		/// <returns></returns>
		public MutationResult AddMutationAndAspects([NotNull] Pawn pawn, [NotNull] MutationDef mutation,
													[CanBeNull] List<BodyPartDef> parts, [CanBeNull] Hediff cause = null,
													int countToAdd = int.MaxValue,
													AncillaryMutationEffects? ancillaryEffects = null, bool force = false)
		{
			MutationResult res = AddMutation(pawn, mutation, parts, countToAdd, ancillaryEffects, force);
			HandlePostMutationEffects(pawn, res, ancillaryEffects, cause);
			return res;
		}


		/// <summary>
		/// Adds the mutation and aspects to the given pawn using the aspects attached to this mutagen
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="mutation">The mutation.</param>
		/// <param name="bodyPart">The body part.</param>
		/// <param name="cause">The cause.</param>
		/// <param name="ancillaryEffects">The ancillary effects.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">pawn
		/// or
		/// mutation
		/// or
		/// bodyPart</exception>
		public MutationResult AddMutationAndAspects([NotNull] Pawn pawn, [NotNull] MutationDef mutation,
													[NotNull] BodyPartRecord bodyPart, [CanBeNull] Hediff cause = null,
													AncillaryMutationEffects? ancillaryEffects = null)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			if (mutation == null) throw new ArgumentNullException(nameof(mutation));
			if (bodyPart == null) throw new ArgumentNullException(nameof(bodyPart));
			MutationResult res = AddMutation(pawn, mutation, bodyPart, ancillaryEffects);
			HandlePostMutationEffects(pawn, res, ancillaryEffects, cause);
			return res;
		}

		/// <summary>
		///     Determines whether this instance can infect the specified pawn.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///     <c>true</c> if this instance can infect the specified pawn; otherwise, <c>false</c>.
		/// </returns>
		public bool CanInfect(Pawn pawn)
		{
			return MutagenCached.CanInfect(pawn);
		}


		/// <summary>
		///     Determines whether this instance can infect the specified race.
		/// </summary>
		/// <param name="race">The definition.</param>
		/// <returns>
		///     <c>true</c> if this instance can infect the specified race; otherwise, <c>false</c>.
		/// </returns>
		public bool CanInfect(ThingDef race)
		{
			return MutagenCached.CanInfect(race);
		}

		/// <summary> Determines whether this instance can transform the specified pawn. </summary>
		/// <param name="pawn"> The pawn. </param>
		/// <returns> <c>true</c> if this instance can transform the specified pawn; otherwise, <c>false</c>. </returns>
		public bool CanTransform(Pawn pawn)
		{
			return MutagenCached.CanTransform(pawn);
		}

		/// <summary>
		///     Determines whether this instance can transform the specified race definition.
		/// </summary>
		/// <param name="raceDef">The race definition.</param>
		/// <returns>
		///     <c>true</c> if this instance can transform the specified race definition; otherwise, <c>false</c>.
		/// </returns>
		public bool CanTransform(ThingDef raceDef)
		{
			return MutagenCached.CanTransform(raceDef);
		}

		/// <summary>Get all Configuration Errors with this instance</summary>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string configError in base.ConfigErrors()) yield return configError;

			foreach (AspectGiver aspectGiver in aspectGivers.MakeSafe()) //check the aspect givers for errors 
				if (aspectGiver == null) yield return "null aspect giver";
				else
					foreach (string configError in aspectGiver.ConfigErrors())
						yield return configError;

			if (mutagenType == null)
				yield return "no mutagen type";
			else if (!typeof(Mutagen).IsAssignableFrom(mutagenType))
				yield return $"type {mutagenType.Name} is not a subtype of Mutagen";
		}

		private void HandlePostMutationEffects(Pawn pawn, in MutationResult res, AncillaryMutationEffects? aEffects, Hediff cause)
		{
			if (res)
			{
				this.TryApplyAspects(pawn);
			}
		}

		/// <summary>
		/// Gets the rules using the given prefix 
		/// </summary>
		/// <param name="prefix">The prefix.</param>
		/// <returns></returns>
		public IEnumerable<Rule> GetRules(string prefix)
		{
			if (causeRulePack == null) return Enumerable.Empty<Rule>();
			return causeRulePack.Rules.MakeSafe(); //ignoring prefix so the xml rules can be used as is 
		}
	}


	/// <summary>
	///     class that stores information about mutagenic damage
	/// </summary>
	public class MutagenDamageProperties
	{
		/// <summary>
		///     the minimum amount of damage to do to apparel
		/// </summary>
		public int apparelDamageOffset;

		/// <summary>
		///     The apparel damage multiplier
		/// </summary>
		public float apparelDamageMultiplier = 1;

		/// <summary>
		///     how much biproduct to spawn per point of damage
		/// </summary>
		public float spawnedBiproductMult;

		/// <summary>
		///     The biproduct to spawn when apparel takes damage
		/// </summary>
		[CanBeNull] public ThingDef biproduct;
	}
}