// AddAspectEffectProps.cs created by Iron Wolf for Pawnmorph on 07/30/2021 7:01 AM
// last updated 07/30/2021  7:01 AM

using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Pawnmorph.ThingComps
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="RimWorld.CompProperties_UseEffect" />
	public class AddAspectEffectProps : CompProperties_UseEffect
	{
		/// <summary>
		/// The aspect to give 
		/// </summary>
		public AspectDef aspect;
		/// <summary>
		/// The stage to give 
		/// </summary>
		public int? stage;
		/// <summary>
		/// Initializes a new instance of the <see cref="AddAspectEffectProps"/> class.
		/// </summary>
		public AddAspectEffectProps()
		{
			compClass = typeof(AddAspectEffect);
		}

		/// <summary>
		/// gets all configuration errors with this instance.
		/// </summary>
		/// <param name="parentDef">The parent definition.</param>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string configError in base.ConfigErrors(parentDef))
			{
				yield return configError;
			}

			if (aspect == null) yield return "no aspect set";
		}
	}

	/// <summary>
	/// effect to add a specific aspect on use 
	/// </summary>
	/// <seealso cref="RimWorld.CompUseEffect" />
	public class AddAspectEffect : CompUseEffect
	{
		AddAspectEffectProps Props
		{
			get
			{
				try
				{
					return (AddAspectEffectProps)props;
				}
				catch (InvalidCastException e)
				{
					throw new
						InvalidCastException($"unable to cast {props?.GetType().Name} to {nameof(AddAspectEffectProps)} on {parent.def.defName}!",
											 e);
				}
			}
		}

		private const string COULD_NOT_ADD = "PMAspectCouldNotBeAddedInjector";
		private const string ASPECT_LABEL = "Aspect";
		private const string PAWN_LABEL = "Pawn";

		/// <summary>
		/// Does the effect.
		/// </summary>
		/// <param name="usedBy">The used by.</param>
		public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);

			var aspect = Props.aspect;
			var stg = Props.stage;
			var aTracker = usedBy.GetAspectTracker();
			string message;
			if (aTracker != null)
			{
				if (stg != null)
				{
					var oldAspect = aTracker.GetAspect(aspect);
					if (oldAspect?.StageIndex == stg)
					{
						message = COULD_NOT_ADD;
					}
					else
					{
						aTracker.Remove(oldAspect);
						aTracker.Add(aspect, stg.Value);
						message = null;
					}
				}
				else
				{
					if (aTracker.Contains(aspect))
					{
						message = COULD_NOT_ADD;
					}
					else
					{
						aTracker.Add(aspect);
						message = null;
					}
				}
			}
			else return;

			if (message != null)
			{
				Messages.Message(message.Translate(aspect.Named(ASPECT_LABEL), usedBy.Named(PAWN_LABEL)), usedBy, MessageTypeDefOf.RejectInput);
			}

		}
	}
}