using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Pawnmorph.Hediffs.Utility
{

    /// <summary>
    /// A class that represents a (savable) checklist.  Used to handle stateful
    /// operations like iterating through a list of body parts.  Can be saved and
    /// loaded.
    /// 
    /// Don't add any logic to this class, since instances will be thrown away and
    /// recreated. Instead, add the logic to the components that create them.
    /// </summary>
    public abstract class Checklist<T> : IExposable
    {
        private List<T> list;
        private int index;

        // empty constructor for loading purposes
        protected Checklist() { }

        protected Checklist(IEnumerable<T> list)
        {
            this.list = list.ToList();
        }

        /// <summary>
        /// The look mode used to save and load the collection entries.
        /// </summary>
        public abstract LookMode LookMode { get; }

        /// <summary>
        /// Gets the current entry in this checklist, or the default value if
        /// we reached the end
        /// </summary>
        public T GetCurrentEntry()
        {
            if (list == null)
            {
                Log.Error("Checklist had no parts list");
                return default(T);
            }

            if (index >= list.Count)
                return default(T);
            return list[index];
        }

        /// <summary>
        /// Advances the checklist to the next entry
        /// </summary>
        /// <returns>true if more entries exist, false if the checklist is finished</returns>
        public bool NextEntry()
        {
            index++;
            return index < list.Count;
        }

        /// <summary>
        /// Resets this checklist to the beginning
        /// </summary>
        public void Reset() => index = 0;

        /// <summary>
        /// Advances the checklist to the next entry, or resets it if we reached the end
        /// </summary>
        public void NextEntryOrReset()
        {
            if (!NextEntry()) Reset();
        }

        /// <summary>
        /// Exposes data to be saved/loaded from XML upon saving the game
        /// </summary>
        public void ExposeData()
        {
            Scribe_Collections.Look(ref list, nameof(list), LookMode);
            Scribe_Values.Look(ref index, nameof(index));
        }
    }
}
