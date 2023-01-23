using Pawnmorph.Chambers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.Genebank.Model
{
	/// <summary>
	/// Abstract base type for a typed genebank entry.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class GenebankEntry<T> : IGenebankEntry, IExposable
	{
		/// <summary>
		/// The wrapped value object
		/// </summary>
		protected T _value;

		/// <summary>
		/// Gets or sets the wrapped object for this genebank entry.
		/// </summary>
		public T Value
		{
			get => _value;
			set => _value = value;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="GenebankEntry{T}"/> class.
		/// </summary>
		/// <param name="value">The underlying object.</param>
		public GenebankEntry(T value)
		{
			_value = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GenebankEntry{T}"/> class.
		/// </summary>
		private GenebankEntry()
		{

		}

		public abstract int GetRequiredStorage();
		public abstract bool CanAddToDatabase(ChamberDatabase database, out string reason);
		public abstract string GetCaption();
		public abstract void ExposeData();

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (_value == null || obj == null)
				return false;

			if (obj is GenebankEntry<T> entry)
				return _value.Equals(entry._value);

			return false;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

	}
}
