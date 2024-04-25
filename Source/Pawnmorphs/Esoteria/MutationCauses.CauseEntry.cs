// MutationCauses.CauseEntry.cs created by Iron Wolf for Pawnmorph on //2021 
// last updated 09/04/2021  7:53 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
using Verse.Grammar;

namespace Pawnmorph
{
	public partial class MutationCauses
	{
		/// <summary>
		///     a single cause entry
		/// </summary>
		/// <seealso cref="Verse.IExposable" />
		public abstract class CauseEntry : IExposable
		{
			/// <summary>
			///     The prefix for the cause
			/// </summary>
			public string prefix;

			/// <summary>
			///     Exposes the data.
			/// </summary>
			public virtual void ExposeData()
			{
				Scribe_Values.Look(ref prefix, nameof(prefix));
			}

			/// <summary>
			///     Gets the type of the definition that this cause uses
			/// </summary>
			/// <value>
			///     The type of the definition.
			/// </value>
			public abstract Type DefType { get; }

			/// <summary>
			///     The cause def
			/// </summary>
			public abstract Def Def { get; }

			/// <summary>
			///     Generates the rule strings for this cause
			/// </summary>
			/// <param name="additionalPrefix">The additional prefix</param>
			/// <returns></returns>
			[NotNull]
			public abstract IEnumerable<Rule> GenerateRules(string additionalPrefix = "");
		}


		/// <summary>
		///     generic subclass of CauseEntry
		/// </summary>
		/// this is needed as Scribe_Def&lt;Def&gt; gets confused with multiple defs of different types with the same name
		/// <typeparam name="T"></typeparam>
		/// <seealso cref="Pawnmorph.MutationCauses.CauseEntry" />
		public class SpecificDefCause<T> : CauseEntry where T : Def, new()
		{
			/// <summary>
			///     The definition field
			/// </summary>
			public T causeDef;

			/// <summary>
			///     Gets the type of the definition that this cause uses
			/// </summary>
			/// <value>
			///     The type of the definition.
			/// </value>
			public override Type DefType => typeof(T);

			/// <summary>
			///     The cause def
			/// </summary>
			public override Def Def => causeDef;

			/// <summary>
			///     Exposes the data.
			/// </summary>
			public override void ExposeData()
			{
				base.ExposeData();
				Scribe_Defs.Look(ref causeDef, "def");
			}

			/// <summary>
			///     Generates the rule strings for this cause
			/// </summary>
			/// <returns></returns>
			public override IEnumerable<Rule>
				GenerateRules(string additionalPrefix = "") //overriding simply to give more info in the case of an error 
			{
				//can be cached per prefix & def if performance becomes an issue 
				//if so implement IEquatable<> so it can be used with dicts 

				if (causeDef == null)
				{
					Log.Warning($"encountered null def in {typeof(T).Name}");
					return Enumerable.Empty<Rule>();
				}

				string pfx;
				if (!additionalPrefix.NullOrEmpty())
				{
					if (!prefix.NullOrEmpty())
						pfx = additionalPrefix + "_" + prefix;
					else pfx = additionalPrefix;
				}
				else
				{
					pfx = prefix;
				}

				if (causeDef is ICauseRulePackContainer cDef) return cDef.GetRules(pfx);


				//in addition to sub classing there should be a way to add additional rules via xml 
				//probably a CauseRuleExtension or something 
				return GetFromDef(causeDef, pfx);
			}

			/// <summary>Returns a string that represents the current object.</summary>
			/// <returns>A string that represents the current object.</returns>
			public override string ToString()
			{
				string pfx = prefix.NullOrEmpty() ? "" : prefix + "_";
				return $"{typeof(T).Name}: {pfx}{causeDef?.defName ?? "NULL DEF"}";
			}

			[NotNull]
			private IEnumerable<Rule> GetFromDef(T def, string prefix)
			{
				foreach (Rule rule in GrammarUtility.RulesForDef(prefix, def).MakeSafe()) yield return rule;

				IEnumerable<Rule> exts = def.modExtensions.MakeSafe()
											.OfType<ICauseRulePackContainer>()
											.SelectMany(e => e.GetRules(prefix));
				foreach (Rule rule in exts) yield return rule;
			}
		}

		/// <summary>
		/// cause entry for precepts 
		/// </summary>
		/// <seealso cref="Pawnmorph.MutationCauses.CauseEntry" />
		public class PreceptEntry : CauseEntry
		{
			/// <summary>
			/// The precept
			/// </summary>
			public Precept precept;


			/// <summary>
			///     Exposes the data.
			/// </summary>
			public override void ExposeData()
			{
				base.ExposeData();
				Scribe_References.Look(ref precept, nameof(precept));
			}

			/// <summary>
			///     Gets the type of the definition that this cause uses
			/// </summary>
			/// <value>
			///     The type of the definition.
			/// </value>
			public override Type DefType => typeof(PreceptDef);

			/// <summary>
			///     The cause def
			/// </summary>
			public override Def Def => precept?.def;

			/// <summary>
			///     Generates the rule strings for this cause
			/// </summary>
			/// <param name="additionalPrefix">The additional prefix</param>
			/// <returns></returns>
			public override IEnumerable<Rule> GenerateRules(string additionalPrefix = "")
			{
				if (precept == null)
				{
					Log.Warning("unable to get rules for precept as precept is missing!");
					return Enumerable.Empty<Rule>();
				}


				string pfx;
				if (!additionalPrefix.NullOrEmpty())
				{
					if (!prefix.NullOrEmpty())
						pfx = additionalPrefix + "_" + prefix;
					else pfx = additionalPrefix;
				}
				else
				{
					pfx = prefix;
				}

				return GrammarUtility.RulesForPrecept(pfx, precept);
			}

			/// <summary>Returns a string that represents the current object.</summary>
			/// <returns>A string that represents the current object.</returns>
			public override string ToString()
			{
				return $"Precept Cause[{prefix}]={precept?.Label ?? "NO PRECEPT"}";
			}
		}
	}
}