// IPartTransformer.cs created by Iron Wolf for Pawnmorph on 03/16/2020 9:12 PM
// last updated 03/16/2020  9:12 PM

using System;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Hybrids
{
	/// <summary>
	/// interface for something that turns a part record from one body def into a part record from another body def 
	/// </summary>
	public interface IPartTransformer
	{
		/// <summary>
		/// Transforms the specified record into a record from the target def 
		/// </summary>
		/// <param name="record">The record.</param>
		/// <param name="targetDef">The target definition.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">record or targetDef</exception>
		[CanBeNull]
		BodyPartRecord Transform([NotNull] BodyPartRecord record, [NotNull] BodyDef targetDef);
	}


	/// <summary>
	/// default implementation of IPartTransformer 
	/// </summary>
	/// <seealso cref="Pawnmorph.Hybrids.IPartTransformer" />
	public class DefaultPartTransformer : IPartTransformer
	{
		/// <summary>
		/// Transforms the specified record into a record from the target def 
		/// </summary>
		/// <param name="record">The record.</param>
		/// <param name="targetDef">The target definition.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">record or targetDef</exception>
		public BodyPartRecord Transform(BodyPartRecord record, BodyDef targetDef)
		{
			if (record == null) throw new ArgumentNullException(nameof(record));
			if (targetDef == null) throw new ArgumentNullException(nameof(targetDef));
			var addr = record.GetAddress();
			return targetDef.GetRecordAt(addr);
		}
	}
}