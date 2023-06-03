using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnmorph.Utilities
{
	/// <summary>
	/// Allows for hot swapping files while game is running.
	/// https://github.com/Zetrith/HotSwap/wiki	
	/// </summary>
	/// <seealso cref="System.Attribute" />
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class HotSwappableAttribute : Attribute
	{
	}
}
