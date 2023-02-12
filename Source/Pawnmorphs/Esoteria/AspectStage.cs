// AspectStage.cs created by Iron Wolf for Pawnmorph on 09/23/2019 12:16 PM
// last updated 09/23/2019  12:16 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
	/// <summary> Class representing a single stage of a mutation 'aspect'. </summary>
	public class AspectStage
	{
		/// <summary>
		/// the label of the stage 
		/// </summary>
		public string label;
		/// <summary>
		/// prefix to add to the aspects label 
		/// </summary>
		public string modifier = "";
		/// <summary>
		/// the description of the aspect at this stage 
		/// </summary>
		public string description;
		/// <summary>
		/// how often the mental state givers give breaks 
		/// </summary>
		public float mentalBreakMtbDays;
		/// <summary>
		/// optional override to the aspects label color 
		/// </summary>
		public Color? labelColor;

		/// <summary>
		/// if an aspect at this stage should be considered 'bad'
		/// </summary>
		public bool isBad;

		/// <summary>
		/// text displayed in a message when a pawn is given this aspect 
		/// </summary>
		/// this is adjusted for [PAWN_nameDef] kinds of substitution 
		public string messageText;

		/// <summary>
		/// The message definition, if null NeutralEvent is used 
		/// </summary>
		[CanBeNull]
		public MessageTypeDef messageDef;


		/// <summary>
		/// list of thoughts this aspect stage nullifies 
		/// </summary>
		[NotNull] public List<ThoughtDef> nullifiedThoughts = new List<ThoughtDef>();

		/// <summary>
		/// all capacity modifiers that will be active during this stage 
		/// </summary>
		[CanBeNull] public List<PawnCapacityModifier> capMods;

		/// <summary>
		/// all skill modifiers that will be active during this stage 
		/// </summary>
		[CanBeNull] public List<SkillMod> skillMods;

		/// <summary>
		/// all stat offsets that will be active during this stage 
		/// </summary>
		[CanBeNull] public List<StatModifier> statOffsets;

		/// <summary>
		/// all mental states that can be given by the aspect in this stage 
		/// </summary>
		[CanBeNull] public List<MentalStateGiver> mentalStateGivers;

		/// <summary>
		/// all production boosts an aspect gives in this stage 
		/// </summary>
		[CanBeNull] public List<ProductionBoost> productionBoosts;

		[Unsaved(false)] private string cachedLabelCap;

		/// <summary>Gets the capitalized version of the stage's label.</summary>
		public string LabelCap
		{
			get
			{
				if (label.NullOrEmpty())
				{
					return null;
				}
				if (cachedLabelCap.NullOrEmpty())
				{
					cachedLabelCap = label.CapitalizeFirst();
				}
				return cachedLabelCap;
			}
		}
	}
}