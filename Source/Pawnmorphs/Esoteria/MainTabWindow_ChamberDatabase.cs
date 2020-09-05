// MainTabWindow_ChamberDatabase.cs created by Iron Wolf for Pawnmorph on 08/26/2020 2:36 PM
// last updated 08/26/2020  2:36 PM

using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Chambers;
using Pawnmorph.Hediffs;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    ///     main tab window for the chamber database
    /// </summary>
    /// <seealso cref="RimWorld.MainTabWindow" />
    public class MainTabWindow_ChamberDatabase : MainTabWindow
    {
        private Vector2 _scrollPosition;
        private float _scrollViewHeight;


        private List<TabRecord> _tabs;

        private Mode _curMode;
        /// <summary>
        /// Gets the size of the requested tab.
        /// </summary>
        /// <value>
        /// The size of the requested tab.
        /// </value>
        public override Vector2 RequestedTabSize => new Vector2(1010f, 640f);

        private static readonly Texture2D PinTex = ContentFinder<Texture2D>.Get("UI/Icons/Pin");
        private ChamberDatabase _chamberDatabase;

        /// <summary>
        /// called just before the tab is opened 
        /// </summary>
        public override void PreOpen()
        {
            base.PostOpen();
            _rowEntries.Clear();
        }

        [NotNull]
        private List<TabRecord> Tabs
        {
            get
            {
                if (_tabs == null)
                    _tabs = new List<TabRecord>
                    {
                        new TabRecord("PMDBTaggedAnimal".Translate(), () => SetMode(Mode.Animal), () => _curMode == Mode.Animal),
                        new TabRecord("PMDBTaggedMutations".Translate(), () => SetMode(Mode.Mutations),
                                      () => _curMode == Mode.Mutations)
                    };

                return _tabs;
            }
        }

        [NotNull]
        private ChamberDatabase Database
        {
            get
            {
                ChamberDatabase database = _chamberDatabase;
                if (database != null) return database;

                _chamberDatabase = Find.World.GetComponent<ChamberDatabase>();
                if (_chamberDatabase == null) Log.Error("Unable to find chamber database world component!");

                return _chamberDatabase;
            }
        }


        /// <summary>
        /// Does the window contents.
        /// </summary>
        /// <param name="inRect">The in rect.</param>
        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            Rect sRect = inRect;
            sRect.yMin += 45f;
            TabDrawer.DrawTabs(sRect, Tabs);
            switch (_curMode)
            {
                case Mode.Animal:
                    DrawAnimalsTab(sRect);
                    break;
                case Mode.Mutations:
                    DrawMutationsTab(sRect);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        struct RowEntry
        {
            public RowEntry(MutationDef mDef)
            {
                label = mDef.label;
                storageSpaceUsed = mDef.GetRequiredStorage(); 
            }

            public RowEntry(PawnKindDef pkDef)
            {
                label = pkDef.label;
                storageSpaceUsed = pkDef.GetRequiredStorage();
            }

            public string label;
            public int storageSpaceUsed; 
        }

        [NotNull]
        readonly 
        private List<RowEntry> _rowEntries = new List<RowEntry>(); 


        [NotNull] readonly 
        private StringBuilder _builder = new StringBuilder();


        void DrawTable(Rect inRect)
        {
            inRect.yMin += 10f;
            inRect.yMax += 40f;
            Rect mainView = inRect.ContractedBy(10f);

            Rect outRect = new Rect(inRect.x, inRect.y, mainView.width, mainView.height - inRect.y - 10f);

            Rect viewRect = new Rect(mainView.x + mainView.width / 2f + 10f, mainView.y, mainView.width / 2f - 10f - 16f, mainView.height);
            var viewWidth = viewRect.width / 3 - 10f;
            Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);
            try
            {
                if (_rowEntries.Count == 0) return;

                for (var index = 0; index < _rowEntries.Count; index++)
                {
                    RowEntry rowEntry = _rowEntries[index];
                    var rect = viewRect; //TODO fix this 
                    rect.y += index * 30; //go down 1 row? 
                    DrawRow(rowEntry, rect);
                }

                //TODO scroll view stuff 
                //use _rowEntries to get the needed information 
                // Set the scroll view height
              

            }
            finally
            {
                Widgets.EndScrollView();
            }

        }

        void DrawRow(RowEntry entry, Rect inRect)
        {
            Widgets.Label(inRect, entry.label + " : " + entry.storageSpaceUsed + "/" + Database.TotalStorage);
        }

        private void DrawMutationsTab(Rect inRect)
        {
            _rowEntries.Clear(); //TODO fix this 
            foreach (MutationDef mutation in Database.StoredMutations)
            {
                _rowEntries.Add(new RowEntry(mutation)); 
            }

            DrawTable(inRect); 

        }

        private void DrawAnimalsTab(Rect inRect)
        {
            _rowEntries.Clear();
            foreach (PawnKindDef taggedAnimal in Database.TaggedAnimals)
            {
                _rowEntries.Add(new RowEntry(taggedAnimal));
            }

            DrawTable(inRect);
        }


        private void SetMode(Mode mode)
        {
            _curMode = mode;
        }

        private enum Mode
        {
            Animal,
            Mutations
        }
    }
}