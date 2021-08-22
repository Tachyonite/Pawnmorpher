using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace Pawnmorph.Utilities.Collections
{

    /// <summary>
    /// A bundled list and iterator, which can be saved and loaded with Scribe_Deep
    /// Useful to keep track of iteration that may need to be interrupted and saved
    /// partway through, like a hediff that gradually iterates through a list of
    /// body parts.
    /// </summary>
    public abstract class Checklist<T> : IExposable
    {
        private List<T> list;
        private int index;

        /// <summary>
        /// Initializes a new empty checklist
        /// </summary>
        protected Checklist() { }

        /// <summary>
        /// Initializes a new checklist with the given list
        /// </summary>
        protected Checklist(IEnumerable<T> list)
        {
            this.list = list.ToList();
        }

        /// <summary>
        /// The look mode used to save and load the collection entries.
        /// </summary>
        protected abstract LookMode LookMode { get; }

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

    /// <summary>
    /// A bundled list and iterator, which can be saved and loaded with Scribe_Deep.
    /// Useful to keep track of iteration that may need to be interrupted and saved
    /// partway through, like a hediff that gradually iterates through a list of
    /// body parts.
    /// 
    /// This checklist is for lists of simple values, like ints, floats, or strings
    /// 
    /// NOTE: The type constraint can't enforce this, but do NOT use this for
    /// anything but simple values! It won't work! Use one of the other Checklist
    /// types instead.
    /// </summary>
    public sealed class ValueChecklist<T> : Checklist<T>
    {
        /// <summary>
        /// Initializes a new empty checklist
        /// </summary>
        public ValueChecklist() { }

        /// <summary>
        /// Initializes a new checklist with the given list
        /// </summary>
        public ValueChecklist(IEnumerable<T> list) : base(list) { }

        /// <summary>
        /// The look mode used to save and load the collection entries.
        /// </summary>
        protected override LookMode LookMode => LookMode.Value;
    }

    /// <summary>
    /// A bundled list and iterator, which can be saved and loaded with Scribe_Deep.
    /// Useful to keep track of iteration that may need to be interrupted and saved
    /// partway through, like a hediff that gradually iterates through a list of
    /// body parts.
    /// 
    /// This checklist is for lists of Defs
    /// </summary>
    public sealed class DefChecklist<T> : Checklist<T> where T : Def
    {
        /// <summary>
        /// Initializes a new empty checklist
        /// </summary>
        public DefChecklist() { }

        /// <summary>
        /// Initializes a new checklist with the given list
        /// </summary>
        public DefChecklist(IEnumerable<T> list) : base (list) { }

        /// <summary>
        /// The look mode used to save and load the collection entries.
        /// </summary>
        protected override LookMode LookMode => LookMode.Def;
    }

    /// <summary>
    /// A bundled list and iterator, which can be saved and loaded with Scribe_Deep.
    /// Useful to keep track of iteration that may need to be interrupted and saved
    /// partway through, like a hediff that gradually iterates through a list of
    /// body parts.
    /// 
    /// This checklist is for lists of IExposable things
    /// </summary>
    public sealed class IExposableChecklist<T> : Checklist<T> where T : IExposable
    {
        /// <summary>
        /// Initializes a new empty checklist
        /// </summary>
        public IExposableChecklist() { }

        /// <summary>
        /// Initializes a new checklist with the given list
        /// </summary>
        public IExposableChecklist(IEnumerable<T> list) : base(list) { }

        /// <summary>
        /// The look mode used to save and load the collection entries.
        /// </summary>
        protected override LookMode LookMode => LookMode.Deep;
    }

    /// <summary>
    /// A bundled list and iterator, which can be saved and loaded with Scribe_Deep.
    /// Useful to keep track of iteration that may need to be interrupted and saved
    /// partway through, like a hediff that gradually iterates through a list of
    /// body parts.
    /// 
    /// This checklist is for lists of ILoadReferenceable things
    /// </summary>
    public sealed class ILoadReferenceableChecklist<T> : Checklist<T> where T : ILoadReferenceable
    {
        /// <summary>
        /// Initializes a new empty checklist
        /// </summary>
        public ILoadReferenceableChecklist() { }

        /// <summary>
        /// Initializes a new checklist with the given list
        /// </summary>
        public ILoadReferenceableChecklist(IEnumerable<T> list) : base(list) { }

        /// <summary>
        /// The look mode used to save and load the collection entries.
        /// </summary>
        protected override LookMode LookMode => LookMode.Reference;
    }

    /// <summary>
    /// A bundled list and iterator, which can be saved and loaded with Scribe_Deep.
    /// Useful to keep track of iteration that may need to be interrupted and saved
    /// partway through, like a hediff that gradually iterates through a list of
    /// body parts.
    /// 
    /// This checklist is for lists of BodyPartRecords
    /// </summary>
    public sealed class BodyPartChecklist : Checklist<BodyPartRecord>
    {
        /// <summary>
        /// Initializes a new empty checklist
        /// </summary>
        public BodyPartChecklist() { }

        /// <summary>
        /// Initializes a new checklist with the given list
        /// </summary>
        public BodyPartChecklist(IEnumerable<BodyPartRecord> list) : base(list) { }

        /// <summary>
        /// The look mode used to save and load the collection entries.
        /// </summary>
        protected override LookMode LookMode => LookMode.BodyPart;
    }

    /// <summary>
    /// A bundled list and iterator, which can be saved and loaded with Scribe_Deep.
    /// Useful to keep track of iteration that may need to be interrupted and saved
    /// partway through, like a hediff that gradually iterates through a list of
    /// body parts.
    /// 
    /// This checklist is for lists of TargetInfos
    /// </summary>
    public sealed class TargetInfoChecklist : Checklist<TargetInfo>
    {
        /// <summary>
        /// Initializes a new empty checklist
        /// </summary>
        public TargetInfoChecklist() { }

        /// <summary>
        /// Initializes a new checklist with the given list
        /// </summary>
        public TargetInfoChecklist(IEnumerable<TargetInfo> list) : base(list) { }

        /// <summary>
        /// The look mode used to save and load the collection entries.
        /// </summary>
        protected override LookMode LookMode => LookMode.TargetInfo;
    }

    /// <summary>
    /// A bundled list and iterator, which can be saved and loaded with Scribe_Deep.
    /// Useful to keep track of iteration that may need to be interrupted and saved
    /// partway through, like a hediff that gradually iterates through a list of
    /// body parts.
    /// 
    /// This checklist is for lists of LocalTargetInfos
    /// </summary>
    public sealed class LocalTargetInfoChecklist : Checklist<LocalTargetInfo>
    {
        /// <summary>
        /// Initializes a new empty checklist
        /// </summary>
        public LocalTargetInfoChecklist() { }

        /// <summary>
        /// Initializes a new checklist with the given list
        /// </summary>
        public LocalTargetInfoChecklist(IEnumerable<LocalTargetInfo> list) : base(list) { }

        /// <summary>
        /// The look mode used to save and load the collection entries.
        /// </summary>
        protected override LookMode LookMode => LookMode.LocalTargetInfo;
    }

    /// <summary>
    /// A bundled list and iterator, which can be saved and loaded with Scribe_Deep.
    /// Useful to keep track of iteration that may need to be interrupted and saved
    /// partway through, like a hediff that gradually iterates through a list of
    /// body parts.
    /// 
    /// This checklist is for lists of GlobalTargetInfoChecklists
    /// </summary>
    public sealed class GlobalTargetInfoChecklist : Checklist<GlobalTargetInfo>
    {
        /// <summary>
        /// Initializes a new empty checklist
        /// </summary>
        public GlobalTargetInfoChecklist() { }

        /// <summary>
        /// Initializes a new checklist with the given list
        /// </summary>
        public GlobalTargetInfoChecklist(IEnumerable<GlobalTargetInfo> list) : base(list) { }

        /// <summary>
        /// The look mode used to save and load the collection entries.
        /// </summary>
        protected override LookMode LookMode => LookMode.GlobalTargetInfo;
    }
}
