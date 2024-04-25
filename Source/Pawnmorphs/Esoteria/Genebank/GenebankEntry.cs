using Pawnmorph.Chambers;
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
		/// Gets the wrapped object for this genebank entry.
		/// </summary>
		public T Value => _value;

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

		/// <summary>
		/// Gets the required storage capacity needed to store this value in the genebank.
		/// </summary>
		/// <returns></returns>
		public abstract int GetRequiredStorage();

		/// <summary>
		/// Additional requirements to check if this object can be added to the genebank.
		/// </summary>
		/// <param name="database">The genebank to be added to.</param>
		/// <param name="reason">The reason if it fails.</param>
		public abstract bool CanAddToDatabase(ChamberDatabase database, out string reason);

		/// <summary>
		/// Gets the caption.
		/// </summary>
		/// <returns></returns>
		public abstract string GetCaption();

		/// <inheritdoc/>
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

		bool IGenebankEntry.IsValid()
		{
			return _value != null;
		}
	}
}
