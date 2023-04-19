using System;
using Verse;

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
		/// Gets the translated descriptive tooltip displayed when hovering over the patch in options menu.
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
		/// Gets translated patch title.
		/// </summary>
		public string Caption { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="OptionalPatchAttribute"/> class.
		/// </summary>
		/// <param name="captionKey">Translation key to use for caption in options menu.</param>
		/// <param name="descriptionKey">Translation key to use as tooltip in options menu.</param>
		/// <param name="enabledMember">Name of the settable field or property that contains the value that determines if the patch is active.</param>
		/// <param name="defaultEnabled">Default state of the patch when listed in options menu.</param>
		public OptionalPatchAttribute(string captionKey, string descriptionKey, string enabledMember, bool defaultEnabled)
		{
			Caption = captionKey.Translate();
			Description = descriptionKey.Translate();
			EnableMemberName = enabledMember;
			DefaultEnabled = defaultEnabled;
		}
	}
}
