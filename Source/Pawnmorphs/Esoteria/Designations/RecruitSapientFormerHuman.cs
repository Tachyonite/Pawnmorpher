// RecruitSapientFormerHuman.cs created by Iron Wolf for Pawnmorph on 03/15/2020 2:54 PM
// last updated 03/15/2020  2:54 PM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Designations
{
	/// <summary>
	/// designation class for 'recruiting' sapient former humans
	/// </summary>
	/// <seealso cref="Verse.Designator" />
	public class RecruitSapientFormerHuman : Designator
	{

		/// <summary>
		/// Gets the designation definition 
		/// </summary>
		/// <value>
		/// The designation.
		/// </value>
		protected override DesignationDef Designation => PMDesignationDefOf.RecruitSapientFormerHuman;

		[NotNull]
		private readonly List<Pawn> _justDesignated = new List<Pawn>();
		/// <summary>
		/// Initializes a new instance of the <see cref="RecruitSapientFormerHuman"/> class.
		/// </summary>
		public RecruitSapientFormerHuman()
		{
			this.defaultLabel = (string)"PMDesignatorRecruitSapient".Translate();
			this.defaultDesc = (string)"PMDesignatorRecruitSapientDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("UI/Designators/Tame", true);
			this.soundDragSustain = SoundDefOf.Designate_DragStandard;
			this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			this.useMouseIcon = true;
			this.soundSucceeded = SoundDefOf.Designate_Claim;
			this.hotKey = KeyBindingDefOf.Misc4;
		}

		/// <summary>
		/// Gets the draggable dimensions.
		/// </summary>
		/// <value>
		/// The draggable dimensions.
		/// </value>
		public override int DraggableDimensions
		{
			get
			{
				return 2;
			}
		}
		/// <summary>
		/// Designates the single cell.
		/// </summary>
		/// <param name="loc">The loc.</param>
		public override void DesignateSingleCell(IntVec3 loc)
		{
			foreach (Thing t in this.TameablesInCell(loc))
				this.DesignateThing(t);
		}

		/// <summary>
		/// Determines whether this instance with the specified t [can designate thing] 
		/// </summary>
		/// <param name="t">The t.</param>
		/// <returns></returns>
		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			return t is Pawn pawn && pawn.IsSapientFormerHuman() && pawn.Faction == null && Map.designationManager.DesignationOn(pawn, Designation) == null;
		}

		/// <summary>
		/// Finalizes the designation succeeded.
		/// </summary>
		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();

			_justDesignated.Clear();

		}

		/// <summary>
		/// Designates the thing.
		/// </summary>
		/// <param name="t">The t.</param>
		public override void DesignateThing(Thing t)
		{
			Map.designationManager.RemoveAllDesignationsOn(t);
			Map.designationManager.AddDesignation(new Designation((LocalTargetInfo)t, Designation));
			_justDesignated.Add((Pawn)t);

			if (t is Pawn pawn)
			{
				if (pawn.guest != null && pawn.guest.lastRecruiterName == null)
				{
					pawn.guest.resistance = 10 * pawn.def.race.wildness;
				}
			}
		}

		private IEnumerable<Pawn> TameablesInCell(IntVec3 c)
		{
			RecruitSapientFormerHuman designatorTame = this;
			if (!c.Fogged(designatorTame.Map))
			{
				List<Thing> thingList = c.GetThingList(designatorTame.Map);
				for (var i = 0; i < thingList.Count; ++i)
					if (designatorTame.CanDesignateThing(thingList[i]).Accepted)
						yield return (Pawn)thingList[i];
			}
		}

		/// <summary>
		/// Determines whether this instance with the specified c [can designate cell] 
		/// </summary>
		/// <param name="c">The c.</param>
		/// <returns></returns>
		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(Map))
				return false;
			if (!TameablesInCell(c).MakeSafe().Any())
				return "MessageMustDesignateTameable".Translate();
			return true;
		}
	}
}