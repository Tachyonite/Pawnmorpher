// RaceShiftUtilities.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 7:34 PM
// last updated 08/02/2019  7:34 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AlienRace;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.GraphicSys;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Hybrids
{
	/// <summary>
	/// a collection of utilities around changing a pawn's race 
	/// </summary>
	[HotSwappable]
	public static class RaceShiftUtilities
	{
		class CompPropComparer : IEqualityComparer<CompProperties>
		{
			/// <summary>Determines whether the specified objects are equal.</summary>
			/// <param name="x">The first object of type <c>CompPropComparer</c> to compare.</param>
			/// <param name="y">The second object of type <c>CompPropComparer</c> to compare.</param>
			/// <returns>
			/// <see langword="true" /> if the specified objects are equal; otherwise, <see langword="false" />.</returns>
			public bool Equals(CompProperties x, CompProperties y)
			{
				if (ReferenceEquals(x, y)) return true;
				if (x == null || y == null) return false;

				//check if one or the other implements IEquatable 
				if (x is IEquatable<CompProperties> xP)
				{
					return xP.Equals(y);
				}

				if (y is IEquatable<CompProperties> yP)
				{
					return yP.Equals(x);
				}

				if (x.GetType() != y.GetType()) return false;
				if (x.GetType() == typeof(CompProperties))
				{
					return x.compClass == y.compClass;
				}

				//just return true here 
				//need someway to check this by reflection
				return true;
			}

			/// <summary>Returns a hash code for the specified object.</summary>
			/// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
			/// <returns>A hash code for the specified object.</returns>
			/// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj" /> is a reference type and <paramref name="obj" /> is <see langword="null" />.</exception>
			public int GetHashCode(CompProperties obj)
			{
				return obj.GetHashCode();
			}
		}

		[NotNull]

		static readonly IEqualityComparer<CompProperties> _comparer = new CompPropComparer();

		/// <summary>
		/// The race change message identifier (used in the keyed translation file)
		/// </summary>
		public const string RACE_CHANGE_MESSAGE_ID = "RaceChangeMessage";

		private const string RACE_REVERT_MESSAGE_ID = "HumanAgainMessage";
		// private static string RaceRevertLetterLabel => RACE_REVERT_LETTER + "Label";
		//private static string RaceRevertLetterContent => RACE_REVERT_LETTER + "Content";

		private static LetterDef RevertToHumanLetterDef => LetterDefOf.PositiveEvent;

		/// <summary>
		/// Determines whether this pawn is a morph hybrid 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///   <c>true</c> if the specified pawn is a morph hybrid; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">pawn</exception>
		public static bool IsMorphHybrid([NotNull] Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));

			return RaceGenerator.IsMorphRace(pawn.def);

		}

		[NotNull]
		private static readonly List<ThingComp> _rmCompCache = new List<ThingComp>();
		[NotNull]
		private static readonly List<CompProperties> _addCompCache = new List<CompProperties>();
		static void SetCompField([NotNull] Pawn pawn)
		{
			var field = typeof(ThingWithComps).GetField("comps", BindingFlags.NonPublic | BindingFlags.Instance);
			var comps = (List<ThingComp>)field.GetValue(pawn);
			if (comps == null)
			{
				comps = new List<ThingComp>();
				field.SetValue(pawn, comps);
			}
		}

		internal static void AddRemoveDynamicRaceComps([NotNull] Pawn pawn, [NotNull] ThingDef newRace)
		{
			SetCompField(pawn);
			var props = newRace.comps;
			_addCompCache.Clear();
			_rmCompCache.Clear();

			foreach (ThingComp comp in pawn.AllComps)
			{
				if (props?.Any(p => _comparer.Equals(p, comp.props)) != true)
				{
					_rmCompCache.Add(comp);
				}
			}

			foreach (CompProperties prop in props.MakeSafe())
			{
				if (!pawn.AllComps.Any(c => _comparer.Equals(c.props, prop)))
				{
					_addCompCache.Add(prop);
				}
			}


			foreach (ThingComp thingComp in _rmCompCache)
			{
				var pmComp = thingComp as IPMThingComp;
				pmComp?.PreRemove();
				pawn.AllComps.Remove(thingComp);
				pmComp?.PostRemove();
			}

			_rmCompCache.Clear();

			foreach (CompProperties newCompProp in _addCompCache)
			{
				try
				{
					var newComp = (ThingComp)Activator.CreateInstance(newCompProp.compClass);
					newComp.parent = pawn;
					newComp.props = newCompProp;
					var nPMComp = newComp as IPMThingComp;

					pawn.AllComps.Add(newComp);

					nPMComp?.Init();


				}
				catch (Exception e)
				{
					Log.Error($"caught {e.GetType().Name} while trying to add comp with props {newCompProp} to pawn {pawn.Name}!\n{e.ToString().Indented("|\t")}");
				}
			}


		}


		/// <summary>
		/// safely change the pawns race
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="race">The race.</param>
		/// <param name="reRollTraits">if race related traits should be reRolled</param>
		/// <exception cref="ArgumentNullException">pawn</exception>
		public static void ChangePawnRace([NotNull] Pawn pawn, [NotNull] ThingDef race, bool reRollTraits = false)
		{
			var graphicsComp = pawn.GetComp<InitialGraphicsComp>();
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			if (race == null) throw new ArgumentNullException(nameof(race));
			MorphDef oldMorph = pawn.def.GetMorphOfRace();
			ThingDef oldRace = pawn.def;


			var gUpdater = pawn.GetComp<GraphicsUpdaterComp>();
			gUpdater?.BeginUpdate();


			AspectTracker aTracker = pawn.GetAspectTracker();

			TransformerUtility.ScaleInjuriesToNewRace(pawn, race);

			//var pos = pawn.Position;
			Faction faction = pawn.Faction;
			Map map = pawn.Map;

			if (map != null)
				RegionListersUpdater.DeregisterInRegions(pawn, map);
			var removed = false;

			if (map?.listerThings != null)
				if (map.listerThings.Contains(pawn))
				{
					map.listerThings.Remove(pawn); //make sure to update the lister things or else dying will break 
					removed = true;
				}


			if (pawn.story.traits == null)
				pawn.story.traits = new TraitSet(pawn);

			pawn.def = race;

			if (removed && !map.listerThings.Contains(pawn))
				map.listerThings.Add(pawn);

			if (map != null)
				RegionListersUpdater.RegisterInRegions(pawn, map);


			//add the group hediff if applicable 
			MorphDef newMorph = RaceGenerator.GetMorphOfRace(race);



			AspectDef oldMorphAspectDef = oldMorph?.group?.aspectDef;
			AspectDef aspectDef = newMorph?.group?.aspectDef;
			if (aTracker != null)
			{
				// If source morph has the same group trait as target morph, then do nothing.
				if (oldMorphAspectDef != aspectDef)
				{
					if (oldMorphAspectDef != null)
						aTracker.Remove(oldMorphAspectDef);

					if (aspectDef != null)
						aTracker?.Add(aspectDef);
				}
			}

			if (map != null)
			{
				var comp = map.GetComponent<MorphTracker>();
				comp.NotifyPawnRaceChanged(pawn, oldMorph);
			}

			//always revert to human settings first so the race change is consistent 

			if (graphicsComp?.Scanned == true)
				graphicsComp.RestoreGraphics();


			// Convert age
			try
			{
				// If the new race has a significant difference in number of life stages, CurLifeStage will be null, and Notify_LifeStageStarted will throw an exception.
				// Circumvented by temporarily changing the program state to Entry, which will prevent the game from trying to call Notify_LifeStageStarted.
				if (pawn.ageTracker.CurLifeStage == null)
					Current.ProgramState = ProgramState.Entry;

				float currentConvertedAge = TransformerUtility.ConvertAge(pawn, race.race);
				pawn.ageTracker.AgeBiologicalTicks = (long)(currentConvertedAge * TimeMetrics.TICKS_PER_YEAR); // 3600000f ticks per year.;

				Current.ProgramState = ProgramState.Playing;
			}
			catch (Exception e)
			{
				Log.Error($"Error while converting age of pawn {pawn.LabelCap}: {e}");
			}

			// Update racial styles
			try
			{
				ValidateExplicitRaceChange(pawn, race, oldRace);
			}
			catch (Exception e)
			{
				Log.Error($"Error while updating styles for {pawn.LabelCap}: {e}");
			}


			if (newMorph?.raceSettings?.hairstyles?.Count > 0)
			{
				// If any morph-specific hairstyles then pick one at random.
				HairDef morphHair = newMorph.raceSettings.hairstyles.Where(x => CheckStyleGender(x, pawn.gender)).RandomElement();
				if (morphHair != null)
					pawn.story.hairDef = morphHair;
			}

			// Update race restrictions
			try
			{
				HandleRaceRestrictions(pawn, race);
			}
			catch (Exception e)
			{
				Log.Error($"Error while handling race restrictions for {pawn.LabelCap}: {e}");
			}


			//check if the body def changed and handle any apparel changes 
			try
			{
				if (oldRace.race.body != race.race.body)
				{
					ValidateApparelForChangedPawn(pawn, oldRace);

					MorphDef morph = oldMorph ?? newMorph;
					if (morph != null)
						FixHediffs(pawn, oldRace, morph);
				}
			}
			catch (Exception e)
			{
				Log.Error($"Error while updating body of {pawn.LabelCap}: {e}");
			}

			// Update skin color and if turning from explicit race to human then generate new human skin color.
			try
			{
				HandleGraphicsChanges(pawn, newMorph);
			}
			catch (Exception e)
			{
				Log.Error($"Error while updating skin color for {pawn.LabelCap}: {e}");
			}



			//no idea what HarmonyPatches.Patch.ChangeBodyType is for, not listed in pasterbin

			// If reverted to human then rescan graphics to fix base skin color if originally alien.
			if (race == ThingDefOf.Human && graphicsComp != null)
			{
				if (graphicsComp.ScannedRace == oldMorph.ExplicitHybridRace)
					graphicsComp.ScanGraphics();
			}

			foreach (Hediff_AddedMutation mutation in pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>())
			{
				mutation.ApplyVisualAdjustment();
			}

			// Roll traits
			try
			{
				if (reRollTraits && race is ThingDef_AlienRace alienDef)
					ReRollRaceTraits(pawn, alienDef);
			}
			catch (Exception e)
			{
				Log.Error($"Error while re-rolling race traits for {pawn.LabelCap}: {e}");
			}

			if (gUpdater != null)
				gUpdater.EndUpdate();
			else
				pawn.RefreshGraphics();


			//save location 
			if (Current.ProgramState == ProgramState.Playing)
				pawn.ExposeData();

			if (pawn.Faction != faction)
				pawn.SetFaction(faction);

			pawn.VerbTracker?.InitVerbsFromZero();

			foreach (IRaceChangeEventReceiver raceChangeEventReceiver in pawn.AllComps.OfType<IRaceChangeEventReceiver>())
			{
				raceChangeEventReceiver.OnRaceChange(oldRace);
			}
			map?.mapPawns.UpdateRegistryForPawn(pawn);
		}

		private static void HandleRaceRestrictions(Pawn pawn, ThingDef race)
		{
			RaceRestrictionSettings restrictionSettings = null;
			if (race is ThingDef_AlienRace alien)
				restrictionSettings = alien.alienRace.raceRestriction;

			if (restrictionSettings == null)
				return;

			ThingOwner inventory = pawn.inventory.GetDirectlyHeldThings();

			List<Apparel> apparel = pawn.apparel.WornApparel;
			for (int i = apparel.Count - 1; i >= 0; i--)
			{
				Apparel item = apparel[i];
				if (item.def != null)
				{
					if (RaceRestrictionSettings.CanWear(item.def, race) == false)
					{
						// If pawn cannot keep item equipped due to race restrictions, then try add it to pawn's inventory otherwise drop.
						if (inventory.TryAddOrTransfer(item) == false)
							pawn.apparel.TryDrop(item);
					}
				}

			}

			List<ThingWithComps> equipment = pawn.equipment.AllEquipmentListForReading;
			for (int i = equipment.Count - 1; i >= 0; i--)
			{
				ThingWithComps item = equipment[i];
				if (item.def != null)
				{
					if (RaceRestrictionSettings.CanEquip(item.def, race) == false)
					{
						// If pawn cannot keep item equipped due to race restrictions, then try add it to pawn's inventory otherwise drop.
						if (inventory.TryAddOrTransfer(item) == false)
							pawn.equipment.TryDropEquipment(item, out _, pawn.PositionHeld);
					}
				}
			}
		}

		private static bool CheckStyleGender(StyleItemDef style, Gender pawnGender)
		{
			switch (style.styleGender)
			{
				case StyleGender.Any:
				case StyleGender.MaleUsually:
				case StyleGender.FemaleUsually:
					return true;

				case StyleGender.Male:
					return pawnGender == Gender.Male;

				case StyleGender.Female:
					return pawnGender == Gender.Female;
			}

			return false;
		}

		[NotNull]
		private readonly static List<Apparel> _apparelCache = new List<Apparel>();

		private static void ValidateApparelForChangedPawn([NotNull] Pawn pawn, [NotNull] ThingDef oldRace)
		{
			Pawn_ApparelTracker apparel = pawn.apparel;
			if (apparel == null) return;

			_apparelCache.Clear();
			_apparelCache.AddRange(apparel.WornApparel.MakeSafe());


			foreach (Apparel ap in _apparelCache) //use a copy so we can remove them safely while iterating 
			{
				if (!ApparelUtility.HasPartsToWear(pawn, ap.def))
				{
					if (DebugLogUtils.ShouldLog(LogLevel.Messages))
						Log.Message($"removing {ap.Label}");

					if (apparel.TryDrop(ap))
					{

						apparel.Remove(ap);
					}
				}
			}
		}

		private static void ValidateExplicitRaceChange(Pawn pawn, ThingDef race, ThingDef oldRace)
		{
			if (oldRace is ThingDef_AlienRace oldARace)
			{
				if (race is ThingDef_AlienRace aRace)
				{
					// Handle if target race is single-gendered.
					float? maleProbability = aRace.alienRace?.generalSettings?.maleGenderProbability;
					if (maleProbability != null)
					{
						if (maleProbability == 0f)
							pawn.gender = Gender.Female;
						else if (maleProbability == 100f)
							pawn.gender = Gender.Male;
					}


					var alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();
					AccessTools.Field(typeof(AlienPartGenerator.AlienComp), "nodeProps").SetValue(alienComp, null);

					ValidateGraphicsPaths(pawn, oldARace, aRace);
					ValidateGenes(pawn, oldARace, aRace);
					HPatches.PawnPatches.QueueRaceCheck(pawn);
				} //validating the graphics paths only works for aliens 
				else
				{
					Log.Warning($"trying change the race of {pawn.Name} to {race.defName} which is not {nameof(ThingDef_AlienRace)}!");

				}
			}
			else
			{
				Log.Warning($"trying change the race of {pawn.Name} from {oldRace.defName} which is not {nameof(ThingDef_AlienRace)}!");
			}
		}


		private static void ValidateGenes([NotNull] Pawn pawn, [NotNull] ThingDef_AlienRace oldRace, [NotNull] ThingDef_AlienRace race)
		{
			if (ThingDefOf.Human == race)
			{
				// Reversion
				// Revert endo genes to original.
				pawn.genes.Endogenes.Clear();

				var graphicsComp = pawn.GetComp<InitialGraphicsComp>();
				if (graphicsComp != null)
					pawn.genes.Endogenes.AddRange(graphicsComp.InitialEndoGenes);

				if (PawnmorpherMod.Settings.generateEndoGenesForAliens)
				{
					// Taken from Verse.PawnGenerator
					if (pawn.genes.GetMelaninGene() == null)
					{
						GeneDef geneDef = PawnSkinColors.RandomSkinColorGene(pawn);
						if (geneDef != null)
						{
							pawn.genes.AddGene(geneDef, xenogene: false);

							Color? skinColor = geneDef.skinColorBase;
							if (skinColor.HasValue)
							{
								pawn.story.SkinColorBase = skinColor.Value;
								pawn.story.skinColorOverride = null;
							}
						}

					}

					if (pawn.genes.GetHairColorGene() == null)
					{
						Color? hairColor;
						GeneDef geneDef2 = PawnHairColors.RandomHairColorGeneFor(pawn);
						if (geneDef2 != null)
						{
							pawn.genes.AddGene(geneDef2, xenogene: false);
							hairColor = geneDef2.hairColorOverride;
						}
						else
						{
							hairColor = PawnHairColors.RandomHairColor(pawn, pawn.story.SkinColorBase, pawn.ageTracker.AgeBiologicalYears);
						}

						if (hairColor.HasValue)
							pawn.story.HairColor = hairColor.Value;
					}
				}
			}
			else
			{
				// Remove invalid endo genes
				for (int i = 0; i < pawn.genes.Endogenes.Count; i++)
				{
					Gene gene = pawn.genes.Endogenes[i];
					if (RaceRestrictionSettings.CanHaveGene(gene.def, race, false) == false)
						pawn.genes.Endogenes.Remove(gene);
				}
			}

			// Remove invalid xeno genes.
			for (int i = 0; i < pawn.genes.Xenogenes.Count; i++)
			{
				Gene gene = pawn.genes.Xenogenes[i];
				if (RaceRestrictionSettings.CanHaveGene(gene.def, race, true) == false)
					pawn.genes.Endogenes.Remove(gene);
			}
		}

		private static void ValidateGraphicsPaths([NotNull] Pawn pawn, [NotNull] ThingDef_AlienRace oldRace, [NotNull] ThingDef_AlienRace race)
		{
			var alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();
			var story = pawn.story;
			if (alienComp == null)
			{
				Log.Error($"trying to validate the graphics of {pawn.Name} but they don't have an {nameof(AlienPartGenerator.AlienComp)}!");
				return;
			}

			var oldGen = oldRace.alienRace.generalSettings.alienPartGenerator;
			var newGen = race.alienRace.generalSettings.alienPartGenerator;

			story.headType = TransferPawnPart(newGen.headTypes.Where(x => x.gender == Gender.None || x.gender == pawn.gender).ToList(), story.headType);

			// alienComp.headVariant = newGen.headTypes.IndexOf();  oldGen.headTypes[alienComp.headVariant]
			story.bodyType = TransferPawnPart(newGen.bodyTypes, story.bodyType);

			// Transfer hair
			pawn.story.hairDef = TransferStyle<HairDef>(pawn, oldRace, race, pawn.story.hairDef, RimWorld.HairDefOf.Bald);

			// Transfer beard
			if (pawn.style.CanWantBeard)
				pawn.style.beardDef = TransferStyle<BeardDef>(pawn, oldRace, race, pawn.style.beardDef, BeardDefOf.NoBeard);

			// Regenerate in case target race has different channels.
			// Default is "skin" and "hair" but might also have "eyes" or "tail"
			alienComp.ColorChannels.Clear();
		}


		private static T TransferPawnPart<T>(List<T> collection, T current)
		{
			// Identify equivalent head. 
			int index = collection.FindIndex(x => x.Equals(current));

			// If no equivalent body type found, select random.
			if (index == -1)
				index = Rand.Range(0, collection.Count);

			if (collection.Count > 0)
			{
				return collection[index];
			}

			return current;
		}


		private static T TransferStyle<T>(Pawn pawn, ThingDef_AlienRace originalRace, ThingDef_AlienRace targetRace, T current, T noStyle) where T : StyleItemDef
		{
			// Log.Message($"Transferring {typeof(T).ToString()} from {originalRace.defName} to {targetRace.defName}");

			// Transfer beard
			StyleSettings targetStyle = targetRace.alienRace?.styleSettings?[typeof(T)];
			StyleSettings oldStyle = originalRace.alienRace?.styleSettings?[typeof(T)];

			// Target race has hair
			if (targetStyle != null && targetStyle.hasStyle)
			{
				// Current race has hair
				if (oldStyle != null && oldStyle.hasStyle)
				{
					if (current != null && current != noStyle)
					{
						// If pawn still supports current hairstyle, keep it.
						if (PawnStyleItemChooser.WantsToUseStyle(pawn, current))
							return current;
					}

					// Otherwise pick a new hairstyle.
					return GetRandomStyle<T>(pawn);
				}
			}
			else
				return noStyle;

			return current;
		}

		static private T GetRandomStyle<T>(Pawn pawn) where T : StyleItemDef
		{
			if (typeof(HairDef) == typeof(T))
			{
				return PawnStyleItemChooser.RandomHairFor(pawn) as T;
			}
			else if (typeof(BeardDef) == typeof(T))
			{
				return PawnStyleItemChooser.RandomBeardFor(pawn) as T;
			}

			return null;
		}


		static void ReRollRaceTraits(Pawn pawn, ThingDef_AlienRace newRace)
		{
			var traitSet = pawn.story?.traits;
			if (traitSet == null) return;
			var allAlienTraits = newRace.alienRace.generalSettings?.forcedRaceTraitEntries;
			if (allAlienTraits == null || allAlienTraits.Count == 0) return;
			//removing traits not supported right now, Rimworld doesn't like it when you remove traits 


			var traitsToAdd = allAlienTraits;
			foreach (AlienChanceEntry<TraitWithDegree> alienTraitEntry in traitsToAdd)
			{
				var trait = alienTraitEntry.entry;
				if (traitSet.HasTrait(trait.def)) continue; //don't add traits that are already added 

				var add = (Rand.RangeInclusive(0, 100) <= alienTraitEntry.chance);

				if (add && pawn.gender == Gender.Male && alienTraitEntry.commonalityMale > 0)
				{
					add = Rand.RangeInclusive(0, 100) <= alienTraitEntry.commonalityMale;
				}
				else if (add && pawn.gender == Gender.Female && alienTraitEntry.commonalityFemale > 0) //only check gender chance if the add roll has passed 
				{                                                                                        //this is consistent with how the alien race framework handles it  
					add = Rand.RangeInclusive(0, 100) <= alienTraitEntry.commonalityMale;
				}


				if (add)
				{
					int traitDegree = trait.degree;

					// If trait has degrees and the degree is not valid, then get degree of first trait entry.
					if (trait.def.degreeDatas.Count > 0 && trait.def.degreeDatas.Any(x => x.degree == traitDegree) == false)
					{
						traitDegree = trait.def.degreeDatas[0].degree;
					}

					traitSet.GainTrait(new Trait(trait.def, traitDegree, true));

					var degree = trait.def.DataAtDegree(traitDegree);
					if (degree.skillGains != null)
						UpdateSkillsPostAdd(pawn, degree.skillGains); //need to update the skills manually
				}
			}
		}

		static void UpdateSkillsPostAdd(Pawn pawn, IList<SkillGain> skillgains)
		{
			var skills = pawn.skills;
			if (skills == null) return;

			foreach (SkillGain skill in skillgains)
			{
				var skRecord = skills.GetSkill(skill.skill);
				skRecord.Level += skill.amount;
			}
		}

		/// <summary>
		/// change the given pawn to the hybrid race of the desired morph
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="morph">the morph to change the pawn to</param>
		/// <param name="addMissingMutations">if true, any missing mutations will be applied to the pawn</param>
		/// <param name="displayNotifications">if set to <c>true</c> display race shit notifications.</param>
		/// <exception cref="ArgumentNullException">
		/// pawn
		/// or
		/// morph
		/// </exception>
		public static void ChangePawnToMorph([NotNull] Pawn pawn, [NotNull] MorphDef morph, bool addMissingMutations = true, bool displayNotifications = true)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			if (morph == null) throw new ArgumentNullException(nameof(morph));
			if (morph.hybridRaceDef == null)
			{
				Log.Error($"tried to change pawn {pawn.Name.ToStringFull} to morph {morph.defName} but morph has no hybridRace!");
				return;
			}
			if (pawn.def != ThingDefOf.Human && !pawn.IsHybridRace())
			{

				return;
			}

			var oldRace = pawn.def;
			//apply mutations 
			if (addMissingMutations)
				SwapMutations(pawn, morph);

			if (morph.raceSettings?.requiredMutations != null)
			{
				CheckRequiredMutations(pawn, morph.raceSettings.requiredMutations);
			}

			var hRace = morph.hybridRaceDef;

			if (!oldRace.IsHybridRace()) //rescan the graphics if the old race is not a morph (ie, human) and save the graphics settings for reversion later 
			{
				var cacherComp = pawn.TryGetComp<InitialGraphicsComp>();
				cacherComp?.ScanGraphics();
			}


			MorphDef.TransformSettings tfSettings = morph.transformSettings;
			ChangePawnRace(pawn, hRace, true);

			if (pawn.IsColonist)
			{
				PortraitsCache.SetDirty(pawn);
			}

			if (displayNotifications && (pawn.IsColonist || pawn.IsPrisonerOfColony))
				SendHybridTfMessage(pawn, oldRace, tfSettings);



			if (tfSettings?.transformTale != null) TaleRecorder.RecordTale(tfSettings.transformTale, pawn);
			pawn.TryGainMemory(tfSettings?.transformationMemory ?? PMThoughtDefOf.DefaultMorphTfMemory);
		}

		private static void CheckRequiredMutations([NotNull] Pawn pawn, [NotNull] List<MutationDef> requiredMutations)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			if (requiredMutations == null) throw new ArgumentNullException(nameof(requiredMutations));
			List<BodyPartRecord> addLst = new List<BodyPartRecord>();
			foreach (MutationDef requiredMutation in requiredMutations)
			{
				if (requiredMutation.parts == null) continue;
				addLst.Clear();
				foreach (BodyPartRecord record in pawn.GetAllNonMissingParts(requiredMutation.parts)) //get all parts missing the required mutations 
				{
					var hediff = pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.def == requiredMutation && h.Part == record);
					if (hediff == null)
					{
						addLst.Add(record);
					}
				}

				if (addLst.Count != 0)
				{
					MutationUtilities.AddMutation(pawn, requiredMutation, addLst);
				}
			}
		}

		[NotNull]
		private static readonly List<Hediff> _rmList = new List<Hediff>();

		private static void FixHediffs([NotNull] Pawn pawn, [NotNull] ThingDef oldRace, [NotNull] MorphDef morph)
		{
			var transformer = morph.raceSettings.Transformer;


			_rmList.Clear();
			foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
			{
				if (hediff.Part == null) continue;
				var newRecord = transformer.Transform(hediff.Part, pawn.RaceProps.body);
				if (newRecord != null)
				{
					hediff.Part = newRecord;
				}
				else
				{
					_rmList.Add(hediff);
				}
			}

			foreach (Hediff hediff in _rmList)
			{
				pawn.health.RemoveHediff(hediff);
			}

		}

		private static void SwapMutations([NotNull] Pawn pawn, [NotNull] MorphDef morph)
		{
			if (pawn.health?.hediffSet?.hediffs == null)
			{
				Log.Error($"pawn {pawn.Name} has null health or hediffs?");
				return;
			}

			var partDefsToAddTo = pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>() //only want to count mutations 
									  .Where(m => m.Part != null && !m.def.HasComp(typeof(SpreadingMutationComp)) && !morph.IsAnAssociatedMutation(m))
									  //don't want mutations without a part or mutations that spread past the part they were added to 
									  .Select(m => m.Part)
									  .ToList(); //needs to be a list because we're about to modify hediffs 

			List<BodyPartRecord> addedRecords = new List<BodyPartRecord>();

			foreach (BodyPartRecord bodyPartRecord in partDefsToAddTo)
			{
				if (addedRecords.Contains(bodyPartRecord)) continue; //if a giver already added to the record don't add it twice 

				// ReSharper disable once AssignNullToNotNullAttribute
				var mutation = morph.GetMutationForPart(bodyPartRecord.def).RandomElementWithFallback();
				if (mutation != null)
				{
					var result = MutationUtilities.AddMutation(pawn, mutation, bodyPartRecord);
					foreach (Hediff_AddedMutation addedMutation in result)
					{
						addedRecords.Add(addedMutation.Part);
					}
				}
			}
		}

		private static void SendHybridTfMessage(Pawn pawn, ThingDef oldRace, MorphDef.TransformSettings tfSettings)
		{
			string label;

			label = string.IsNullOrEmpty(tfSettings?.transformationMessage)
						? RACE_CHANGE_MESSAGE_ID.Translate(pawn.LabelShort, oldRace.label)
						: tfSettings.transformationMessage.Formatted(pawn.LabelShort);

			label = label.CapitalizeFirst();



			var messageDef = tfSettings.messageDef ?? MessageTypeDefOf.NeutralEvent;
			Messages.Message(label, pawn, messageDef);
		}

		private static void HandleGraphicsChanges(Pawn pawn, MorphDef morph)
		{
			var comp = pawn.GetComp<AlienPartGenerator.AlienComp>();
			if (morph != null)
			{
				comp.ColorChannels["skin"].first = morph.GetSkinColorOverride(pawn) ?? comp.ColorChannels["skin"].first;
				comp.ColorChannels["skin"].second = morph.GetSkinColorSecondOverride(pawn) ?? comp.ColorChannels["skin"].second;

				comp.ColorChannels["hair"].first = morph.GetHairColorOverride(pawn) ?? pawn.story.HairColor;
				comp.ColorChannels["hair"].second = morph.GetHairColorOverrideSecond(pawn) ?? comp.ColorChannels["hair"].second;
			}
		}

		/// <summary>
		/// change the race of the pawn back to human 
		/// </summary>
		/// <param name="pawn"></param>
		public static void RevertPawnToHuman([NotNull] Pawn pawn)
		{
			var race = pawn.def;

			var human = ThingDefOf.Human;
			if (race == human) return; //do nothing 


			var oldMorph = pawn.def.GetMorphOfRace();
			bool isHybrid = oldMorph != null;


			DebugLogUtils.Assert(isHybrid, "pawn.IsHybridRace()");
			if (!isHybrid) return;

			var storedGraphics = pawn.GetComp<GraphicSys.InitialGraphicsComp>();

			ChangePawnRace(pawn, human);
			storedGraphics.RestoreGraphics();


			var morphRThought = oldMorph.transformSettings?.revertedMemory;
			morphRThought = morphRThought ?? PMThoughtDefOf.DefaultMorphRevertsToHuman;

			if (morphRThought != null)
				pawn.TryGainMemory(morphRThought);
			var messageStr = RACE_REVERT_MESSAGE_ID.Translate(pawn.LabelShort).CapitalizeFirst();
			Messages.Message(messageStr, pawn, MessageTypeDefOf.NeutralEvent);
		}
	}
}
