using JetBrains.Annotations;
using RimWorld;

namespace Pawnmorph
{
	/// <summary> Static container for HistoryEventDef (event system for percepts). </summary>
	[DefOf]
	public static class PMHistoryEventDefOf
	{
		static PMHistoryEventDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PMHistoryEventDefOf));
		}
		//Notes: 
		//Doer: should refer to the pawn who the even is about
		//Subject should refer to a thing the Doer sees or does something about 


		/// <summary>
		/// Whenever a mutation is gained
		/// </summary>
		/// args:
		/// Doer(pawn)
		/// Mutation(Mutation) 
		[NotNull]
		public static HistoryEventDef MutationGained;

		/// <summary> Whenever a mutation is lost </summary>
		/// args
		/// Doer(Pawn)
		/// Mutation(Mutation)
		[NotNull]
		public static HistoryEventDef MutationLost;

		/// <summary> Whenever a pawn becomes a former human </summary>
		/// args:
		/// Doer(pawn)
		/// Animal(PawnkindDef)
		/// FactionResponsible(Faction) can be null
		/// Source(MutagenDef) the mutagen that caused the transformation 
		[NotNull]
		public static HistoryEventDef Transformed;

		/// <summary> Whenever a pawn is no longer a former human </summary>
		/// args:
		/// Doer(pawn)
		/// Animal(PawnKindDef)
		/// FactionResponsible(Faction) can be null
		/// Source(MutagenDef) the mutagen that caused the reversion 
		[NotNull]
		public static HistoryEventDef Reverted;

		/// <summary>
		/// Whenever a pawn is transformed into a morph
		/// </summary>
		/// args:
		/// Doer(pawn)
		/// OldMorph(MorphDef) can be null
		/// NewMorph(MorphDef)
		[NotNull]
		public static HistoryEventDef Morphed;

		/// <summary>
		/// Whenever a pawn is reverted
		/// </summary>
		/// args:
		/// Doer(pawn)
		/// Morph(morphDef)
		/// FactionResponsible(Faction) can be null
		[NotNull]
		public static HistoryEventDef DeMorphed;

		/// <summary> Whenever sapience level changes </summary>
		/// args:
		/// Doer(pawn)
		/// OldSapienceLevel(SapienceLevel)
		/// NewSapienceLevel(SapienceLevel)
		[NotNull]
		public static HistoryEventDef SapienceLevelChanged;

		/// <summary>
		/// when a former human goes permanently feral
		/// </summary>
		/// args: 
		/// Doer(pawn) the pawn that went permanently feral 
		[NotNull]
		public static HistoryEventDef PermanentlyFeral;


		/// <summary>
		/// event for when a former human hunts another animal either due to hunger or as a mental break 
		/// </summary>
		/// args:
		/// Doer(pawn) the pawn that hunted
		/// VICTIM(pawn) what the pawn hunted 
		[NotNull]
		public static HistoryEventDef FormerHumanHunted;

		/// <summary>
		/// event for when a former human grazes (eats either live plants, seeds or trees)
		/// </summary>
		/// args:
		/// Doer(pawn) the pawn that grazed
		/// VICTIM(Thing) the thing the pawn ate 
		[NotNull]
		public static HistoryEventDef FormerHumanGrazed;

		/// <summary>
		/// event for when a former human eats a raw corpse 
		/// </summary>
		/// args:
		/// Doer(pawn) 
		public static HistoryEventDef FormerHumanAteCorpse;


		/// <summary>
		/// history event for when a pawn applies mutagenics on another pawn
		/// </summary>
		/// args:
		/// Doer(pawn): the pawn applying the mutagenics 
		/// Victim(pawn): the pawn they are being applied on 
		[NotNull]
		public static HistoryEventDef ApplyMutagenicsOn;


		/// <summary>
		/// event for when a pawn sows mutagenic plants
		/// </summary>
		/// args:
		/// Doer(pawn): the pawn sowing
		/// SUBJECT (thingDef): the plant def being sowed 
		[NotNull] public static HistoryEventDef SowMutagenicPlants;

		/// <summary>
		/// event for when a pawn creates 
		/// </summary>
		/// args:
		/// Doer: the pawn making the weapon
		/// SUBJECT (thingDef): the weapon def being made 
		[NotNull] public static HistoryEventDef CreateMutagenicWeapon;



	}
}
