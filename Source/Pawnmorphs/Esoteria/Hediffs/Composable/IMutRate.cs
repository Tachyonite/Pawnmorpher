using JetBrains.Annotations;

namespace Pawnmorph.Hediffs.Composable
{
	/// <summary>
	/// interface for all mutation rate specifying classes 
	/// </summary>
	public interface IMutRate
	{
		/// <summary>
		/// get a string giving debug information about the specified hediff.
		/// </summary>
		/// <param name="hediff">The hediff.</param>
		/// <returns></returns>
		string DebugString(Hediff_MutagenicBase hediff);

		/// <summary>
		/// Gets the mutations per second from the specified hediff.
		/// </summary>
		/// <param name="hediff">The hediff.</param>
		/// <returns></returns>
		int GetMutationsPerSecond([NotNull] Hediff_MutagenicBase hediff);

		/// <summary>
		/// Gets the mutations per severity for the given hediff and change in severity.
		/// </summary>
		/// <param name="hediff">The hediff.</param>
		/// <param name="sevChange">The sev change.</param>
		/// <returns></returns>
		int GetMutationsPerSeverity([NotNull] Hediff_MutagenicBase hediff, float sevChange);
	}
}