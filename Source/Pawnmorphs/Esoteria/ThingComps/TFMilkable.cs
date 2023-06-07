// TFMilkable.cs modified by Iron Wolf for Pawnmorph on 12/26/2019 8:03 AM
// last updated 12/26/2019  8:03 AM

using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.ThingComps
{
	/// <summary>
	/// thing comp for making certain animals drop tf milk if they are 'mutagen infused' 
	/// </summary>
	/// <seealso cref="RimWorld.CompMilkable" />
	public class TFMilkable : CompMilkable
	{
		[NotNull]
		private TFMilkableProps TFComp => (TFMilkableProps)props;

		/// <summary>
		/// Gets the resource definition.
		/// </summary>
		/// <value>
		/// The resource definition.
		/// </value>
		protected override ThingDef ResourceDef
		{
			get
			{
				if (IsMutagenInfused)
				{
					return TFComp.mutagenicProduct;
				}
				else
				{
					return base.ResourceDef;
				}
			}
		}

		bool IsMutagenInfused
		{
			get { return (parent as Pawn)?.GetAspectTracker()?.Contains(AspectDefOf.MutagenInfused, 0) == true; }
		}


	}

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="RimWorld.CompProperties_Milkable" />
	public class TFMilkableProps : CompProperties_Milkable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TFMilkableProps"/> class.
		/// </summary>
		public TFMilkableProps()
		{
			compClass = typeof(TFMilkable);
		}

		/// <summary>
		/// The mutagenic product
		/// </summary>
		public ThingDef mutagenicProduct;

	}
}