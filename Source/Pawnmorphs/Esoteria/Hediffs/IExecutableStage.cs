// IExecutableStage.cs created by Iron Wolf for Pawnmorph on 01/02/2020 2:10 PM
// last updated 01/02/2020  2:10 PM

using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// interface for hediff stages that execute something when they are entered 
	/// </summary>
	public interface IExecutableStage
	{
		/// <summary>called when the given hediff enters this stage</summary>
		/// <param name="hediff">The hediff.</param>
		void EnteredStage([NotNull] Hediff hediff);
	}
}