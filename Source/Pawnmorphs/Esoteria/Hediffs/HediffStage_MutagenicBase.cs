using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Composable.Hediffs;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// Abstract base class for all hediff stages that involve mutation or
	/// transformation, for use with Hediff_MutagenicBase.
	/// </summary>
	/// <seealso cref="Verse.HediffStage" />
	/// <seealso cref="Pawnmorph.Hediffs.IDescriptiveStage" />
	/// <seealso cref="Pawnmorph.Hediffs.Hediff_MutagenicBase" />
	public class HediffStage_MutagenicBase : HediffStage, IDescriptiveStage, IInitializableStage
	{
		/// <summary>
		/// The description.
		/// </summary>
		[UsedImplicitly] public string description;

		/// <summary>
		/// The label override.
		/// </summary>
		[UsedImplicitly] public string labelOverride;


		/// <summary>
		///     the alert this stage sends out once entered
		/// </summary>
		[UsedImplicitly][CanBeNull] public StageAlert alert;

		/// <summary>
		/// Gets the description override.
		/// </summary>
		/// <value>The description override.</value>
		public string DescriptionOverride => description;

		/// <summary>
		/// Gets the label override.
		/// </summary>
		/// <value>The label override.</value>
		public string LabelOverride => labelOverride;

		/// <summary>
		/// gets all configuration errors in this stage .
		/// </summary>
		/// <param name="parentDef">The parent definition.</param>
		/// <returns></returns>
		public virtual IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			return Enumerable.Empty<string>();
		}

		/// <summary>
		/// Resolves all references in this instance.
		/// </summary>
		/// <param name="parent">The parent.</param>
		public virtual void ResolveReferences(HediffDef parent)
		{

		}
	}
}