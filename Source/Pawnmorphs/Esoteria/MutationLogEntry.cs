// MutationLogEntry.cs created by Iron Wolf for Pawnmorph on 10/09/2019 12:03 PM
// last updated 10/09/2019  12:03 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Grammar;
using static Pawnmorph.DebugUtils.DebugLogUtils;

namespace Pawnmorph
{
	/// <summary> Log entry for when a pawn mutates. </summary>
	public class MutationLogEntry : LogEntry
	{

		// Rule pack constants.
		private const string PAWN_IDENTIFIER = "PAWN";
		private const string MUTATION_IDENTIFIER = "MUTATION";
		private const string RP_ROOT_RULE = "mutation_log";
		private const string PART_LABEL = "PART";

		/// <summary>
		/// identifier for a block of text representing the cause of the mutation from a mutagen 
		/// </summary>
		public const string MUTAGEN_CAUSE_STRING = "mutagen_cause";

		/// <summary>
		/// identifier for a block of text representing the default caused by part of the mutation log. 
		/// </summary>
		public const string MUTAGEN_CAUSED_BY_STRING = "caused_by";

		private HediffDef _mutationDef;
		private BodyPartRecord _bodyPart;
		private Pawn _pawn;
		private MutationCauses _causes;

		/// <summary>
		/// Initializes a new instance of the <see cref="MutationLogEntry"/> class.
		/// </summary>
		public MutationLogEntry()
		{
		}


		/// <summary>
		/// Gets the causes.
		/// </summary>
		/// <value>
		/// The causes.
		/// </value>
		[NotNull]
		public MutationCauses Causes
		{
			get
			{
				return _causes;
			}
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="MutationLogEntry"/> class.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="mutationDef">The mutation definition.</param>
		/// <param name="mutagenCause">The cause for this mutation (optional)</param>
		/// <param name="bodypart">The mutated body part.</param>
		public MutationLogEntry(Pawn pawn, HediffDef mutationDef, [CanBeNull] MutagenDef mutagenCause, BodyPartRecord bodypart)
		{
			_bodyPart = bodypart;
			_pawn = pawn;
			_mutationDef = mutationDef;
			_causes = new MutationCauses();

			if (mutagenCause != null)
				_causes.AddMutagenCause(mutagenCause);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MutationLogEntry"/> class.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="mutationDef">The mutation definition.</param>
		/// <param name="bodypart">The mutated part.</param>
		public MutationLogEntry(Pawn pawn, HediffDef mutationDef, BodyPartRecord bodypart)
		{
			_bodyPart = bodypart;
			_pawn = pawn;
			_mutationDef = mutationDef;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MutationLogEntry"/> class.
		/// </summary>
		/// <param name="pawn">The pawn that was mutated.</param>
		/// <param name="mutation">The mutation itself.</param>
		public MutationLogEntry(Pawn pawn, Hediff_AddedMutation mutation)
			: this(pawn, mutation.def, mutation.Part)
		{
			_causes = mutation.Causes;
		}


		/// <summary>
		/// true if this log is about the given thing.
		/// </summary>
		/// <param name="t">The t.</param>
		/// <returns></returns>
		public override bool Concerns(Thing t)
		{
			return t == _pawn;
		}

		/// <summary>
		/// Exposes the data.
		/// </summary>
		public override void ExposeData()
		{
			base.ExposeData();

			Scribe_Defs.Look(ref _mutationDef, nameof(_mutationDef));
			Scribe_BodyParts.Look(ref _bodyPart, nameof(_bodyPart));
			Scribe_References.Look(ref _pawn, nameof(_pawn));
			Scribe_Deep.Look(ref _causes, "causes");
		}

		/// <summary>
		/// Gets everything this log is about.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<Thing> GetConcerns()
		{
			yield return _pawn;
		}

		/// <summary>
		/// Gets the tip string.
		/// </summary>
		/// <returns></returns>
		public override string GetTipString()
		{
			return $"{_mutationDef.LabelCap}";
		}

		/// <summary> Returns a string that represents the current object. </summary>
		/// <returns> A string that represents the current object. </returns>
		public override string ToString()
		{
			return $"{_pawn.Name}: {_bodyPart.LabelCap} -> {_mutationDef.LabelCap}";
		}

		private const string UNKNOWN_CAUSE = "PmUnkownMutagenCause";

		/// <summary>
		/// create the main log text 
		/// </summary>
		/// <param name="pov">The pov.</param>
		/// <param name="forceLog">if set to <c>true</c> [force log].</param>
		/// <returns></returns>
		protected override string ToGameStringFromPOV_Worker(Thing pov, bool forceLog)
		{
			Assert(pov == _pawn, "pov == _pawn");

			Rand.PushState(logID); // Does not need a MP-safe seed.
			try
			{
				GrammarRequest grammarRequest = GenerateGrammarRequest();

				RulePackDef mutationRulePack = (_mutationDef as MutationDef)?.mutationLogRulePack;

				if (mutationRulePack != null)
				{
					grammarRequest.Includes.Add(mutationRulePack);
				}
				else
				{
					grammarRequest.Includes.Add(PMRulePackDefOf.GetDefaultPackForMutation(_mutationDef));
					grammarRequest.Rules.Add(new Rule_String("DATE", GenDate.DateFullStringAt(ticksAbs, Vector2.zero)));
				}
				AddCustomRules(grammarRequest.Rules);

				IEnumerable<Rule> pawnR = GrammarUtility.RulesForPawn(PAWN_IDENTIFIER, _pawn, grammarRequest.Constants);

				IEnumerable<Rule> partRules;
				IEnumerable<Rule> mutR;
				if (_bodyPart != null)
				{
					partRules = GrammarUtility.RulesForBodyPartRecord(PART_LABEL, _bodyPart);
					mutR = GrammarUtility.RulesForHediffDef(MUTATION_IDENTIFIER, _mutationDef, _bodyPart);
				}
				else
				{
					partRules = Enumerable.Empty<Rule>();
					mutR = Enumerable.Empty<Rule>();
				}

				if (_causes != null)
				{
					grammarRequest.Rules.AddRange(_causes.GenerateRules());
				}

				if (grammarRequest.HasRule(MUTAGEN_CAUSE_STRING))
				{
					if (grammarRequest.HasRule(MUTAGEN_CAUSED_BY_STRING) == false)
						grammarRequest.Rules.Add(new Rule_String(MUTAGEN_CAUSED_BY_STRING, "Log_CausedBy".Translate()));
					//grammarRequest.Rules.Add(new Rule_String(MUTAGEN_CAUSE_STRING, UNKNOWN_CAUSE.Translate()));
				}
				else
				{
					grammarRequest.Rules.Add(new Rule_String(MUTAGEN_CAUSED_BY_STRING, ""));
					grammarRequest.Rules.Add(new Rule_String(MUTAGEN_CAUSE_STRING, ""));
				}



				// Add the rules.
				grammarRequest.Rules.AddRange(pawnR);
				grammarRequest.Rules.AddRange(mutR);
				grammarRequest.Rules.AddRange(partRules);
				return GrammarResolver.Resolve(RP_ROOT_RULE, grammarRequest, "mutation log", forceLog);
			}
			catch (Exception exception)
			{
				Log.Error($"encountered {exception.GetType().Name} exception while generating string for mutation log\n\t{exception}");
			}
			finally
			{
				Rand.PopState(); // Make sure to always pop rand.
			}

			return _mutationDef?.LabelCap ?? "INVALID MUTATION LOG ENTRY";
		}


		private const string MODIFIER_RULE_KEYWORD = "modifier";
		private const string VOWEL_CHECK = "aeiouAEIOU";




		/// <summary>
		/// if a word starts with a vowel, return 'an' else return 'a'
		/// </summary>
		/// <param name="word"></param>
		/// <returns></returns>
		string GetAAn(string word)
		{
			return VOWEL_CHECK.IndexOf(word[0]) > 0 ? "an" : "a";

		}

		private void AddCustomRules(List<Rule> grammarRequestRules)
		{
			if (!grammarRequestRules.Any(r => r.keyword == MODIFIER_RULE_KEYWORD))
			{
				var rule = new Rule_String(MODIFIER_RULE_KEYWORD, ""); //TODO check for modifier using morphDef (grab morphDef from when mutation was added) 
				grammarRequestRules.Add(rule); //add a blank modifier if none is set 
			}

			if (!grammarRequestRules.Any(r => r.keyword == "a_an"))
			{
				var split = _mutationDef.label.Split(' ');
				if (split.Length > 1) //label is two words, like 'wolf tail' or 'fox muzzle'
				{
					grammarRequestRules.Add(new Rule_String("a_an", GetAAn(split[0])));
				}
				else //if the label is one word it's probably a modifier like 'wolfish'
				{
					grammarRequestRules.Add(new Rule_String("a_an", ""));
				}
			}
		}
	}
}