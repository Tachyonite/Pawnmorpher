// MainTabWindow_ChamberDatabase.cs created by Iron Wolf for Pawnmorph on 08/26/2020 2:36 PM
// last updated 08/26/2020  2:36 PM

using Pawnmorph.UserInterface;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	///     main tab window for the chamber database
	/// </summary>
	/// <seealso cref="RimWorld.MainTabWindow" />
	public partial class MainTabWindow_ChamberDatabase : MainTabWindow
	{
		/// <inheritdoc />
		public override void PostOpen()
		{
			base.PostOpen();

			if (Find.WindowStack.TryRemove(typeof(Window_Genebank)) == false)
			{
				Window_Genebank genebankWindow = new Window_Genebank();

				if (Event.current.control)
					genebankWindow.ResetToDefaults();

				Find.WindowStack.Add(genebankWindow);
			}

			this.Close();
		}

		/// <inheritdoc />
		public override void DoWindowContents(Rect inRect)
		{

		}
	}
}