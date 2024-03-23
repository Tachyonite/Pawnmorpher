// FormerHuman.cs created by Iron Wolf for Pawnmorph on 04/25/2020 9:33 AM
// last updated 04/25/2020  9:34 AM

using System;
using System.Linq;
using Pawnmorph.TfSys;
using Pawnmorph.Thoughts;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.SapienceStates
{
	/// <summary>
	/// sapience state for former humans 
	/// </summary>
	/// <seealso cref="Pawnmorph.SapienceState" />
	public class FormerHuman : SapienceState
	{
		private const float PERMANENTLY_FERAL_MTB = 3;

		/// <summary>
		/// Gets a value indicating whether this state makes the pawn count as a 'former human'.
		/// </summary>
		/// <value>
		///   <c>true</c> if this state makes the pawn count as a 'former human'; otherwise, <c>false</c>.
		/// </value>
		public override bool IsFormerHuman => true;

		/// <summary>
		/// called after every tick 
		/// </summary>
		public override void Tick()
		{
			if (_countdownStarted && Pawn.IsHashIntervalTick(60) && RandUtilities.MtbDaysEventOccured(PERMANENTLY_FERAL_MTB))
			{
				_countdownStarted = false;
				Tracker.MakePermanentlyFeral();
			}
		}

		internal void MakePermanentlyFeral()
		{
			Hediff fHediff;

			if (StateDef.forcedHediff != null)
			{
				fHediff = Pawn.health.hediffSet.GetFirstHediffOfDef(StateDef.forcedHediff);

			}
			else
			{
				fHediff = null;
			}

			//transfer relationships back if possible 
			var gComp = Find.World.GetComponent<PawnmorphGameComp>();
			Pawn oPawn = gComp.GetTransformedPawnContaining(Pawn)?.Item1?.OriginalPawns.FirstOrDefault();
			if (oPawn == Pawn) oPawn = null;

			Pawn_RelationsTracker aRelations = Pawn.relations;
			if (aRelations != null && oPawn != null) FormerHumanUtilities.TransferRelationsToOriginal(oPawn, Pawn);

			Pawn.health.AddHediff(TfHediffDefOf.PermanentlyFeral);
			if (fHediff != null)
				Pawn.health.RemoveHediff(fHediff);

			var loader = Find.World.GetComponent<PawnmorphGameComp>();
			TransformedPawn inst = loader.GetTransformedPawnContaining(Pawn)?.Item1;
			var singleInst = inst as TransformedPawnSingle; //hacky, need to come up with a better solution 
			foreach (Pawn instOriginalPawn in inst?.OriginalPawns ?? Enumerable.Empty<Pawn>()
			) //needed to handle merges correctly 
				ReactionsHelper.OnPawnPermFeral(instOriginalPawn, Pawn,
												singleInst?.reactionStatus ?? FormerHumanReactionStatus.Wild);

			//remove the original and destroy the pawns 
			foreach (Pawn instOriginalPawn in inst?.OriginalPawns ?? Enumerable.Empty<Pawn>()) instOriginalPawn.Destroy();

			if (inst != null) loader.RemoveInstance(inst);

			if (inst != null || Pawn.Faction == Faction.OfPlayer)
				Find.LetterStack.ReceiveLetter("LetterHediffFromPermanentTFLabel".Translate(Pawn.LabelShort).CapitalizeFirst(),
											   "LetterHediffFromPermanentTF".Translate(Pawn.LabelShort).CapitalizeFirst(),
											   LetterDefOf.NegativeEvent, Pawn);

		}

		/// <summary>
		///     Gets the current intelligence.
		/// </summary>
		/// <value>
		///     The current intelligence.
		/// </value>
		public override Intelligence CurrentIntelligence
		{
			get
			{

				if (Tracker == null)
				{
					return Pawn?.RaceProps?.intelligence ?? Intelligence.Animal;
				}

				switch (Tracker.SapienceLevel)
				{
					case SapienceLevel.Sapient:
					case SapienceLevel.MostlySapient:
					case SapienceLevel.Conflicted:
						return Intelligence.Humanlike;
					case SapienceLevel.MostlyFeral:
					case SapienceLevel.Feral:
					case SapienceLevel.PermanentlyFeral:
						return Intelligence.Animal;
					default:
						throw new ArgumentOutOfRangeException();
				}


			}
		}


		/// <summary>
		/// called when the pawn exits this state 
		/// </summary>
		public override void Exit()
		{
			base.Exit();

			if (_subscribed)
			{
				var need = Tracker.SapienceNeed;
				if (need != null)
				{
					need.SapienceLevelChanged -= OnSapienceLevelChanged;
				}
			}
		}

		/// <summary>
		///     called to save/load all data.
		/// </summary>
		protected override void ExposeData()
		{
			Scribe_Values.Look(ref _countdownStarted, "countdownStarted");
		}

		/// <summary>
		/// Adds the or remove dynamic components.
		/// </summary>
		public override void AddOrRemoveDynamicComponents()
		{
			switch (CurrentSapience)
			{
				case SapienceLevel.Sapient:
				case SapienceLevel.Conflicted:
				case SapienceLevel.MostlySapient:
					AddSapientAnimalComponents();
					break;

				case SapienceLevel.MostlyFeral:
				case SapienceLevel.Feral:
					MakeFeral();
					AddSapientAnimalComponents();//ferals need to keep them so stuff doesn't break, like relationships 
					RemoveNonFeralComps();
					break;

				case SapienceLevel.PermanentlyFeral:
					RemoveSapientAnimalComponents(); //actually removing the components seems to break stuff for some reason 
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void RemoveNonFeralComps()
		{
			Pawn.ideo = null;
		}

		private void RemoveSapientAnimalComponents()
		{


			//remove the drafter component if the animal is now feral 
			if (Pawn?.drafter != null)
				Pawn.drafter.Drafted = false;

			if (Pawn.MapHeld != null)
			{
				Pawn.equipment?.DropAllEquipment(Pawn.PositionHeld, Pawn.Faction?.IsPlayer != true);
				Pawn.apparel?.DropAll(Pawn.PositionHeld, Pawn.Faction?.IsPlayer != true);
			}
			else
			{
				Pawn.equipment?.DestroyAllEquipment();
				Pawn.apparel?.DestroyAll();
			}

			Pawn.ownership?.UnclaimAll();
			Pawn.workSettings?.EnableAndInitializeIfNotAlreadyInitialized();
			Pawn.workSettings?.DisableAll();
			//Pawn.ownership = null;
			Pawn.drafter = null;
			Pawn.apparel = null;
			Pawn.foodRestriction = null;
			Pawn.equipment = null;
			Pawn.royalty = null;
			Pawn.guest = null;
			Pawn.guilt = null;
			Pawn.drugs = null;
			Pawn.story = null;
			Pawn.abilities = null;
			Pawn.skills = null;
			Pawn.timetable = null;
			Pawn.workSettings = null;
			Pawn.outfits = null;
			Pawn.ideo = null;
			Pawn.style = null;
			Pawn.styleObserver = null;
			var saComp = Pawn.GetComp<Comp_SapientAnimal>();
			if (saComp != null)
			{
				Pawn.AllComps?.Remove(saComp);
			}
		}

		private void AddSapientAnimalComponents()
		{
			//add the drafter and equipment components 
			//if 
			if (Pawn.Faction?.IsPlayer == true)
			{
				if (Pawn.drafter == null)
				{
					Pawn.drafter = new Pawn_DraftController(Pawn);
					Pawn.jobs = Pawn.jobs ?? new Pawn_JobTracker(Pawn);
				}

				if (Pawn.workSettings == null)
				{
					Pawn.workSettings = new Pawn_WorkSettings(Pawn);
				}
			}

			Pawn.ownership = Pawn.ownership ?? new Pawn_Ownership(Pawn);
			Pawn.equipment = Pawn.equipment ?? new Pawn_EquipmentTracker(Pawn);
			Pawn.story = Pawn.story ?? new Pawn_StoryTracker(Pawn); //need to add story component to not break hospitality 
			Pawn.genes = Pawn.genes ?? new Pawn_GeneTracker(Pawn);
			Pawn.apparel = Pawn.apparel ?? new Pawn_ApparelTracker(Pawn); //need this to not break thoughts and stuff 
			Pawn.skills = Pawn.skills ?? new Pawn_SkillTracker(Pawn); //need this for thoughts 
			Pawn.royalty = Pawn.royalty ?? new Pawn_RoyaltyTracker(Pawn);// former humans can be royalty  
			Pawn.abilities = Pawn.abilities ?? new Pawn_AbilityTracker(Pawn);
			Pawn.mindState = Pawn.mindState ?? new Pawn_MindState(Pawn);
			Pawn.drugs = Pawn.drugs ?? new Pawn_DrugPolicyTracker(Pawn);
			Pawn.guest = Pawn.guest ?? new Pawn_GuestTracker(Pawn);
			Pawn.outfits = Pawn.outfits ?? new Pawn_OutfitTracker(Pawn);
			Pawn.guilt = Pawn.guilt ?? new Pawn_GuiltTracker(Pawn);
			Pawn.foodRestriction = Pawn.foodRestriction ?? new Pawn_FoodRestrictionTracker(Pawn);
			Pawn.timetable = Pawn.timetable ?? new Pawn_TimetableTracker(Pawn);
			Pawn.style = Pawn.style ?? new Pawn_StyleTracker(Pawn);
			Pawn.styleObserver = Pawn.styleObserver ?? new Pawn_StyleObserverTracker(Pawn);
			Comp_SapientAnimal nComp = Pawn.GetComp<Comp_SapientAnimal>();

			if (Pawn.ideo == null && ModLister.IdeologyInstalled)
			{
				Pawn.ideo = new Pawn_IdeoTracker(Pawn);
				Pawn.ideo.SetIdeo(Faction.OfPlayer.ideos.GetRandomIdeoForNewPawn());
			}


			if (nComp == null)
			{
				nComp = new Comp_SapientAnimal { parent = Pawn };
				Pawn.AllComps.Add(nComp);

				nComp.Initialize(new CompProperties());//just pass in empty props 
			}


			Pawn.workSettings?.EnableAndInitializeIfNotAlreadyInitialized();
		}


		void SubscribeToEvents()
		{
			if (_subscribed) return;
			var need = Tracker?.SapienceNeed;
			if (need == null) return;

			need.SapienceLevelChanged += OnSapienceLevelChanged;
			_subscribed = true;
		}

		private bool _countdownStarted;

		private void OnSapienceLevelChanged(Need_Control sender, Pawn pawn, SapienceLevel oldLevel, SapienceLevel currentLevel)
		{
			if (currentLevel == SapienceLevel.Feral)
			{
				_countdownStarted = true;
			}
			else if (_countdownStarted && currentLevel < SapienceLevel.Feral)
			{
				// Stop cooldown if sapience is increased above Feral
				_countdownStarted = false;
			}

			if (PawnUtility.ShouldSendNotificationAbout(pawn))
				SendFHLetter(pawn, oldLevel, currentLevel);
		}

		private static void SendFHLetter(Pawn pawn, SapienceLevel oldLevel, SapienceLevel currentLevel)
		{
			// The translation keys should be $SapienceLevel_IncreaseTransitionLabel and $SapienceLevel_IncreaseTransitionContent (or Decrease).
			string transition = oldLevel > currentLevel ? "Increase" : "Decrease"; //Enum: sapient is 0, feral is 4. So increase if the oldlevel is higher.
			string translationLabel = "FormerHuman" + currentLevel + "_" + transition + "Transition";
			string letterLabelKey = translationLabel + "Label";
			string letterContentKey = translationLabel + "Content";
			TaggedString letterContent, letterLabel;


			if (letterLabelKey.TryTranslate(out letterLabel) && letterContentKey.TryTranslate(out letterContent))
			{
				letterLabel = letterLabel.AdjustedFor(pawn);
				letterContent = letterContent.AdjustedFor(pawn);

				Find.LetterStack.ReceiveLetter(letterLabel, letterContent, LetterDefOf.NeutralEvent, new LookTargets(pawn));
			}
		}

		private bool _subscribed;




		/// <summary>
		///     Initializes this instance.
		/// </summary>
		/// this is always called before enter and after loading a pawn
		protected override void Init()
		{
			SubscribeToEvents();
		}
	}
}