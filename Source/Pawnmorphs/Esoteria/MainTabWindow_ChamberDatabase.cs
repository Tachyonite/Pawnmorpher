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
    public partial class MainTabWindow_ChamberDatabase : MainTabWindow
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

        readonly struct RowEntry : IEquatable<RowEntry>
        { 
            public RowEntry(MutationDef mDef)
            {
                label = mDef.label;
                storageSpaceUsed = mDef.GetRequiredStorage();
                def = mDef;
            }

            public RowEntry(PawnKindDef pkDef)
            {
                label = pkDef.label;
                storageSpaceUsed = pkDef.GetRequiredStorage();
                def = pkDef; 
            }


            public readonly Def def; 
            public readonly string label;
            public readonly int storageSpaceUsed;

            /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
            /// <param name="other">An object to compare with this object.</param>
            /// <returns>
            /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
            public bool Equals(RowEntry other)
            {
                return Equals(def, other.def)
                    && label == other.label
                    && storageSpaceUsed == other.storageSpaceUsed;
            }

            /// <summary>Indicates whether this instance and a specified object are equal.</summary>
            /// <param name="obj">The object to compare with the current instance. </param>
            /// <returns>
            /// <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />. </returns>
            public override bool Equals(object obj)
            {
                return obj is RowEntry other && Equals(other);
            }

            /// <summary>Returns the hash code for this instance.</summary>
            /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = (def != null ? def.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (label != null ? label.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ storageSpaceUsed;
                    return hashCode;
                }
            }

            /// <summary>Returns a value that indicates whether the values of two <see cref="T:Pawnmorph.MainTabWindow_ChamberDatabase.RowEntry" /> objects are equal.</summary>
            /// <param name="left">The first value to compare.</param>
            /// <param name="right">The second value to compare.</param>
            /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
            public static bool operator ==(RowEntry left, RowEntry right)
            {
                return left.Equals(right);
            }

            /// <summary>Returns a value that indicates whether two <see cref="T:Pawnmorph.MainTabWindow_ChamberDatabase.RowEntry" /> objects have different values.</summary>
            /// <param name="left">The first value to compare.</param>
            /// <param name="right">The second value to compare.</param>
            /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
            public static bool operator !=(RowEntry left, RowEntry right)
            {
                return !left.Equals(right);
            }
        }

        [NotNull]
        readonly 
        private List<RowEntry> _rowEntries = new List<RowEntry>(); 


        [NotNull] readonly 
        private StringBuilder _builder = new StringBuilder();

         

        void RemoveFromDB(Def def)
        {
            var db = Find.World.GetComponent<ChamberDatabase>();
            //explicit type casts here are hacky but best way to reuse the gui code 
            if (def is PawnKindDef pkDef)
            {
                db.RemoveFromDatabase(pkDef); 
            }else if (def is MutationDef mDef)
            {
                db.RemoveFromDatabase(mDef); 
            }
            else
            {
                throw new NotImplementedException(nameof(RemoveFromDB) + " is not implemented for " + def.GetType().Name + "!"); 
            }



        }

        void DrawTable(Rect inRect, RowHeader header)
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
                //draw the header row
                DrawRow(header, viewRect);
                viewRect.y += 30;


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

        private const float DESCRIPTION_ROW_FRACT = 0.66f;
        private const float TEXT_ROW_FRACT = 4f / 5f;
        private const float BUTTON_FRACT = 1f - TEXT_ROW_FRACT; 
        private const float DESC_ROW_T_FRACT = DESCRIPTION_ROW_FRACT * TEXT_ROW_FRACT;
        private const float STORAGE_INFO_FRACT = (1 - DESCRIPTION_ROW_FRACT) * (TEXT_ROW_FRACT);

        void DrawRow(RowHeader header, Rect inRect)
        {
            float tW = inRect.width * 2f;
            float wTxt = tW * DESC_ROW_T_FRACT;
            float wST = tW * STORAGE_INFO_FRACT;
            float wButton = BUTTON_FRACT * tW;
            float x0 = inRect.x;
            float x1 = DESC_ROW_T_FRACT * tW + x0;
            float x2 = TEXT_ROW_FRACT * tW + x0;


            Rect txtHRect = new Rect(inRect) { width = wTxt };
            Rect stInfoHRect = new Rect(inRect) { x = x1, width = wST };
            Rect buttonHRect = new Rect(inRect) { x = x2, width = wButton};
            Widgets.Label(txtHRect, header.defHeader);
            Widgets.Label(stInfoHRect, header.totalHeader);
            Widgets.Label(buttonHRect, header.removeHeader);
        }
        void DrawRow(RowEntry entry, Rect inRect)
        {
            float tW = inRect.width * 2f;
            float wTxt = tW * DESC_ROW_T_FRACT;
            float wST = tW * STORAGE_INFO_FRACT;
            float wButton = BUTTON_FRACT * tW; 
            float x0 = inRect.x;
            float x1 = DESC_ROW_T_FRACT * tW + x0;
            float x2 = TEXT_ROW_FRACT * tW + x0;


            Rect txtRect = new Rect(inRect) {width = wTxt};
            Rect stInfoRect = new Rect(inRect) {x = x1, width = wST};
            Rect buttonRect = new Rect(inRect) {x = x2, width = wButton, height = 10};

            Widgets.Label(txtRect, GetDescriptionStringFor(entry));
            Widgets.Label(stInfoRect, Database.TotalStorage.ToString());
            if (Widgets.ButtonImage(buttonRect, PMTexButton.CloseXSmall))
            {
                RemoveFromDB(entry.def); 
            }

        }

        private void DrawMutationsTab(Rect inRect)
        {
            _rowEntries.Clear(); //TODO fix this 
            foreach (MutationDef mutation in Database.StoredMutations)
            {
                _rowEntries.Add(new RowEntry(mutation)); 
            }

            DrawTable(inRect, _cachedHeaders[(int) Mode.Mutations]); 

        }

        private void DrawAnimalsTab(Rect inRect)
        {
            _rowEntries.Clear();
            foreach (PawnKindDef taggedAnimal in Database.TaggedAnimals)
            {
                _rowEntries.Add(new RowEntry(taggedAnimal));
            }

            DrawTable(inRect, _cachedHeaders[(int) Mode.Animal]);
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