using System;
using JetBrains.Annotations;

namespace Pawnmorph.Utilities
{
	/// <summary>
	/// A class to cache generic values rather than calculating them every time.
	/// </summary>
	public class Cached<T>
	{
		private bool _cached;

		[NotNull]
		private readonly Func<T> valueGetter;

		private T _value;

		/// <summary>
		/// Gets the value.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		public T Value
		{
			get
			{
				if (!_cached)
				{
					_value = valueGetter.Invoke();
					_cached = true;
				}
				return (T)_value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Cached{T}"/> class.
		/// </summary>
		/// <param name="valueGetter">The value getter.</param>
		public Cached([NotNull] Func<T> valueGetter)
		{
			this.valueGetter = valueGetter ?? throw new ArgumentNullException(nameof(valueGetter));
			_cached = false;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Cached{T}"/> class.
		/// with an initial value 
		/// </summary>
		/// <param name="valueGetter">The value getter.</param>
		/// <param name="val">The value.</param>
		public Cached([NotNull] Func<T> valueGetter, T val)
		{
			this.valueGetter = valueGetter ?? throw new ArgumentNullException(nameof(valueGetter));
			_value = val;
			_cached = true;
		}

		/// <summary>
		/// Purges the cache and causes the value to be recalculated the next time
		/// it's accessed
		/// </summary>
		public void Recalculate()
		{
			_cached = false;
		}
	}
}
