// MutationRuleDef.cs created by Iron Wolf for Pawnmorph on 03/06/2020 9:16 PM
// last updated 03/06/2020  9:16 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	///     def for all 'mutation' rules
	/// </summary>
	/// mutation rules are rules that are run on pawns every so often that will add mutations/hediffs if certain criteria are met
	/// <seealso cref="Verse.Def" />
	public class MutationRuleDef : Def
	{
		/// <summary>
		///     how often the rules are checked
		/// </summary>
		public const int CHECK_RATE = 1000;


		[Unsaved] private List<IMRuleEntry> _allRules;

		/// <summary>
		/// Gets all rules in this def 
		/// </summary>
		/// <value>
		/// All rules.
		/// </value>
		public IReadOnlyList<IMRuleEntry> AllRules
		{
			get
			{
				if (_allRules == null)
				{
					_allRules = new List<IMRuleEntry>();
					_allRules.AddRange(conditions.MakeSafe());
					_allRules.AddRange(aspectConditions.MakeSafe());
				}

				return _allRules;
			}
		}


		/// <summary>
		///     the mean time to happen (in days)
		/// </summary>
		/// the mean time it takes for a rule to trigger one it's conditions are met'
		public float mtth;

		/// <summary>
		///     the type of the rule worker
		/// </summary>
		public Type ruleWorker;


		/// <summary>
		///     The priority of this rule
		/// </summary>
		/// rules with a 'lower' priority value are run before those with a higher priority value
		public int priority = 0;

		/// <summary>
		///     The entries that are check against a pawn to see if the rule can be executed
		/// </summary>
		public List<HediffEntry> conditions = new List<HediffEntry>();

		/// <summary>
		/// list of aspect conditions
		/// </summary>
		public List<AspectEntry> aspectConditions = new List<AspectEntry>();

		/// <summary>
		///     The output entry when this rule is run
		/// </summary>
		public List<Result> results;

		/// <summary>
		/// Gets the instantiated mutation rule worker.
		/// </summary>
		/// <value>
		/// The worker.
		/// </value>
		[NotNull] public MutationRuleWorker Worker { get; private set; }

		/// <summary>
		///     gets all configuration errors on this object
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string configError in base.ConfigErrors().MakeSafe()) yield return configError;

			foreach (HediffEntry hediffEntry in conditions.MakeSafe())
				if (hediffEntry.hediffs == null || hediffEntry.hediffs.Count == 0)
					yield return "rule entry with null hediff";

			foreach (AspectEntry aspectEntry in aspectConditions.MakeSafe())
			{
				if (aspectEntry.aspectDef == null)
				{
					yield return "aspect rule entry with null aspect def";
				}
			}

			if ((results == null || results.Count == 0) && ruleWorker == null)
				yield return "no output effect set while using default rule worker";
		}

		/// <summary>
		///     Resolves the references.
		/// </summary>
		public override void ResolveReferences()
		{
			base.ResolveReferences();
			if (ruleWorker == null)
				Worker = new DefaultMutationRuleWorker(this);
			else
				try
				{
					var worker = (MutationRuleWorker)Activator.CreateInstance(ruleWorker, this);
					if (worker == null)
						Log.Error($"unable to create mutation rule worker of type \"{ruleWorker.Name}\"");
					else
						Worker = worker;
				} //just prettying up the error messages that will get produced if the rule worker is incorrect 
				catch (InvalidCastException e)
				{
					Log.Error($"unable to cast {ruleWorker.Name} to {nameof(MutationRuleWorker)}\n\n{e}");
				}
				catch (MissingMethodException e)
				{
					Log.Error($"could not find valid constructor on type \"{ruleWorker.Name}\". Does it derive from {nameof(MutationRuleWorker)}?\n\n{e}");
				}
		}

		/// <summary>
		///     Tries to execute the rule on the given pawn.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>if the rule was successfully executed on the pawn</returns>
		/// <exception cref="System.ArgumentNullException">pawn</exception>
		public virtual bool TryRule([NotNull] Pawn pawn)
		{
			return Worker.TryRule(pawn);
		}

		/// <summary>
		/// aspect rule entry
		/// </summary>
		/// <seealso cref="Pawnmorph.MutationRuleDef.IMRuleEntry" />
		public class AspectEntry : IMRuleEntry
		{
			/// <summary>
			///     The aspect definition to look for
			/// </summary>
			public AspectDef aspectDef;

			/// <summary>
			///     if set, then the aspect must be in this given stage
			/// </summary>
			public int? aspectStage;


			/// <summary>
			/// check if this entry is satisfied by the given pawn
			/// </summary>
			/// <param name="pawn">The pawn.</param>
			/// <returns></returns>
			/// <exception cref="ArgumentNullException">pawn</exception>
			public bool Satisfied([NotNull] Pawn pawn)
			{
				if (pawn == null) throw new ArgumentNullException(nameof(pawn));
				AspectTracker tracker = pawn.GetAspectTracker();
				if (tracker == null) return false;

				Aspect a = tracker.GetAspect(aspectDef);
				if (aspectStage == null) return a != null;
				return a?.StageIndex == aspectStage.Value;
			}
		}

		/// <summary>
		///     simple POD for rule results
		/// </summary>
		public class Result
		{
			/// <summary>
			///     The hediff definition
			/// </summary>
			public HediffDef hediffDef;


			/// <summary>
			///     The record the hediff must be on
			/// </summary>
			public BodyPartDef partDef;
		}


		/// <summary>
		/// </summary>
		public class HediffEntry : IMRuleEntry
		{
			/// <summary>
			///     list of hediffs to check for
			/// </summary>
			public List<HediffDef> hediffs = new List<HediffDef>();


			/// <summary>
			///     The record the hediff must be on
			/// </summary>
			public BodyPartDef partDef;

			/// <summary>
			///     if true, this entry is satisfied if the pawn has any the hediff on any part
			/// </summary>
			public bool anyPart;

			/// <summary>
			///     if set, the hediff must be on this stage
			/// </summary>
			public int? stageIndex;

			/// <summary>
			///     check if this entry is satisfied by the given pawn
			/// </summary>
			/// <param name="pawn">The pawn.</param>
			/// <returns></returns>
			public virtual bool Satisfied([NotNull] Pawn pawn)
			{
				hediffs = hediffs ?? new List<HediffDef>();
				Hediff hediff;
				hediff = anyPart
							 ? pawn.health.hediffSet.hediffs.FirstOrDefault(h => hediffs.Contains(h.def)
																			  && (stageIndex == null
																			   || stageIndex.Value == h.CurStageIndex))
							 : pawn.health.hediffSet.hediffs.FirstOrDefault(h => hediffs.Contains(h.def)
																			  && h.Part?.def == partDef);

				if (hediff == null) return false;
				if (stageIndex == null) return true;
				return stageIndex.Value == hediff.CurStageIndex;
			}
		}
		/// <summary>
		/// interface for all rule entries 
		/// </summary>
		public interface IMRuleEntry
		{
			/// <summary>
			///     check if this entry is satisfied by the given pawn
			/// </summary>
			/// <param name="pawn">The pawn.</param>
			/// <returns></returns>
			bool Satisfied([NotNull] Pawn pawn);

		}
	}


	/// <summary>
	///     base class for all 'worker' classes for mutation rules
	/// </summary>
	public abstract class MutationRuleWorker
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="MutationRuleWorker" /> class.
		/// </summary>
		/// <param name="ruleDef">The rule definition.</param>
		protected MutationRuleWorker([NotNull] MutationRuleDef ruleDef)
		{
			RuleDef = ruleDef;
		}

		/// <summary>
		///     Gets the rule definition.
		/// </summary>
		/// <value>
		///     The rule definition.
		/// </value>
		[NotNull]
		public MutationRuleDef RuleDef { get; }

		/// <summary>
		///     Tries to execute the rule on the given pawn.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>if the rule was successfully executed on the pawn</returns>
		/// <exception cref="System.ArgumentNullException">pawn</exception>
		public bool TryRule([NotNull] Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));

			if (!ConditionsMet(pawn)) return false;

			if (RuleDef.mtth <= 0 || Rand.MTBEventOccurs(RuleDef.mtth, TimeMetrics.TICKS_PER_DAY, MutationRuleDef.CHECK_RATE))
			{
				DoRule(pawn);
				return true;
			}

			return false;
		}

		/// <summary>
		///     checks if the given pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">pawn</exception>
		protected abstract bool ConditionsMet([NotNull] Pawn pawn);


		/// <summary>
		///     Does the rule on the given pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <exception cref="System.ArgumentNullException">pawn</exception>
		protected abstract void DoRule([NotNull] Pawn pawn);
	}

	/// <summary>
	///     simple implementation of MutationRuleWorker
	/// </summary>
	/// <seealso cref="Pawnmorph.MutationRuleWorker" />
	public class DefaultMutationRuleWorker : MutationRuleWorker
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="MutationRuleWorker" /> class.
		/// </summary>
		/// <param name="ruleDef">The rule definition.</param>
		public DefaultMutationRuleWorker([NotNull] MutationRuleDef ruleDef) : base(ruleDef)
		{
		}

		/// <summary>
		///     checks if the given pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">pawn</exception>
		protected override bool ConditionsMet(Pawn pawn)
		{
			return RuleDef.AllRules?.All(p => p.Satisfied(pawn)) == true;
		}

		/// <summary>
		///     Does the rule on the given pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <exception cref="System.ArgumentNullException">pawn</exception>
		protected override void DoRule(Pawn pawn)
		{
			List<MutationRuleDef.Result> entry = RuleDef.results;
			foreach (MutationRuleDef.Result result in entry.MakeSafe())
			{
				DoRule(pawn, result);
				OnRuleApplied(pawn, result);
			}
			OnRuleApplied(pawn);
		}


		/// <summary>
		/// Called when the rule is successfully applied 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		protected virtual void OnRuleApplied([NotNull] Pawn pawn)
		{
			//empty 
		}

		/// <summary>
		/// Called when the rule is successfully applied
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="result">The result.</param>
		protected virtual void OnRuleApplied([NotNull] Pawn pawn, [NotNull] MutationRuleDef.Result result)
		{

		}

		/// <summary>
		///     Does the rule on the given pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="result">The result.</param>
		protected virtual void DoRule([NotNull] Pawn pawn, [NotNull] MutationRuleDef.Result result)
		{
			if (result?.hediffDef == null) return;
			if (result.hediffDef is MutationDef mDef)
			{
				//handle mutations correctly 
				MutationUtilities.AddMutation(pawn, mDef);
				return;
			}


			if (result.partDef == null)
			{
				pawn.health.AddHediff(result.hediffDef);
				IntermittentMagicSprayer.ThrowMagicPuffUp(pawn.GetCorrectPosition().ToVector3(), pawn.GetCorrectMap());
				return;
			}


			foreach (BodyPartRecord partRecord in pawn.health.hediffSet.GetAllNonMissingWithoutProsthetics())
			{
				if (partRecord.def != result.partDef) continue;
				//add the hediff to all parts 
				pawn.health.AddHediff(result.hediffDef, partRecord);
				IntermittentMagicSprayer.ThrowMagicPuffUp(pawn.GetCorrectPosition().ToVector3(), pawn.GetCorrectMap());
			}
		}
	}
}