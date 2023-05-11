using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnmorph.FormerHumans
{
	/// <summary>
	/// Used to describe if an animal can be appear as a former human.
	/// </summary>
	public enum FormerHumanRestrictions : byte
	{
		/// <summary>
		/// This animal type is a valid former human and can be spawned randomly without restrictions.
		/// </summary>
		Enabled = 0,

		/// <summary>
		/// This animal type is a valid former human but cannot be spawned randomly.
		/// </summary>
		Restricted = 1,

		/// <summary>
		/// This animal type is not a valid former human.
		/// </summary>
		Disabled = 2,
	}
}
