using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnmorph.Interfaces
{
	internal interface ICaused
	{
		MutationCauses Causes { get; }
	}
}
