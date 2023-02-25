using System;
using UnityEngine;
using Verse;

namespace Pawnmorph.Abilities
{
	/// <summary>
	/// Ability properties.
	/// </summary>
	/// <seealso cref="Verse.IExposable" />
	public class MutationAbilityDef : IExposable
	{
		/// <summary>
		/// The class that contains the logic for the ability. Must be a MutationAbility type.
		/// </summary>
		public Type abilityClass;

		/// <summary>
		/// The ability caption.
		/// </summary>
		public string label;

		/// <summary>
		/// The ability description.
		/// </summary>
		public string description;

		/// <summary>
		/// Path to the icon that should be displayed for the ability button.
		/// </summary>
		public string iconPath;

		/// <summary>
		/// The total cooldown in ticks.
		/// </summary>
		public int cooldown;

		private Texture2D _iconTextureCache;

		/// <summary>
		/// The texture for the ability icon
		/// </summary>
		public Texture2D IconTexture
		{
			get
			{
				CacheTexture();
				return _iconTextureCache;
			}
		}

		/// <summary>
		/// Loads the icon texture into the texture cache
		/// </summary>
		public void CacheTexture()
		{
			if (_iconTextureCache == null)
				_iconTextureCache = ContentFinder<Texture2D>.Get(iconPath);
		}


		/// <summary>
		/// Exposes the data for serialization and deserialization.
		/// </summary>
		public void ExposeData()
		{
			Scribe_Values.Look<Type>(ref abilityClass, nameof(abilityClass));
			Scribe_Values.Look<string>(ref label, nameof(label));
			Scribe_Values.Look<string>(ref description, nameof(description));
			Scribe_Values.Look<string>(ref iconPath, nameof(iconPath));
			Scribe_Values.Look<int>(ref cooldown, nameof(cooldown));
		}
	}
}
