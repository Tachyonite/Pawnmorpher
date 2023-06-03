using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.ModExtensions
{
	/// <summary>
	/// Used by animal genomes to decide what group of animals it can pick from.
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	internal class AnimalFilterModExtension : DefModExtension
	{
		/// <summary>
		/// Allow genome to select a non-restricted animal.
		/// </summary>
		public bool allowNormal = false;

		/// <summary>
		/// Allow genome to select a restricted animal.
		/// </summary>
		public bool allowRestricted = false;
	}
}
