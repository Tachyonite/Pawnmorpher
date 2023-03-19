// SapientVarients.cs modified by Iron Wolf for Pawnmorph on 12/22/2019 8:10 AM
// last updated 12/22/2019  8:10 AM

using System;
using JetBrains.Annotations;

namespace Pawnmorph.FormerHumans
{
	/// <summary>
	/// simple class for storing several 'variant' of things for different sapient levels 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SapientVariants<T>
	{
		/// <summary>
		/// The sapient variant
		/// </summary>
		public T sapient;
		/// <summary>
		/// The mostly sapient variant
		/// </summary>
		public T mostlySapient;
		/// <summary>
		/// The conflicted variant
		/// </summary>
		public T conflicted;
		/// <summary>
		/// The mostly feral variant
		/// </summary>
		public T mostlyFeral;
		/// <summary>
		/// The feral variant
		/// </summary>
		public T feral;
		/// <summary>
		/// The permanently feral variant
		/// </summary>
		public T permanentlyFeral;

		/// <summary>
		/// Gets or sets the <see ref="T"/> with the specified key.
		/// </summary>
		/// <value>
		/// </value>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// key - null
		/// or
		/// key - null
		/// </exception>
		[CanBeNull]
		public T this[SapienceLevel key]
		{
			get
			{
				switch (key)
				{
					case SapienceLevel.Sapient:
						return sapient;
					case SapienceLevel.MostlySapient:
						return mostlySapient;
					case SapienceLevel.Conflicted:
						return conflicted;
					case SapienceLevel.MostlyFeral:
						return mostlyFeral;
					case SapienceLevel.Feral:
						return feral;
					case SapienceLevel.PermanentlyFeral:
						return permanentlyFeral;
					default:
						throw new ArgumentOutOfRangeException(nameof(key), key, null);
				}
			}
			set
			{
				switch (key)
				{
					case SapienceLevel.Sapient:
						sapient = value;
						break;
					case SapienceLevel.MostlySapient:
						mostlySapient = value;
						break;
					case SapienceLevel.Conflicted:
						conflicted = value;
						break;
					case SapienceLevel.MostlyFeral:
						mostlyFeral = value;
						break;
					case SapienceLevel.Feral:
						feral = value;
						break;
					case SapienceLevel.PermanentlyFeral:
						permanentlyFeral = value;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(key), key, null);
				}
			}
		}


	}
}