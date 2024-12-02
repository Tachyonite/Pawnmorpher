using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnmorph.Interfaces
{
	/// <summary>
	/// Used by hediffs to indicate that they can log and provide causation information.
	/// </summary>
	public interface ICaused
	{
		/// <summary>
		/// Gets information related to what caused this mutation hediff.
		/// </summary>
		MutationCauses Causes { get; }
	}
}
