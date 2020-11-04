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
        private const string STORAGE_SUFFIX = "MB"; 
        
        
        private const float DESCRIPTION_ROW_FRACT = 0.60f;
        private const float TEXT_ROW_FRACT = 5f / 7f;
        private const float BUTTON_FRACT = 1f - TEXT_ROW_FRACT;
        private const float DESC_ROW_T_FRACT = DESCRIPTION_ROW_FRACT * TEXT_ROW_FRACT;
        private const float STORAGE_INFO_FRACT = (1 - DESCRIPTION_ROW_FRACT) * TEXT_ROW_FRACT;


        //header constants 
        private const float HEADER_LABEL_FRACT = 0.5f;
        private const float HEADER_SPACE_FRACT = 1f - HEADER_LABEL_FRACT;
        private const float HEADER_HEIGHT = 60*2f; 
        
        [NotNull] readonly
            private List<RowEntry> _rowEntries = new List<RowEntry>();



        private Vector2 _scrollPosition;


        private List<TabRecord> _tabs;

        private Mode _curMode;

        private ChamberDatabase _chamberDatabase;

        /// <summary>
        ///     Gets the size of the requested tab.
        /// </summary>
        /// <value>
        ///     The size of the requested tab.
        /// </value>
        public override Vector2 RequestedTabSize => new Vector2(1010f, 740f);

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
        ///     Does the window contents.
        /// </summary>
        /// <param name="inRect">The in rect.</param>
        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            Rect sRect = inRect;


            const float headerScaleFactor = 1.0f; 
            sRect.yMin += 45f;
            var headerRect = new Rect(sRect) {height = HEADER_HEIGHT * headerScaleFactor};


            var iFont = Text.Font;
            try
            {
                Text.Font = GameFont.Medium;
                DrawHeader(headerRect);
            }
            finally
            {
                Text.Font = iFont; 
            }
            
            sRect.y += HEADER_HEIGHT; 

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

        private const string HEADER_LABEL = "PMGenebankHeader";
        private const string AVAILABLE_HEADER_LABEL = "PMGenebankAvailableHeader";
        private const string TOTAL_HEADER_LABEL = "PMGenebankTotalHeader";

        private void DrawHeader(Rect sRect)
        {
            var h = sRect.height / 2f; 

            Rect labelRect = new Rect(sRect) {width = sRect.width * HEADER_LABEL_FRACT, height = h};
            var availableRect = new Rect(sRect)
            {
                width = sRect.width *0.5f * HEADER_SPACE_FRACT,
                x = sRect.x += labelRect.width,
                height = h
            };
            var totalRect = new Rect(sRect)
            {
                width = sRect.width * 0.5f * HEADER_SPACE_FRACT,
                x = availableRect.x + availableRect.width,
                height = h

            };

            //draw the labels 
            Widgets.Label(availableRect, AVAILABLE_HEADER_LABEL.Translate());
            Widgets.Label(totalRect, TOTAL_HEADER_LABEL.Translate());

            availableRect.y += h;
            labelRect.y += h/2f;
            totalRect.y += h; 

            var db = Database;
            Widgets.Label(labelRect, HEADER_LABEL.Translate());
            Widgets.Label(availableRect, DatabaseUtilities.GetStorageString(db.FreeStorage));
            Widgets.Label(totalRect, DatabaseUtilities.GetStorageString(db.TotalStorage));
        }

      

        /// <summary>
        ///     called just before the tab is opened
        /// </summary>
        public override void PreOpen()
        {
            base.PostOpen();
            _rowEntries.Clear();
        }

        private void DrawAnimalsTab(Rect inRect)
        {
            _rowEntries.Clear();
            foreach (PawnKindDef taggedAnimal in Database.TaggedAnimals) _rowEntries.Add(new RowEntry(taggedAnimal));

            DrawTable(inRect, _cachedHeaders[(int) Mode.Animal]);
        }

        private void DrawButton(Rect bRect, Def removeDef)
        {
            const float buttonShrinkFactor = 0.071f;

            float bW = bRect.width * buttonShrinkFactor;
            float dX = bRect.width * (1 - buttonShrinkFactor) / 4;
            var bRectReal = new Rect(bRect)
            {
                width = bW,
                x = bRect.x + dX
            };

            if (Widgets.ButtonImage(bRectReal, PMTexButton.CloseXSmall)) RemoveFromDB(removeDef);
        }

        private void DrawMutationsTab(Rect inRect)
        {
            _rowEntries.Clear(); //TODO fix this 
            foreach (MutationDef mutation in Database.StoredMutations) _rowEntries.Add(new RowEntry(mutation));

            DrawTable(inRect, _cachedHeaders[(int) Mode.Mutations]);
        }

        private void DrawRow(RowHeader header, Rect inRect)
        {
            float tW = inRect.width * 2f;
            float wTxt = tW * DESC_ROW_T_FRACT;
            float wST = tW * STORAGE_INFO_FRACT;
            float wButton = BUTTON_FRACT * tW;
            float x0 = inRect.x;
            float x1 = DESC_ROW_T_FRACT * tW + x0;
            float x2 = TEXT_ROW_FRACT * tW + x0;


            var txtHRect = new Rect(inRect) {width = wTxt};
            var stInfoHRect = new Rect(inRect) {x = x1, width = wST};
            var buttonHRect = new Rect(inRect) {x = x2, width = wButton};
            Widgets.Label(txtHRect, header.defHeader);
            Widgets.Label(stInfoHRect, header.totalHeader);
            Widgets.Label(buttonHRect, header.removeHeader);
        }

        private void DrawRow(RowEntry entry, Rect inRect)
        {
            float tW = inRect.width * 2f;
            float wTxt = tW * DESC_ROW_T_FRACT;
            float wST = tW * STORAGE_INFO_FRACT;
            float wButton = BUTTON_FRACT * tW;
            float x0 = inRect.x;
            float x1 = DESC_ROW_T_FRACT * tW + x0;
            float x2 = TEXT_ROW_FRACT * tW + x0;


            var txtRect = new Rect(inRect) { width = wTxt };
            var stInfoRect = new Rect(inRect) { x = x1, width = wST };
            var buttonRect = new Rect(inRect) { x = x2, width = wButton, height = 10 };

            Widgets.Label(txtRect, entry.label);
            DrawUsageColumn(entry, stInfoRect);
            DrawButton(buttonRect, entry.def);
        }

        private void DrawUsageColumn(RowEntry entry, Rect inRect)
        {
            float w = inRect.width / 2f; 
            Rect cRect = new Rect(inRect) {width = w};

            Widgets.Label(cRect, entry.storageSpaceUsed + STORAGE_SUFFIX);

            string usageStr;
            int totalStorage = Database.TotalStorage;
            if (totalStorage <= 0)
                usageStr = "NaN"; //should be an error message if total storage is 0 ? 
            else
                usageStr = ((float)entry.storageSpaceUsed / totalStorage).ToStringPercent();

            cRect.x += w; 
            Widgets.Label(cRect,  "[" + usageStr + "]");
        }

        private void DrawTable(Rect inRect, RowHeader header)
        {
            inRect.yMin += 10f;
            inRect.yMax += 40f;
            Rect mainView = inRect.ContractedBy(10f);

            var outRect = new Rect(inRect.x, inRect.y, mainView.width, mainView.height - inRect.y - 10f);
            const float rowHeight = 30;
            const float lineWidth = 5;
            const float buffer = 5;
            //draw the header row
            var viewRectHeight = (rowHeight + lineWidth + buffer) / 2f;
            viewRectHeight += (rowHeight + lineWidth) / 2f;
            viewRectHeight += _rowEntries.Count * rowHeight;


            viewRectHeight = Mathf.Max(viewRectHeight, mainView.height); 

            var viewRect = new Rect(mainView.x + mainView.width / 2f + 10f, mainView.y, mainView.width / 2f - 10f - 16f,
                                   viewRectHeight);
            float viewWidth = viewRect.width / 3 - 10f;
            Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);
            try
            {
                


                var font = Text.Font;
                try
                {
                    Text.Font = GameFont.Small;
                    DrawRow(header, viewRect);
                }
                finally
                {
                    Text.Font = font; 
                }                
                
                
                viewRect.y += (rowHeight + lineWidth + buffer) / 2f;

                Widgets.DrawLine(new Vector2(viewRect.x, viewRect.y), new Vector2(viewRect.x + mainView.width, viewRect.y),
                                 Color.black, lineWidth);

                viewRect.y += (rowHeight + lineWidth) / 2f;
                if (_rowEntries.Count == 0) return;

                for (var index = 0; index < _rowEntries.Count; index++)
                {
                    RowEntry rowEntry = _rowEntries[index];
                    Rect rect = viewRect; //TODO fix this 
                    rect.y += index * rowHeight; //go down 1 row? 
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


        private void RemoveFromDB(Def def)
        {
            var db = Find.World.GetComponent<ChamberDatabase>();
            //explicit type casts here are hacky but best way to reuse the gui code 
            if (def is PawnKindDef pkDef)
                db.RemoveFromDatabase(pkDef);
            else if (def is MutationDef mDef)
                db.RemoveFromDatabase(mDef);
            else
                throw new NotImplementedException(nameof(RemoveFromDB) + " is not implemented for " + def.GetType().Name + "!");
        }


        private void SetMode(Mode mode)
        {
            _curMode = mode;
        }

        private readonly struct RowEntry : IEquatable<RowEntry>
        {
            public readonly Def def;
            public readonly string label;
            public readonly int storageSpaceUsed;

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

            /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
            /// <param name="other">An object to compare with this object.</param>
            /// <returns>
            ///     <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise,
            ///     <see langword="false" />.
            /// </returns>
            public bool Equals(RowEntry other)
            {
                return Equals(def, other.def)
                    && label == other.label
                    && storageSpaceUsed == other.storageSpaceUsed;
            }

            /// <summary>Indicates whether this instance and a specified object are equal.</summary>
            /// <param name="obj">The object to compare with the current instance. </param>
            /// <returns>
            ///     <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same
            ///     value; otherwise, <see langword="false" />.
            /// </returns>
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
                    int hashCode = def != null ? def.GetHashCode() : 0;
                    hashCode = (hashCode * 397) ^ (label != null ? label.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ storageSpaceUsed;
                    return hashCode;
                }
            }

            /// <summary>
            ///     Returns a value that indicates whether the values of two
            ///     <see cref="T:Pawnmorph.MainTabWindow_ChamberDatabase.RowEntry" /> objects are equal.
            /// </summary>
            /// <param name="left">The first value to compare.</param>
            /// <param name="right">The second value to compare.</param>
            /// <returns>
            ///     true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise,
            ///     false.
            /// </returns>
            public static bool operator ==(RowEntry left, RowEntry right)
            {
                return left.Equals(right);
            }

            /// <summary>
            ///     Returns a value that indicates whether two <see cref="T:Pawnmorph.MainTabWindow_ChamberDatabase.RowEntry" />
            ///     objects have different values.
            /// </summary>
            /// <param name="left">The first value to compare.</param>
            /// <param name="right">The second value to compare.</param>
            /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
            public static bool operator !=(RowEntry left, RowEntry right)
            {
                return !left.Equals(right);
            }
        }

        private enum Mode
        {
            Animal,
            Mutations
        }
    }
}