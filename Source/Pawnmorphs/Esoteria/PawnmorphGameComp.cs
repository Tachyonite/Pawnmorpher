// PawnmorphGameComp.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/07/2019 2:13 PM
// last updated 08/14/2019  7:01 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienRace;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.Chambers;
using Pawnmorph.Hybrids;
using Pawnmorph.TfSys;
using Pawnmorph.Utilities;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// the world comp for this mod 
	/// </summary>
	/// <seealso cref="RimWorld.Planet.WorldComponent" />
	public class PawnmorphGameComp : WorldComponent
	{
		/// <summary>
		/// Occurs when PM static caches should be cleared.
		/// </summary>
		public static event Action<PawnmorphGameComp> OnClear;


		[Obsolete]
		internal HashSet<PawnMorphInstance> pawnmorphs = new HashSet<PawnMorphInstance>();

		[Obsolete]
		internal HashSet<PawnMorphInstanceMerged>
			mergedpawnmorphs = new HashSet<PawnMorphInstanceMerged>(); //why are we using hashsets? 

		/// <summary>all tagged animals</summary>
		[Obsolete("use " + nameof(ChamberDatabase) + "." + nameof(ChamberDatabase.TaggedAnimals) + " instead")]
		public HashSet<PawnKindDef> taggedAnimals = new HashSet<PawnKindDef>();

		private List<TransformedPawn> _transformedPawns = new List<TransformedPawn>();

		//hacky, will find a better solution later 
		internal bool sheepChefEventFired;

		List<Hediff_AddedMutation> _managedMutations = new List<Hediff_AddedMutation>(300);

		float _currentSpreadableProgress = 0;
		float _currentSeverityProgress = 0;
		int _currentSpreadableIterator = 0;
		int _currentSeverityIterator = 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="PawnmorphGameComp"/> class.
		/// </summary>
		/// <param name="world">The world.</param>
		public PawnmorphGameComp(World world) : base(world)
		{
			PawnmorpherMod.WorldComp = this;
		}

		private List<TransformedPawn> TransformedPawnsLst //scribe can set _transformedPawns to null in an old save 
		{
			get
			{
				if (_transformedPawns == null)
				{

					Log.Warning(nameof(_transformedPawns) + " is null! ");
					_transformedPawns = new List<TransformedPawn>();
				}

				return _transformedPawns;
			}
		}

		/// <summary>
		/// called when the world is finishing initialization 
		/// </summary>
		public override void FinalizeInit()
		{
			base.FinalizeInit();

			AddSurgeriesToMorphs();

			CheckForBadDrugPolicies();
		}

		private void CheckForBadDrugPolicies()
		{
			var drugDB = Current.Game?.drugPolicyDatabase;
			if (drugDB == null)
			{
				return;
			}

            System.Reflection.MethodInfo initIfNeeded = AccessTools.Method(typeof(DrugPolicy), "InitializeIfNeeded");
			object[] arguments = [true];
			foreach (DrugPolicy drugPolicy in drugDB.AllPolicies.MakeSafe())
			{
				initIfNeeded.Invoke(drugPolicy, arguments);
			}
		}

		private void AddSurgeriesToMorphs()
		{


			var humanRecipes = ThingDefOf.Human.AllRecipes;
			foreach (ThingDef_AlienRace race in RaceGenerator.ImplicitRaces)
			{
				var rcList = race.AllRecipes;

				//add all that were added by other mods/recipie users/etc. to the morphs 
				foreach (RecipeDef recipe in humanRecipes)
				{
					if (!rcList.Contains(recipe))
						rcList.Add(recipe);
				}

			}
		}


		/// <summary>Gets all transformed pawns.</summary>
		/// <value>The transformed pawns.</value>
		public IEnumerable<TransformedPawn> TransformedPawns => TransformedPawnsLst;
		/// <summary>Adds the transformed pawn.</summary>
		/// <param name="tfPair">The tf pair.</param>
		/// <exception cref="ArgumentNullException">tfPair</exception>
		public void AddTransformedPawn([NotNull] TransformedPawn tfPair)
		{
			if (tfPair == null) throw new ArgumentNullException(nameof(tfPair));
			if (!tfPair.IsValid)
			{
				Log.Error($"tried to add invalid transformed pawn! {tfPair.ToDebugString()}\n");
				return;
			}

			_transformedPawns.Add(tfPair);
		}
#pragma warning disable 612
#pragma warning disable 0618
		/// <summary>Exposes the data.</summary>
		public override void ExposeData()
		{
			Scribe_Collections.Look(ref pawnmorphs, "pawnmorphs", LookMode.Deep);
			Scribe_Collections.Look(ref mergedpawnmorphs, "pawnmorphs", LookMode.Deep); //these are needed for backwards compatibility with old saves 
			Scribe_Collections.Look(ref taggedAnimals, "taggedAnimals");
			Scribe_Collections.Look(ref _transformedPawns, "transformedPawns", LookMode.Deep);
			taggedAnimals = taggedAnimals ?? new HashSet<PawnKindDef>();
			mergedpawnmorphs = mergedpawnmorphs ?? new HashSet<PawnMorphInstanceMerged>();
			pawnmorphs = pawnmorphs ?? new HashSet<PawnMorphInstance>();

			Scribe_Values.Look(ref sheepChefEventFired, nameof(sheepChefEventFired));

			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{

				if (_transformedPawns == null)
				{
					Log.Warning($"_transformedPawns is null in PawnmorphGameComp, this should not happen unless this is an old save or Pawnmorph was just added");
					_transformedPawns = new List<TransformedPawn>();
				}

				// Transfer the old ones to the new system.
				foreach (var pawnMorphInstance in pawnmorphs)
				{
					_transformedPawns.Add(TfSys.TransformedPawn.Create(pawnMorphInstance));
				}

				foreach (var pawnMorphInstanceMerged in mergedpawnmorphs)
				{
					_transformedPawns.Add(TfSys.TransformedPawn.Create(pawnMorphInstanceMerged));
				}

				// Now clear.
				pawnmorphs.Clear();
				mergedpawnmorphs.Clear();

				if (OnClear != null)
					OnClear.Invoke(this);

				// Make sure they're all valid.
				ValidateTransformedPawns();
			}

		}
#pragma warning restore 612
#pragma warning restore 0618
		/// <summary> Validates the transformed pawns. </summary>
		void ValidateTransformedPawns()
		{

			StringBuilder builder = new StringBuilder();
			bool hasDroppedPawns = false;
			for (int i = TransformedPawnsLst.Count - 1; i >= 0; i--)
			{
				var inst = TransformedPawnsLst[i];
				if (!inst.IsValid)
				{
					builder.AppendLine($"encountered invalid transformed pawn instance: {inst}");
					foreach (var pawn in inst.OriginalPawns.Where(p => !p.DestroyedOrNull()))
					{
						if (!pawn.Spawned)
							pawn.Destroy();
					}

					if (inst.TransformedPawns.Any(t => !t.DestroyedOrNull()))
						hasDroppedPawns = true;

					TransformedPawnsLst.RemoveAt(i);
				}
			}

			if (builder.Length > 0 && hasDroppedPawns)
			{
				Log.Error($"encountered invalid transformed pawns instances:\n{builder}");
			}
		}


		/// <summary> Gets the pawn transformation status. </summary>
		/// <param name="p"> The pawn. </param>
		/// <returns> The pawn's current status or null. </returns>
		public TransformedStatus? GetPawnStatus(Pawn p)
		{
			foreach (var transformedPawn in TransformedPawnsLst)
			{
				var status = transformedPawn.GetStatus(p);
				if (status != null) return status;
			}

			return null;
		}

		/// <summary> Gets the transformed pawn containing the given pawn. </summary>
		/// <param name="pawn"> The pawn. </param>
		/// <returns> The TransformedPawn instance as well as the pawn's status to that instance. </returns>
		[CanBeNull]
		public (TransformedPawn pawn, TransformedStatus status)? GetTransformedPawnContaining(Pawn pawn)
		{
			foreach (TransformedPawn transformedPawn in TransformedPawns)
			{
				TransformedStatus? status = transformedPawn.GetStatus(pawn);

				if (status != null) return (transformedPawn, status.Value);
			}

			return null;
		}

		/// <summary> Removes the transformed instance from the list. </summary>
		/// <param name="tfPawn"> The tf pawn. </param>
		public void RemoveInstance(TransformedPawn tfPawn)
		{
			TransformedPawnsLst.Remove(tfPawn);
		}

		/// <summary>add the given pawnkind to the mutagen chamber database</summary>
		/// <param name="pawnkind">The pawnkind.</param>
		[Obsolete("use " + nameof(Chambers.ChamberDatabase) + "." + nameof(Chambers.ChamberDatabase.TryAddToDatabase) + " instead")]
		public void TagPawn(PawnKindDef pawnkind)
		{
			if (!taggedAnimals.Contains(pawnkind)) taggedAnimals.Add(pawnkind);
		}

		public override void WorldComponentTick()
		{
			base.WorldComponentTick();
			try
			{
				int count = _managedMutations.Count;
				_currentSpreadableProgress += count / 60f;
				if (_currentSpreadableProgress > 1)
				{
					float spreadableUpdates = _currentSpreadableProgress - 1;
					for (int i = 0; i < spreadableUpdates; i++)
					{
						_currentSpreadableIterator++;
						if (_currentSpreadableIterator >= count) 
							_currentSpreadableIterator = 0;
						
						Hediff_AddedMutation mutation = _managedMutations[_currentSpreadableIterator];

						// This whole operation is somewhat performance intensive, so only do it once a second
						if (mutation.pawn.Dead == false)
							mutation.SpreadingMutation?.UpdateComp();
						_currentSpreadableProgress--;
					}
				}

				// Each tick, add updates per tick fraction to progress.
				// When at least 1 progress needs updating, process.
				_currentSeverityProgress += count / 200f;
				if (_currentSeverityProgress > 1)
				{
					float severityUpdates = _currentSeverityProgress - 1;
					for (int i = 0; i < severityUpdates; i++)
					{
						_currentSeverityIterator++;
						if (_currentSeverityIterator >= count) 
							_currentSeverityIterator = 0;

						Hediff_AddedMutation mutation = _managedMutations[_currentSeverityIterator];

						// Called every 200 ticks
						if (mutation.pawn.Dead == false)
							mutation.SeverityAdjust?.UpdateComp();
						_currentSeverityProgress--;
					}
				}
			}
			catch (Exception)
			{
				Log.Warning($"Current stats: Registered: {_managedMutations.Count}, spread: {_currentSpreadableProgress}-{_currentSpreadableIterator}, severity: {_currentSeverityProgress}-{_currentSeverityIterator}");
				throw;
			}
		}

		/// <summary>
		/// Registers the mutation to have Spreadable and severity adjustment components updated.
		/// </summary>
		public void RegisterMutation(Hediff_AddedMutation mutation)
		{
			if (_managedMutations.Contains(mutation) == false)
				_managedMutations.Add(mutation);
		}

		/// <summary>
		/// Unregisters the mutation from the managed list.
		/// </summary>
		public void UnregisterMutation(Hediff_AddedMutation mutation)
		{
			if (_managedMutations.Contains(mutation))
				_managedMutations.Remove(mutation);
		}
	}
}