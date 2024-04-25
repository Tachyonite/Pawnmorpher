// MutaChamber.cs created by Iron Wolf for Pawnmorph on 07/26/2020 7:50 PM
// last updated 07/26/2020  7:50 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienRace;
using JetBrains.Annotations;
using LudeonTK;
using Pawnmorph.Hediffs;
using Pawnmorph.TfSys;
using Pawnmorph.ThingComps;
using Pawnmorph.UserInterface;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Pawnmorph.Chambers
{
	/// <summary>
	/// </summary>
	/// <seealso cref="Building_Casket" />
	public class MutaChamber : Building_Casket, ISuspendableThingHolder
	{
		private const int REFUEL_CHECK_TIMER = 100;

		private const float TF_ANIMAL_DURATION = 1.5f; //units are in days 
		private const float MIN_TRANSFORMATION_TIME = 0.5f * 60000; //minimum transformation time in ticks
		private const string PART_PICKER_GIZMO_LABEL = "PMPartPickerGizmo";
		private const string PART_PICKER_GIZMO_DESC = "PMPartPickerGizmoDescription";
		private const string MERGING_GIZMO_LABEL = "PMMergeGizmo";
		private const string MERGING_GIZMO_DESC = "PMMergeGizmoDescription";
		private const string DEBUG_FORCE_COMPLETION_GIZMO = "PMDebugForceChamberCompletion";


		private static List<PawnKindDef> _randomAnimalCache;

		[NotNull]
		private readonly
			static List<Pawn> _scratchList = new List<Pawn>();

		private int _timer = 0;
		private int _curMutationIndex = -1;
		private bool _initialized;


		private ChamberState _innerState = ChamberState.WaitingForPawn;

		private ChamberUse _currentUse;

		private CompRefuelable _refuelable;

		private CompFlickable _flickable;

		private AnimalSelectorComp _aSelector;

		private Gizmo _ppGizmo;

		private Gizmo _mergingGizmo;

		private PawnKindDef _targetAnimal;
		private int _lastTotal = 0;


		/// <summary>
		/// The special thing that a tf is waiting on 
		/// </summary>
		private ThingDef _specialThing;

		private PawnKindDef _lastTfRequest;
		private Gizmo _debugFinishGizmo;

		private CompPowerTrader _power;


		private CompGlower _glower;

		private FillableBarDrawer _fillableDrawer;


		private IReadOnlyAddedMutations _addedMutationData;

		/// <summary>
		/// Gets a value indicating whether this instance has its contents suspended / in stasis.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance's contents are suspended; otherwise, <c>false</c>.
		/// </value>
		public bool IsContentsSuspended => PowerCompTrader?.PowerOn ?? false;

		/// <summary>
		///     Gets the current use.
		/// </summary>
		/// <value>
		///     The current use.
		/// </value>
		public ChamberUse CurrentUse => _currentUse;

		/// <summary>
		///     Gets a value indicating whether this chamber is waiting for a pawn.
		/// </summary>
		/// <value>
		///     <c>true</c> if this chamber is waiting for a pawn; otherwise, <c>false</c>.
		/// </value>
		public bool WaitingForPawn =>
			_innerState == ChamberState.WaitingForPawn || _innerState == ChamberState.WaitingForPawnMerging;

		/// <summary>
		///     Gets a value indicating whether this instance can accept pawns.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance can accept pawns; otherwise, <c>false</c>.
		/// </value>
		public bool CanAcceptPawns => WaitingForPawn && HasPower && HasFuel;


		private ColorInt Clear { get; } = new ColorInt(0, 0, 0, 0);
		private ColorInt GlowColor { get; } = new ColorInt(0, 255, 0, 255);

		[NotNull]
		private IReadOnlyList<MutationDef> AnimalMutations => _targetAnimal?.GetAllMutationsFrom() ?? Array.Empty<MutationDef>();

		private bool HasPower => PowerCompTrader.PowerOn;

		private CompPowerTrader PowerCompTrader
		{
			get
			{
				if (_power == null) _power = GetComp<CompPowerTrader>();

				return _power;
			}
		}

		[CanBeNull]
		private CompGlower Glower
		{
			get
			{
				if (_glower == null) _glower = GetComp<CompGlower>();

				return _glower;
			}
		}

		[NotNull]
		private Gizmo DebugFinishGizmo
		{
			get
			{
				if (_debugFinishGizmo == null)
					_debugFinishGizmo = new Command_Action
					{
						defaultLabel = "Debug Finish Chamber",
						action = DebugFinishChamber
					};

				return _debugFinishGizmo;
			}
		}

		[NotNull]
		private CompRefuelable Refuelable
		{
			get
			{
				if (_refuelable == null) _refuelable = GetComp<CompRefuelable>();
				if (_refuelable == null) Log.ErrorOnce("unable to find refuelable comp on mutachamber!", thingIDNumber);

				return _refuelable;
			}
		}

		private bool Occupied => innerContainer.Any;

		private CompFlickable Flickable
		{
			get
			{
				if (_flickable == null) _flickable = GetComp<CompFlickable>();

				return _flickable;
			}
		}

		[CanBeNull]
		private FillableBarDrawer FillableDrawer
		{
			get
			{
				if (_fillableDrawer == null) _fillableDrawer = GetComp<FillableBarDrawer>();

				return _fillableDrawer;
			}
		}

		private bool HasFuel => Refuelable.HasFuel;

		[NotNull]
		private Gizmo PartPickerGizmo
		{
			get
			{
				if (_ppGizmo == null)
					_ppGizmo = new Command_Action
					{
						action = OpenPartPicker,
						icon = PMTextures.PartPickerIcon,
						defaultLabel = PART_PICKER_GIZMO_LABEL.Translate(),
						defaultDesc = PART_PICKER_GIZMO_DESC.Translate()
					};

				return _ppGizmo;
			}
		}

		private Gizmo MergingGizmo
		{
			get
			{
				if (_mergingGizmo == null)
					_mergingGizmo = new Command_Action
					{
						icon = PMTextures.MergingIcon,
						action = EnterMergingIdle,
						defaultLabel = MERGING_GIZMO_LABEL.Translate(),
						defaultDesc = MERGING_GIZMO_DESC.Translate()
					};

				return _mergingGizmo;
			}
		}


		[NotNull]
		private static List<PawnKindDef> RandomAnimalCache
		{
			get
			{
				if (_randomAnimalCache == null)
					_randomAnimalCache = DefDatabase<PawnKindDef>
										.AllDefsListForReading.Where(p => p.race.IsValidAnimal())
										.ToList();

				return _randomAnimalCache;
			}
		}

		[NotNull]
		private AnimalSelectorComp SelectorComp
		{
			get
			{
				if (_aSelector == null) _aSelector = GetComp<AnimalSelectorComp>();
				if (_aSelector == null) Log.ErrorOnce("unable to find animal selector on mutachamber!", thingIDNumber);

				return _aSelector;
			}
		}

		private float PercentDone => 1f - (float)_timer / _lastTotal;

		/// <summary>
		///     Draws this instance.
		/// </summary>
		protected override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			FillableDrawer?.PreDraw();
			base.DrawAt(drawLoc, flip);
		}


		/// <summary>
		///     Ejects the contents.
		/// </summary>
		public override void EjectContents()
		{
			base.EjectContents();
			ResetChamber();
		}

		/// <summary>
		///     exposes data for serialization/deserialization
		/// </summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref _currentUse, "currentUse");
			Scribe_Values.Look(ref _innerState, "state");
			Scribe_Values.Look(ref _timer, "timer", -1);
			Scribe_Values.Look(ref _lastTotal, "lastTotal");
			Scribe_Defs.Look(ref _targetAnimal, "targetAnimal");
			Scribe_Defs.Look(ref _specialThing, "specialThing");
			Scribe_Defs.Look(ref _lastTfRequest, "lastTfRequest");
			Scribe_Deep.Look(ref _addedMutationData, "addedMutationData");
			Scribe_Values.Look(ref _curMutationIndex, "curMutationIndex");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{

				if (Glower != null) Glower.Props.glowColor = _innerState == ChamberState.Active ? GlowColor : Clear;

				SelectorComp.OnClick += SelectorComp_OnClick;
			}
		}

		private void SelectorComp_OnClick([NotNull] AnimalSelectorComp comp)
		{
			Pawn pawn = innerContainer.OfType<Pawn>().FirstOrDefault();
			if (pawn != null)
				comp.SpeciesFilter = (x) => x.GetModExtension<ChamberAnimalTfController>()?.CanInitiateTransformation(pawn, x, this) ?? true;
		}

		// ThingDefs with classes derived from MutaChamber
		private static IList<ThingDef> chamberDefs;

		/// <summary>
		///     Finds the Mutachamber casket for.
		/// </summary>
		/// <param name="p">The p.</param>
		/// <param name="traveler">The traveler.</param>
		/// <param name="ignoreOtherReservations">if set to <c>true</c> [ignore other reservations].</param>
		/// <param name="use">The use.</param>
		/// <returns></returns>
		public static MutaChamber FindMutaChamberFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false, ValueTuple<ChamberUse, ChamberUse>? use = null)
		{
			if (chamberDefs == null)
				chamberDefs = DefDatabase<ThingDef>.AllDefs.Where(x => typeof(MutaChamber).IsAssignableFrom(x.thingClass)).ToList();


			bool Validator(Thing x)
			{
				var mutaChamber = (MutaChamber)x;
				ChamberUse currentUse = mutaChamber.CurrentUse;
				if ((use == null || use.Value.Item1 == currentUse || use.Value.Item2 == currentUse) 
					&& mutaChamber.CanAcceptPawns
					&& mutaChamber.Flickable.SwitchIsOn)
				{
					return traveler.CanReserve(x, 1, -1, null, ignoreOtherReservations);
				}
				return false;
			}

			// For each type of chamber defs.
			for (int i = 0; i < chamberDefs.Count; i++)
			{
				ThingDef def = chamberDefs[i];

				var chambers = p.Map.listerThings.ThingsOfDef(def);
				int chambersCount = chambers.Count;
				if (chambersCount == 0)
					continue;

				bool anyTargets = false;
				for (int chamberIndex = chambers.Count - 1; chamberIndex >= 0; chamberIndex--)
				{
					// Check if any chambers are available before doing pathing.
					if (Validator(chambers[chamberIndex]))
					{
						anyTargets = true;
						break;
					}
				}
				if (anyTargets == false)
					continue;

				MutaChamber building_MutagenChamber = GenClosest.ClosestThing_Global_Reachable(p.Position, p.Map, chambers,
																   PathEndMode.InteractionCell, TraverseParms.For(traveler),
																   9999f, Validator) as MutaChamber;

				if (building_MutagenChamber != null) 
					return building_MutagenChamber;
			}

			return null;
		}

		[CanBeNull]
		ChamberAnimalTfController GetCurrentTfController()
		{
			var ext = _lastTfRequest?.GetModExtension<ChamberAnimalTfController>();
			return ext;
		}

		/// <summary>
		///     Gets the float menu options.
		/// </summary>
		/// <param name="myPawn">My pawn.</param>
		/// <returns></returns>
		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
		{
			foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(myPawn)) yield return floatMenuOption;
			if (!MutagenDefOf.MergeMutagen.CanTransform(myPawn))
				yield break;

			if (WaitingForPawn)
			{
				if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
				{
					yield return new FloatMenuOption("CannotUseNoPath".Translate(), null);
					yield break;
				}

				JobDef jobDef = PMJobDefOf.EnterMutagenChamber;
				string jobStr;

				switch (_currentUse)
				{
					case ChamberUse.Mutation:

					case ChamberUse.Tf:
						jobStr = "EnterMutagenChamber".Translate();
						break;
					case ChamberUse.Merge:
						jobStr = "PMEnterMutagenChamberMerge".Translate();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				Action jobAction = delegate
				{
					var job = new Job(jobDef, this);
					myPawn.jobs.TryTakeOrderedJob(job);
				};
				yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction), myPawn, this);
			}
		}




		/// <summary>
		///     Gets the gizmos.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos()) yield return gizmo;

			if (DebugSettings.godMode && (_innerState == ChamberState.Active || _innerState == ChamberState.WaitingForSpecialThing)) yield return DebugFinishGizmo;


			if (_innerState != ChamberState.Idle) yield break;

			if ((innerContainer[0] as Pawn).def is ThingDef_AlienRace)
				yield return PartPickerGizmo;

			yield return MergingGizmo;
		}

		/// <summary>
		///     Gets the inspect string.
		/// </summary>
		/// <returns></returns>
		public override string GetInspectString()
		{
			base.GetInspectString();
			var stringBuilder = new StringBuilder();
			string inspectString = base.GetInspectString();
			if (_innerState != ChamberState.WaitingForSpecialThing && _specialThing != null)
				stringBuilder.Append(("PM" + _innerState).Translate() + " ");
			else
				stringBuilder.AppendLine(("PM" + _innerState).Translate(_specialThing) + " ");
			stringBuilder.AppendLine(inspectString);

			if (_innerState == ChamberState.Active)
			{
				float pDone = 1f - (float)_timer / _lastTotal;
				string insString = "MutagenChamberProgress".Translate() + ": " + pDone.ToStringPercent() + " ";
				insString = GetPawnInspectionString(insString);

				stringBuilder.AppendLine(insString);
			}


			return stringBuilder.ToString().TrimEndNewlines();
		}

		/// <summary>
		///     setup after spawning in
		/// </summary>
		/// <param name="map"></param>
		/// <param name="respawningAfterLoad"></param>
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);

			if (!_initialized)
			{
				Initialize();
				_initialized = true;
			}

			LessonAutoActivator.TeachOpportunity(PMConceptDefOf.MergingPawns, OpportunityType.Important);
			LessonAutoActivator.TeachOpportunity(PMConceptDefOf.PM_PartPicker, OpportunityType.Important);
			LessonAutoActivator.TeachOpportunity(PMConceptDefOf.Tagging, OpportunityType.Important);
		}


		/// <summary>
		///     Ticks this instance.
		/// </summary>
		public override void Tick()
		{
			base.Tick();

			if (!Refuelable.HasFuel) return;

			if (_innerState != ChamberState.Active)
				return;

			if (_timer <= 0)
			{
				try
				{
					EjectPawn();
				}
				catch (Exception e)
				{
					Log.Error($"unable to eject pawns from chamber!\ncaught exception {e.GetType().Name}\n{e}");
					ResetChamber();
				} //make sure an exception while ejecting a pawn doesn't put the chamber in a bad state 
				return;
			}

			// If it no longer contains a pawn (pawn probably died) then eject whatever and reset.
			for (int i = 0; i < innerContainer.Count; i++)
			{
				if (innerContainer[i] is Pawn == false)
				{
					try
					{
						EjectContents();
					}
					catch (Exception e)
					{
						Log.Error($"unable to eject content from chamber!\ncaught exception {e.GetType().Name}\n{e}");
						ResetChamber();
					}
					return;
				}
			}

			if (PowerCompTrader?.PowerOn == false)
				return;

			if (!Flickable.SwitchIsOn)
				return;

			Refuelable.Notify_UsedThisTick();


			_timer -= 1;
			Pawn pawn = (Pawn)innerContainer[0];
			switch (_currentUse)
			{
				case ChamberUse.Mutation:
					CheckMutationProgress(pawn);
					break;

				case ChamberUse.Tf:
					CheckTfMutationProgress(pawn);
					break;

				case ChamberUse.Merge:
				default:
					break;
			}
		}

		/// <summary>
		///     Tries the accept special thing.
		/// </summary>
		/// <param name="deliveredThing">The delivered thing.</param>
		public void TryAcceptSpecialThing([NotNull] Thing deliveredThing)
		{
			if (_innerState != ChamberState.WaitingForSpecialThing)
			{
				Log.Error($"chamber was not waiting on anything but got {deliveredThing.Label}");
				return;
			}
			if (deliveredThing.def != _specialThing)
				Log.Error($"Chamber expected {_specialThing?.defName} but got {deliveredThing.Label}");

			_innerState = ChamberState.Active;
			SetActive();
			deliveredThing.Destroy();

		}


		/// <summary>
		/// Gets a value indicating whether this instance is waiting on a special thing to start a transformation.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is waiting on a special thing; otherwise, <c>false</c>.
		/// </value>
		public bool WaitingOnSpecialThing => _innerState == ChamberState.WaitingForSpecialThing;


		/// <summary>
		/// Gets the special thing needed.
		/// </summary>
		/// <value>
		/// The special thing needed.
		/// </value>
		public ThingDef SpecialThingNeeded => _specialThing;

		/// <summary>
		///     tries to accept a new thing into this chamber
		/// </summary>
		/// <param name="thing"></param>
		/// <param name="allowSpecialEffects"></param>
		/// <returns></returns>
		public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
		{
			if (base.TryAcceptThing(thing, allowSpecialEffects))
			{
				var p = thing as Pawn;
				if (p == null)
				{
					Log.Error($"{ThingID} accepted non pawn {p.ThingID}/{p.GetType().Name}! this should never happen");
					return true;
				}

				Need_Food food = p.needs?.food;
				if (food != null) food.CurLevel = food.MaxLevel;

				// ReSharper disable once SwitchStatementMissingSomeCases
				switch (_innerState)
				{
					case ChamberState.WaitingForPawn:
						_innerState = ChamberState.Idle;
						SelectorComp.Enabled = true;
						break;
					case ChamberState.WaitingForPawnMerging:
						_innerState = ChamberState.Active;
						SetActive();
						StartMerging();
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(_innerState), _innerState.ToString());
				}

				return true;
			}

			return false;
		}

		private void AnimalChosen(PawnKindDef pawnkinddef)
		{
			if (pawnkinddef == null)
				return;

			_lastTfRequest = pawnkinddef;
			_currentUse = ChamberUse.Tf;

			var controller = GetCurrentTfController();
			if (controller != null)
			{
				var pawn = (Pawn)innerContainer.First();
				var report = controller.CanInitiateTransformation(pawn, pawnkinddef, this);
				if (!report)
				{
					Messages.Message(report.reason, this, MessageTypeDefOf.RejectInput);
					return;
				}

				var start = controller.InitiateTransformation(pawn, pawnkinddef, this);
				_lastTotal = GetTransformationTime(start.duration);
				_timer = _lastTotal;
				_targetAnimal = start.pawnkindDef;
				_specialThing = start.specialResource;


				if (_specialThing != null)
				{
					_innerState = ChamberState.WaitingForSpecialThing;
				}
				else
				{
					_innerState = ChamberState.Active;
					SetActive();
				}

				SelectorComp.Enabled = false;
				return;
			}


			_timer = GetTransformationTime(pawnkinddef);
			_lastTotal = _timer;
			_innerState = ChamberState.Active;
			_currentUse = ChamberUse.Tf;
			_targetAnimal = pawnkinddef;




			SetActive();

			SelectorComp.Enabled = false;
		}

		private void ApplyMutationData([NotNull] Pawn pawn, [NotNull] IReadOnlyMutationData mutationData)
		{
			if (mutationData.Part != null)
				if (mutationData.Part.IsMissingAtAllIn(pawn))
					return;


			mutationData.ApplyMutationData(pawn, MutationUtilities.AncillaryMutationEffects.HistoryOnly);
			UpdatePawnVisuals(pawn);
		}

		private void UpdatePawnVisuals([NotNull] Pawn pawn)
		{
			var comp = pawn.TryGetComp<GraphicSys.GraphicsUpdaterComp>();
			if (comp == null)
				return;

			comp.RefreshGraphics();
		}

		private void CheckMutationProgress([NotNull] Pawn pawn)
		{
			if (_addedMutationData == null)
				return;

			int mx = _addedMutationData.Count;
			if (mx == 0)
				return;

			int idx = Mathf.FloorToInt(Mathf.Clamp(PercentDone * mx, 0, mx - 1));
			if (idx != _curMutationIndex)
			{
				_curMutationIndex = idx;
				IReadOnlyMutationData mutationData = _addedMutationData[_curMutationIndex];
				ApplyMutationData(pawn, mutationData);
			}
		}

		private void CheckTfMutationProgress([NotNull] Pawn pawn)
		{
			if (AnimalMutations.Count == 0) return;

			int mx = AnimalMutations.Count - 1;
			int idx = Mathf.FloorToInt(Mathf.Clamp(PercentDone * mx, 0, mx));
			if (idx != _curMutationIndex)
			{
				_curMutationIndex = idx;
				MutationDef mut = AnimalMutations[idx];
				MutationResult muts =
					MutationUtilities.AddMutation(pawn, mut, ancillaryEffects: MutationUtilities.AncillaryMutationEffects.None);
				foreach (Hediff_AddedMutation hediffAddedMutation in muts)
				{
					Comp_MutationSeverityAdjust adjComp = hediffAddedMutation.SeverityAdjust;
					if (adjComp != null) hediffAddedMutation.Severity = adjComp.NaturalSeverityLimit;
				}
			}
		}


		[DebugOutput(category = "Pawnmorpher")]
		private static void DBGGetAnimalTransformationTimes()
		{
			var builder = new StringBuilder();
			foreach (PawnKindDef pawnKindDef in DefDatabase<PawnKindDef>.AllDefsListForReading)
			{
				float size = pawnKindDef.RaceProps.baseBodySize;

				builder.AppendLine($"{pawnKindDef.defName},{size}");
			}

			Log.Message(builder.ToString());
		}

		private void DebugFinishChamber()
		{
			_innerState = ChamberState.Active;
			_timer = 0;
		}

		private void EjectPawn()
		{
			_scratchList.Clear();
			_scratchList.AddRange(innerContainer.OfType<Pawn>());
			Pawn pawn = _scratchList[0];
			if (pawn == null)
			{
				Log.Error("trying to eject empty muta chamber!");
				return;
			}

			TransformationRequest tfRequest;
			Mutagen mutagen = null;
			SetInactive();
			switch (_currentUse)
			{
				case ChamberUse.Mutation:
					tfRequest = null;
					FinalizeMutations(pawn);
					break;
				case ChamberUse.Merge:

					Pawn otherPawn = _scratchList[1];
					if (otherPawn == null)
					{
						Log.Error("merging but cannot find other pawn! aborting!");
						tfRequest = null;
						break;
					}

					SpawnSilkFor(pawn.apparel);
					SpawnSilkFor(otherPawn.apparel);


					tfRequest = new TransformationRequest(_targetAnimal, pawn, otherPawn)
					{
						addMutationToOriginal = false,
						factionResponsible = Faction,
						forcedFaction = Faction,
						forcedSapienceLevel = 1,
						manhunterSettingsOverride = ManhunterTfSettings.Never
					};
					mutagen = MutagenDefOf.MergeMutagen.MutagenCached;
					break;
				case ChamberUse.Tf:
					SpawnSilkFor(pawn.apparel);
					PawnKindDef animal = SelectorComp.ChosenKind;
					if (animal == null) animal = GetRandomAnimal();

					tfRequest = new TransformationRequest(animal, pawn)
					{
						addMutationToOriginal = true,
						factionResponsible = Faction,
						forcedFaction = Faction,
						forcedGender = TFGender.Original,
						forcedSapienceLevel = 1,
						manhunterSettingsOverride = ManhunterTfSettings.Never
					};
					mutagen = MutagenDefOf.PM_ChamberMutagen.MutagenCached;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			EjectContents();

			if (tfRequest == null) return;


			TransformedPawn tfPawn = mutagen.Transform(tfRequest);

			if (tfPawn == null)
			{
				Log.Error($"unable to transform pawn(s) with {mutagen.def.defName}! {_currentUse} {_innerState}");
				return;
			}

			var gComp = Find.World.GetComponent<PawnmorphGameComp>();
			gComp.AddTransformedPawn(tfPawn);
			foreach (Pawn oPawn in tfPawn.OriginalPawns)
				if (oPawn.Spawned)
					oPawn.DeSpawn();


			if (_currentUse == ChamberUse.Tf)
			{
				var oPawn = tfPawn.OriginalPawns.FirstOrDefault();
				if (oPawn == null) return;
				var controller = _lastTfRequest?.GetModExtension<ChamberAnimalTfController>();
				if (controller != null)
				{
					controller.OnPawnEjects(oPawn, tfPawn.TransformedPawns.FirstOrDefault(), this);
				}
			}
		}

		private void EnterMergingIdle()
		{
			_innerState = ChamberState.WaitingForPawnMerging;
			_currentUse = ChamberUse.Merge;
			SelectorComp.Enabled = false;
		}

		private void FinalizeMutations([NotNull] Pawn pawn)
		{
			if (_addedMutationData == null) return;

			for (int i = _curMutationIndex + 1; i < _addedMutationData.Count; i++)
			{
				IReadOnlyMutationData mut = _addedMutationData[i];
				ApplyMutationData(pawn, mut);
			}
		}

		private int GetMutasilkAmountFrom(Apparel apparel)
		{
			int matAmount = apparel.def.costList?.Select(s => s.count).Sum() ?? 10;
			int amt = Mathf.RoundToInt(matAmount * (float)apparel.HitPoints * 0.05f);
			return Mathf.Min(amt, 75);
		}

		private int GetMutationDuration(IReadOnlyAddedMutations addedMutations)
		{
			const float averageValue = 10;
			const float averageMaxPossible = 30;
			float maxTime = 60000 * TF_ANIMAL_DURATION;
			float minTime = 60000 * TF_ANIMAL_DURATION / 10;
			float tValue = 0;

			foreach (IReadOnlyMutationData mutation in addedMutations) tValue += mutation.Mutation.value;

			tValue /= averageValue * averageMaxPossible;

			float t = Mathf.Clamp(tValue * maxTime, minTime, maxTime);
			return Mathf.RoundToInt(t);
		}

		private string GetPawnInspectionString(string insString)
		{
			var pawn = (Pawn)innerContainer.First();
			switch (_currentUse)
			{
				case ChamberUse.Mutation:
					insString += "PMChamberAddingMutations".Translate();
					break;
				case ChamberUse.Merge:
				case ChamberUse.Tf:
					insString += "PMChamberTransforming".Translate(pawn, _targetAnimal);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return insString;
		}

		private PawnKindDef GetRandomAnimal()
		{
			return RandomAnimalCache.RandomElement();
		}


		private int GetTransformationTime(PawnKindDef pawnKindDef)
		{
			float baseTime = TF_ANIMAL_DURATION * 60000; //convert from days to ticks 
			float szFactor =
				Mathf.Pow(pawnKindDef.RaceProps.baseBodySize, 1f / 3); //want to reduce the time of extreme body size values 
			float time = baseTime * szFactor;
			return Mathf.RoundToInt(Mathf.Max(time, MIN_TRANSFORMATION_TIME));
		}


		int GetTransformationTime(float days)
		{
			return Mathf.RoundToInt(days * 60000);
		}

		private void Initialize()
		{
			if (Glower != null)
				UpdateGlow(Glower, Map, false);

			SelectorComp.Enabled = Occupied && _innerState == ChamberState.Idle;
			if (_innerState == ChamberState.Active && _timer == -1)
			{
				Log.Error("timer on mutachamber is -1 while active, ending");
				_timer = 0;
			}

			SelectorComp.AnimalChosen += AnimalChosen;
		}

		private void OpenPartPicker()
		{
			var pawn = innerContainer.First() as Pawn;
			if (pawn == null) Log.Error("unable to find pawn to open part picker for");

			var dialogue = new Dialog_PartPicker(pawn);

			dialogue.WindowClosed += WindowClosed;
			Find.WindowStack.Add(dialogue);
		}

		private void ResetChamber()
		{
			FillableDrawer?.Clear();
			SelectorComp.Enabled = false;
			SelectorComp.ResetSelection();
			_innerState = ChamberState.WaitingForPawn;
			_currentUse = ChamberUse.Tf;
			_addedMutationData = null;
			_curMutationIndex = -1;
			_timer = 0;

			if (Glower != null)
				UpdateGlow(Glower, Map, false);
		}

		private void SetActive()
		{
			FillableDrawer?.Trigger();

			PowerCompTrader.PowerOn = true;
			if (Glower != null)
			{
				Glower.Props.glowColor = GlowColor;
				UpdateGlow(Glower, Map, true);
			}
		}

		private void SetInactive()
		{
			PowerCompTrader.PowerOn = false;
			if (Glower != null)
			{
				Glower.Props.glowColor = Clear;
				UpdateGlow(Glower, Map, false);
			}
		}

		private void SpawnSilkFor([CanBeNull] Pawn_ApparelTracker apparel)
		{
			if (apparel == null) return;


			var count = 0;
			foreach (Apparel app in apparel.WornApparel)
			{
				if (app == null) continue;
				count += GetMutasilkAmountFrom(app);
			}

			// Don't try to spawn anything if count is 0.
			if (count > 0)
			{
				Thing silk = ThingMaker.MakeThing(PMThingDefOf.Morphsilk);
				silk.stackCount = count;
				GenPlace.TryPlaceThing(silk, Position, Map, ThingPlaceMode.Near);
			}

			apparel.DestroyAll();
		}

		private void StartMerging()
		{
			ThingDef merge = ChaomorphUtilities.GetRandomChaomorph(ChaomorphType.Merge);
			if (merge == null)
			{
				Log.Error("unable to get random merge, instead generating chaomorph!");
				merge = ChaomorphUtilities.GetRandomChaomorph(ChaomorphType.Chaomorph);
			}

			_targetAnimal = DefDatabase<PawnKindDef>.AllDefs.FirstOrDefault(pk => pk.race == merge);
			if (_targetAnimal == null)
			{
				Log.Error($"unable to find pawnkind def for {merge.defName}! aborting!");
				EjectContents();
				return;
			}

			_timer = GetTransformationTime(_targetAnimal);
			_lastTotal = _timer;
			_innerState = ChamberState.Active;
			_currentUse = ChamberUse.Merge;
			SelectorComp.Enabled = false;
		}

		private void WindowClosed(Dialog_PartPicker sender, IReadOnlyAddedMutations addedmutations)
		{
			sender.WindowClosed -= WindowClosed;

			if (_innerState != ChamberState.Idle)
			{
				Log.Message("state is not idle!");

				return;
			}

			var pawn = innerContainer.FirstOrDefault() as Pawn;
			if (pawn == null)
				return;

			if (addedmutations?.Any() != true)
			{
				UpdatePawnVisuals(pawn);
				return;
			}


			_addedMutationData = new AddedMutations(addedmutations);
			_timer = GetMutationDuration(addedmutations);
			_currentUse = ChamberUse.Mutation;
			_innerState = ChamberState.Active;
			SetActive();
			_lastTotal = _timer;
			sender.Reset();

			SelectorComp.Enabled = false;
		}

		private void UpdateGlow(CompGlower glowerComp, Map map, bool lit)
		{
			map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things);

			if (lit)
				map.glowGrid.RegisterGlower(glowerComp);
			else
				map.glowGrid.DeRegisterGlower(glowerComp);

		}

		private enum ChamberState
		{
			WaitingForPawn,
			WaitingForPawnMerging,
			Idle,
			Active,
			WaitingForSpecialThing
		}
	}
}