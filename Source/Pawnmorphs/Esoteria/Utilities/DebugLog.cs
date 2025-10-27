using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.Utilities
{
	internal static class DebugLog
	{
		[Conditional("DEBUG")]
		public static void Message(string message)
		{
			Log.Message($"[{DateTime.Now.TimeOfDay}][Pawnmorpher]: " + message);
		}
	}
}
