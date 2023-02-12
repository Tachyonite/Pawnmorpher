// State_Hunting.cs modified by Iron Wolf for Pawnmorph on 12/15/2019 5:01 PM
// last updated 12/15/2019  5:01 PM

using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Mental
{
	/// <summary>
	/// mental state for the hunting mental break 
	/// </summary>
	/// <seealso cref="Verse.AI.MentalState" />
	public class State_Hunting : MentalState
	{
		private Pawn _prey;
		private Job _job;
		/// <summary>
		/// called when the mental break starts
		/// </summary>
		/// <param name="reason">The reason.</param>
		public override void PostStart(string reason)
		{
			base.PostStart(reason);

			if (_prey != null)
			{
				_job = new Job(JobDefOf.PredatorHunt, _prey);
			}

		}
		/// <summary>
		/// Exposes the data.
		/// </summary>
		public override void ExposeData()
		{
			Scribe_References.Look(ref _prey, "prey");
			base.ExposeData();
		}



		/// <summary>
		/// Gets the prey.
		/// </summary>
		/// <value>
		/// The prey.
		/// </value>
		public Pawn Prey => _prey;

		/// <summary>
		/// Mentals the state tick.
		/// </summary>
		public override void MentalStateTick()
		{
			base.MentalStateTick();
			if (pawn.IsHashIntervalTick(60) && (Prey?.Dead ?? true))
				RecoverFromState();
		}


		/// <summary>
		/// Notifies the attacked target.
		/// </summary>
		/// <param name="hitTarget">The hit target.</param>
		public override void Notify_AttackedTarget(LocalTargetInfo hitTarget)
		{
			base.Notify_AttackedTarget(hitTarget);
			if (hitTarget.Thing == Prey && Prey.Dead)
			{
				RecoverFromState();
			}
		}
		/// <summary>
		/// called when this state ends.
		/// </summary>
		public override void PostEnd()
		{
			if (def.recoveryMessage.NullOrEmpty() || !PawnUtility.ShouldSendNotificationAbout(pawn))
				return;
			string str = def.recoveryMessage.Formatted(pawn.LabelShort, Prey.LabelShort.Named("prey"), Prey.Named("PREYFULL"), pawn.Named("PAWN"));
			if (str.NullOrEmpty())
				return;
			Messages.Message(str.AdjustedFor(pawn).AdjustedFor(Prey, "PREYFULL").CapitalizeFirst(), pawn, MessageTypeDefOf.SituationResolved);

		}

		/// <summary>
		/// called before the mental state is started 
		/// </summary>
		public override void PreStart()
		{
			_prey = FormerHumanUtilities.FindRandomPreyFor(pawn);
			if (_prey == null)
			{
				Log.Error($"could not find prey for {pawn.Name}");
			}
		}
		/// <summary>
		/// Gets the begin letter text.
		/// </summary>
		/// <returns></returns>
		public override TaggedString GetBeginLetterText()
		{
			return def.beginLetter.Formatted(pawn.LabelShort, pawn.Named("PAWN"), _prey.LabelShort.Named("prey")).AdjustedFor(this.pawn, "PAWN").CapitalizeFirst();
		}
	}
}