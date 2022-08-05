using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pawnmorph.HPatches.Optional
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    internal class OptionalPatchAttribute : Attribute
    {
        public string Description { get; }
        public string EnableMemberName { get; }
        public bool DefaultEnabled { get; }
        public string Caption { get; }

        public OptionalPatchAttribute(string caption, string description, string enabledMember, bool defaultEnabled)
        {
            Caption = caption;
            EnableMemberName = enabledMember;
            Description = description;
            DefaultEnabled = defaultEnabled;
        }
    }
}
