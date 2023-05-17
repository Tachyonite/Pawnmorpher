// GraphicsUpdaterComp.cs created by Iron Wolf for Pawnmorph on 09/13/2019 8:14 AM
// last updated 09/13/2019  8:14 AM

#pragma warning disable 1591
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.Hybrids;
using UnityEngine;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils;

namespace Pawnmorph.GraphicSys
{
	/// <summary>
	/// Thing comp to update the graphics of a pawn as they gain/lose mutations. <br/>
	/// Requires that the pawn have a MutationTracker comp too.
	/// </summary>
	[UsedImplicitly]
	public class GraphicsUpdaterComp : ThingComp, IMutationEventReceiver
	{
		private bool _subOnce;
		private Color _effectiveHairColor;
		private bool _suspended;

		/// <summary>
		/// Assigned by <see cref="HPatches.GeneTrackerPatches.EnsureCorrectSkinColorOverridePostfix(Pawn)"/> right before this is triggered.
		/// </summary>
		public Color? GeneOverrideColor { get; set; }

		public bool IsDirty { get; set; }

		[NotNull]
		private Pawn Pawn => (Pawn)parent;

		private AlienPartGenerator.AlienComp GComp => Pawn.GetComp<AlienPartGenerator.AlienComp>();

		private InitialGraphicsComp InitialGraphics => Pawn.GetComp<InitialGraphicsComp>();


		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			Assert(parent is Pawn, "parent is Pawn");
		}


		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (IsDirty)
			{
				RefreshGraphics();
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();

			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				Assert(_subOnce == false, "_subOnce == false");
				_subOnce = true;

				var tracker = Pawn.GetMutationTracker();
				Assert(tracker != null, "tracker != null");
				Assert(Pawn.GetComp<AlienPartGenerator.AlienComp>() != null, "Pawn.GetComp<AlienPartGenerator.AlienComp>() != null");
				Assert(InitialGraphics != null, "InitialGraphics != null");
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			if (IsDirty)
			{
				IsDirty = false;
				RefreshGraphics();
			}
		}

		public void RefreshGraphics()
		{
			if (_suspended)
				return;

			var pawn = (Pawn)parent;
			var mTracker = pawn.GetMutationTracker();
			if (mTracker != null)
			{
				var colorationAspect = pawn.GetAspectTracker()?.GetAspect<Aspects.ColorationAspect>();
				bool force = colorationAspect != null;
				UpdateSkinColor(mTracker, force);
				UpdateHairColor(mTracker, force);
			}

			if (pawn.Spawned)
				pawn.RefreshGraphics();
			IsDirty = false;
		}


		void UpdateSkinColor([NotNull] MutationTracker tracker, bool force = false)
		{
			if (GComp == null || InitialGraphics == null)
				return;

			var colorationAspect = tracker.Pawn.GetAspectTracker()?.GetAspect<Aspects.ColorationAspect>();
			if (colorationAspect != null && colorationAspect.IsFullOverride)
			{
				var color = colorationAspect.ColorSet.skinColor;
				if (color.HasValue)
				{
					GComp.SetSkinColor(color.Value); // Assign to HAR addons. (Might not be needed once HAR fixes updating colors)
					Pawn.story.skinColorOverride = color.Value; // Assign to pawn.
					return;
				}
			}

			var highestInfluence = Pawn.GetHighestInfluence();
			var curMorph = Pawn.def.GetMorphOfRace();
			if (highestInfluence != null)
			{
				// This may cause issues if a pawn's color is changed by outside influences like other mods and end up fighting over it.



				// Calculate skin color based on mutation influences.
				float lerpVal = tracker.GetDirectNormalizedInfluence(highestInfluence);
				var baseColor = GeneOverrideColor ?? curMorph?.GetSkinColorOverride(tracker.Pawn) ?? InitialGraphics.SkinColor;
				var morphColor = highestInfluence.GetSkinColorOverride(tracker.Pawn) ?? InitialGraphics.SkinColor;

				Color effectiveSkinColor = Color.Lerp(baseColor, morphColor, Mathf.Sqrt(lerpVal)); // Blend the 2 by the normalized colors.

				// Log.Message($"Coloring: gene: {GeneOverrideColor}, base: {baseColor}, morph: {morphColor}, effective: {effectiveSkinColor}");

				Pawn.story.skinColorOverride = effectiveSkinColor;
				GComp.SetSkinColor(effectiveSkinColor); // Assign effective skin color to body parts. (Might not be needed once HAR fixes updating colors)
			}
			else
			{
				// If no mutation influence then reset skin color to either gene override or natural skin color (assign null);
				Pawn.story.skinColorOverride = GeneOverrideColor;

				// If we never touch SkinColorBase, then there is actually no reason to reassign with initial.
				Pawn.story.SkinColorBase = InitialGraphics.SkinColor;

				// HAR (at the time of this comment) postfixes Pawn.Story.SkinColor.
				GComp.SetSkinColor(GeneOverrideColor ?? InitialGraphics.SkinColor); // Assign effective skin color to body parts. (Might not be needed once HAR fixes updating colors)
			}
		}

		bool UpdateHairColor([NotNull] MutationTracker tracker, bool force = false)
		{
			if (GComp == null || InitialGraphics == null || Pawn.story == null) return false;

			var colorationAspect = tracker.Pawn.GetAspectTracker()?.GetAspect<Aspects.ColorationAspect>();
			if (colorationAspect != null && colorationAspect.IsFullOverride)
			{
				var color = colorationAspect.ColorSet.hairColor;
				if (color.HasValue)
				{
					Pawn.story.HairColor = color.Value;
					return true;
				}
			}

			var highestInfluence = Pawn.GetHighestInfluence();
			var curMorph = Pawn.def.GetMorphOfRace();

			if (highestInfluence == null)
			{
				if (Pawn.story.HairColor == InitialGraphics.HairColor && !force)
				{
					return false; // If there is not influence or if the highest influence is that of their current race do nothing.
				}
				else
				{
					Pawn.story.HairColor = InitialGraphics.HairColor;
					return true;
				}
			}


			Color currentHairColor = Pawn.story.HairColor;
			// If forced, hair color is set here or hair color is natural then update. (Avoid updating if overriden by genes)
			if (force || currentHairColor == _effectiveHairColor || currentHairColor == InitialGraphics.HairColor)
			{
				float lerpVal = tracker.GetDirectNormalizedInfluence(highestInfluence);
				var baseColor = curMorph?.GetHairColorOverride(tracker.Pawn) ?? InitialGraphics.HairColor;
				var morphColor = highestInfluence.GetHairColorOverride(tracker.Pawn) ?? InitialGraphics.HairColor;

				// If base is transparent don't do anything
				if (baseColor.a == 0)
					return false;

				Color col;
				if (morphColor.a == 0)
				{
					// If target is transparent, keep using base.
					col = baseColor;
				}
				else
					col = Color.Lerp(baseColor, morphColor, Mathf.Sqrt(lerpVal)); //blend the 2 by the normalized colors 

				_effectiveHairColor = col;
				Pawn.story.HairColor = col;
			}


			GComp.ColorChannels["hair"].first = Pawn.story.HairColor;

			return true;
		}

		void IMutationEventReceiver.MutationAdded(Hediff_AddedMutation mutation, MutationTracker tracker)
		{
			IsDirty = true;
		}


		void IMutationEventReceiver.MutationRemoved(Hediff_AddedMutation mutation, MutationTracker tracker)
		{
			IsDirty = true;
		}


		public void BeginUpdate()
		{
			_suspended = true;
		}

		public void EndUpdate()
		{
			_suspended = false;
			RefreshGraphics();
		}
	}
}