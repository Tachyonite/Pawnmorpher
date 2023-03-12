// DatabaseStorageComp.cs created by Iron Wolf for Pawnmorph on 08/03/2020 4:57 PM
// last updated 08/03/2020  4:57 PM

using System.Text;
using Pawnmorph.Chambers;
using RimWorld;
using Verse;

namespace Pawnmorph.ThingComps
{
	/// <summary>
	///     comp to add to buildings to have them add database storage
	/// </summary>
	/// <seealso cref="Verse.ThingComp" />
	public class DatabaseStorage : ThingComp
	{
		private bool _added;
		private bool _powered;
		private DatabaseStorageProperties Props => (DatabaseStorageProperties)props;

		private ChamberDatabase Database => Find.World.GetComponent<ChamberDatabase>();

		/// <inheritdoc />
		public override void ReceiveCompSignal(string signal)
		{
			if (signal == CompPowerTrader.PowerTurnedOnSignal && _powered == false)
			{
				_powered = true;
				Database.NotifyPowerOn(Props.storageAmount);
			}
			else if (signal == CompPowerTrader.PowerTurnedOffSignal && _powered)
			{
				_powered = false;
				Database.NotifyLostPower(Props.storageAmount);
			}
			base.ReceiveCompSignal(signal);
		}

		/// <summary>
		///     called after this thing is destroyed
		/// </summary>
		/// <param name="mode">The mode.</param>
		/// <param name="previousMap">The previous map.</param>
		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			base.PostDestroy(mode, previousMap);
			if (_added)
			{
				var database = Find.World.GetComponent<ChamberDatabase>();
				database.TotalStorage -= Props.storageAmount;

				// Clear inactive storage.
				if (_powered == false)
					Database.NotifyPowerOn(Props.storageAmount);
			}
		}

		private const string PROVIDE_MESSAGE_TAG = "PMStorageSpaceMessage";

		/// <summary>
		/// Comps the inspect string extra.
		/// </summary>
		/// <returns></returns>
		public override string CompInspectStringExtra()
		{
			StringBuilder builder = new StringBuilder();
			var wComp = Find.World.GetComponent<ChamberDatabase>();
			var provideStr = $"{DatabaseUtilities.GetStorageString(Props.storageAmount)}/{DatabaseUtilities.GetStorageString(wComp.TotalStorage)}";

			builder.AppendLine(PROVIDE_MESSAGE_TAG.Translate());
			builder.Append(provideStr);
			return builder.ToString();
		}

		/// <summary>
		///     expose data.
		/// </summary>
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref _added, "added");
			Scribe_Values.Look(ref _powered, "powered");
		}


		/// <summary>
		///     called after the parent is spawned in
		/// </summary>
		/// <param name="respawningAfterLoad">if set to <c>true</c> [respawning after load].</param>
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);

			LessonAutoActivator.TeachOpportunity(PMConceptDefOf.PM_Genebanks, OpportunityType.Important);


			if (!respawningAfterLoad && parent.Faction == Faction.OfPlayer)
			{
				_added = true;
				var database = Find.World.GetComponent<ChamberDatabase>();
				database.TotalStorage += Props.storageAmount;
				Database.NotifyLostPower(Props.storageAmount);
				// Buildings are initially unpowered once constructed.
			}
		}
	}

	/// <summary>
	///     comp properties for
	/// </summary>
	/// <seealso cref="Verse.CompProperties" />
	public class DatabaseStorageProperties : CompProperties
	{
		/// <summary>
		///     the amount of storage to add
		/// </summary>
		public int storageAmount = 10;

		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseStorageProperties" /> class.
		/// </summary>
		public DatabaseStorageProperties()
		{
			compClass = typeof(DatabaseStorage);
		}
	}
}