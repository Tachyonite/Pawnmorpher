using Pawnmorph.Chambers;

namespace Pawnmorph.Genebank.Model
{
	/// <summary>
	/// An interface for various kinds of data that can be stored in genebanks
	/// </summary>
	public interface IGenebankEntry : Verse.IExposable
	{
		/// <summary>
		/// Returns the caption for the genebank entry
		/// </summary>
		/// <returns>The genebank entry caption</returns>
		string GetCaption();

		/// <summary>
		/// Computes the required amount of storage to store this genebank entry
		/// </summary>
		/// <returns>The required storage, in kMb</returns>
		int GetRequiredStorage();

		/// <summary>
		/// Tests for any additional requirements preventing this entry from being added to the database
		/// (Not including basic stuff like sufficient storage space)
		/// </summary>
		/// <param name="database">The chamber database</param>
		/// <param name="reason">A string for returning the reason why an entry cannot be added</param>
		/// <returns>True if the entry can be added to database, false if it cannot</returns>
		bool CanAddToDatabase(ChamberDatabase database, out string reason);

		/// <summary>
		/// Returns true if this entry is still valid, or should be removed.
		/// </summary>
		bool IsValid();
	}
}
