// Driver_ResurrectTf.cs modified by Iron Wolf for Pawnmorph on 11/02/2019 11:29 AM
// last updated 11/02/2019  11:29 AM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.TfSys;
using Pawnmorph.ThingComps;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{

	/// <summary>
	/// job driver for the tf resurrector 
	/// </summary>
	/// <seealso cref="Verse.AI.JobDriver" />
	public class Driver_ResurrectTf : JobDriver
	{
		private const TargetIndex CorpseInd = TargetIndex.A;

		private const TargetIndex ItemInd = TargetIndex.B;

		private const int DurationTicks = 600;

		private Corpse Corpse => (Corpse)job.GetTarget(TargetIndex.A).Thing;

		private Thing Item => job.GetTarget(TargetIndex.B).Thing;


		CompProperties_TfResurrect ThingProps => Item.def.GetCompProperties<CompProperties_TfResurrect>();


		private ResurrectorTargetComp TargetSettings => Item.TryGetComp<ResurrectorTargetComp>();

		/// <summary>
		/// Tries the make pre toil reservations.
		/// </summary>
		/// <param name="errorOnFailed">if set to <c>true</c> [error on failed].</param>
		/// <returns></returns>
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = this.pawn; //just copied from base games resurrector 
			LocalTargetInfo target = this.Corpse;
			Job job = this.job;
			bool arg_58_0;
			if (pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
			{
				pawn = this.pawn;
				target = this.Item;
				job = this.job;
				arg_58_0 = pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
			}
			else
			{
				arg_58_0 = false;
			}
			return arg_58_0;
		}

		/// <summary>
		/// Makes the new toils.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
			Toil prepare = Toils_General.Wait(600, TargetIndex.None);
			prepare.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			prepare.FailOnDespawnedOrNull(TargetIndex.A);
			prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			yield return prepare;
			yield return Toils_General.Do(Resurrect);
		}

		private const string RESURRECTION_MESSAGE_LABEL = "MessagePawnResurrectedTransformed";

		private void Resurrect()
		{
			Pawn innerPawn = this.Corpse.InnerPawn;
			ResurrectionUtility.TryResurrect(innerPawn); //make sure pawn is alive again 

			var mutagen = MutagenDefOf.defaultMutagen;
			PawnKindDef animalKind;
			TransformationRequest request;
			request = MakeRequest(innerPawn);
			animalKind = request.outputDef;
			var tfPawn = mutagen.MutagenCached.Transform(request);

			if (tfPawn != null)
			{
				var comp = Find.World.GetComponent<PawnmorphGameComp>();
				var oFirst = tfPawn.TransformedPawns.First();
				comp.AddTransformedPawn(tfPawn);
				var messageContent =
					RESURRECTION_MESSAGE_LABEL.Translate(innerPawn.Named("original"), oFirst.Named("animal"),
														 animalKind.Named(nameof(animalKind)))
											  .CapitalizeFirst();
				Messages.Message(messageContent, oFirst, MessageTypeDefOf.PositiveEvent, true);
			}
			else
			{
				Log.Warning($"resurrected pawn {pawn.Name} who cannot be transformed normally! is this intended?");
			}



			//Messages.Message("MessagePawnResurrected".Translate(innerPawn).CapitalizeFirst(), innerPawn, MessageTypeDefOf.PositiveEvent, true);
			this.Item.SplitOff(1).Destroy(DestroyMode.Vanish);
		}

		private TransformationRequest MakeRequest(Pawn innerPawn)
		{
			var animalKind = GetAnimalFor(innerPawn);
			var request = new TransformationRequest(animalKind, innerPawn, TargetSettings?.ForcedSapienceLevel ?? SapienceLevel.Sapient)
			{
				cause = null,
				forcedGender = ThingProps.genderTf,
				tale = ThingProps.taleDef,
			};
			return request;
		}

		private PawnKindDef GetAnimalFor([NotNull] Pawn innerPawn)
		{
			var target = TargetSettings;
			if (target == null)
			{
				Log.Warning($"{Item.def.defName} does not have a {nameof(ResurrectorTargetComp)}!");
				return DefDatabase<PawnKindDef>.AllDefs.Where(pk => pk.RaceProps.Animal).RandomElement();
			}

			return target.GetValidAnimalFor(innerPawn);

		}
	}
}