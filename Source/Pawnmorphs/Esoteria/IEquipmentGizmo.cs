// IEquipmentGizmo.cs created by Iron Wolf for Pawnmorph on 06/08/2020 11:30 AM
// last updated 06/08/2020  11:30 AM

using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// interface for thing comps that have gizmos that need to be displayed when equipped 
	/// </summary>
	public interface IEquipmentGizmo
	{
		/// <summary>
		/// Gets the gizmos.
		/// </summary>
		/// <returns></returns>
		[NotNull]
		IEnumerable<Gizmo> GetGizmos();
	}
}