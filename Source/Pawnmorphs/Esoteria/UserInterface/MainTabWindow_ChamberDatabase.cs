// MainTabWindow_ChamberDatabase.cs created by Iron Wolf for Pawnmorph on 08/26/2020 2:36 PM
// last updated 08/26/2020  2:36 PM

using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Chambers;
using Pawnmorph.Hediffs;
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

        public override void PostOpen()
        {
            base.PostOpen();

            if (Find.WindowStack.TryRemove(typeof(Window_Genebank)) == false)
                Find.WindowStack.Add(new Window_Genebank());

            this.Close();
        }

        public override void DoWindowContents(Rect inRect)
        {

        }
    }
}