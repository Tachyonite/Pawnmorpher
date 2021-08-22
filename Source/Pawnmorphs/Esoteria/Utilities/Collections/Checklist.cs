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
        public ValueChecklist() { }
        public ValueChecklist(IEnumerable<T> list) : base(list) { }

        public override LookMode LookMode => LookMode.Value;
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
        public DefChecklist() { }
        public DefChecklist(IEnumerable<T> list) : base (list) { }

        public override LookMode LookMode => LookMode.Def;
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
        public IExposableChecklist() { }
        public IExposableChecklist(IEnumerable<T> list) : base(list) { }

        public override LookMode LookMode => LookMode.Deep;
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
        public ILoadReferenceableChecklist() { }
        public ILoadReferenceableChecklist(IEnumerable<T> list) : base(list) { }

        public override LookMode LookMode => LookMode.Reference;
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
        public BodyPartChecklist() { }
        public BodyPartChecklist(IEnumerable<BodyPartRecord> list) : base(list) { }

        public override LookMode LookMode => LookMode.BodyPart;
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
        public TargetInfoChecklist() { }
        public TargetInfoChecklist(IEnumerable<TargetInfo> list) : base(list) { }

        public override LookMode LookMode => LookMode.TargetInfo;
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
        public LocalTargetInfoChecklist() { }
        public LocalTargetInfoChecklist(IEnumerable<LocalTargetInfo> list) : base(list) { }

        public override LookMode LookMode => LookMode.LocalTargetInfo;
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
        public GlobalTargetInfoChecklist() { }
        public GlobalTargetInfoChecklist(IEnumerable<GlobalTargetInfo> list) : base(list) { }

        public override LookMode LookMode => LookMode.GlobalTargetInfo;
    }
}
