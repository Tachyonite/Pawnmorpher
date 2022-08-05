using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pawnmorph.HPatches.Optional
{
    /// <summary>
    /// Designates a harmony patch as optional and lists it in the optional patches options menu.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    internal class OptionalPatchAttribute : Attribute
    {
        /// <summary>
        /// Gets the descriptive tooltip displayed when hovering over the patch in options menu.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the name of the patch member that determines if patch is active.
        /// </summary>
        public string EnableMemberName { get; }

        /// <summary>
        /// Gets whether or not the patch is enabled by default.
        /// </summary>
        public bool DefaultEnabled { get; }

        /// <summary>
        /// Patch title.
        /// </summary>
        public string Caption { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionalPatchAttribute"/> class.
        /// </summary>
        /// <param name="caption">The patch caption in options menu.</param>
        /// <param name="description">Description shown as a tooltip in options menu.</param>
        /// <param name="enabledMember">Name of the settable field or property that contains the value that determines if the patch is active.</param>
        /// <param name="defaultEnabled">Default state of the patch when listed in options menu.</param>
        public OptionalPatchAttribute(string caption, string description, string enabledMember, bool defaultEnabled)
        {
            Caption = caption;
            EnableMemberName = enabledMember;
            Description = description;
            DefaultEnabled = defaultEnabled;
        }
    }
}
