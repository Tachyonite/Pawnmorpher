using System;
using Verse;

namespace Pawnmorph.Utilities
{
	/// <summary>
	/// Object used to cache a value based on age in ticks.
	/// </summary>
	/// <typeparam name="T">The type of value cached.</typeparam>
	public class TimedCache<T>
	{
		/// <summary>
		/// Cache status.
		/// </summary>
		public enum CacheStatus : byte
		{
			/// <summary>
			/// The cached value has not yet been stored.
			/// </summary>
			Unknown = 0,

			/// <summary>
			/// The cached value is cached.
			/// </summary>
			Cached = 1,

			/// <summary>
			/// The cached value has been queued for update.
			/// </summary>
			Queued = 2,
		}

		private TickManager _tickManager;
		private T _value;
		private readonly Func<T> _valueGetter;
		private int _timestamp;
		private CacheStatus _cachedStatus;

		/// <summary>
		/// Object containing the sender cache, old value and new value.
		/// </summary>
		/// <param name="sender">The cache containing the changed value.</param>
		/// <param name="oldValue">The old value.</param>
		/// <param name="newValue">The new value.</param>
		public delegate void ValueChangedHandler(TimedCache<T> sender, T oldValue, T newValue);

		/// <summary>
		/// Occurs when the cached value changes.
		/// </summary>
		public event ValueChangedHandler ValueChanged;

		/// <summary>
		/// Timestamp in ticks for when the stat was last recalculated.
		/// </summary>
		public int Timestamp => _timestamp;

		/// <summary>
		/// Gets the cached value.
		/// </summary>
		/// <param name="maxAge">The maximum age in ticks before an update is queued.</param>
		/// <returns></returns>
		public T GetValue(int maxAge)
		{
			// If stat has not already been queued for update, then check if it should be updated.
			if (_cachedStatus != CacheStatus.Queued)
			{
				if (_cachedStatus == CacheStatus.Unknown)
					Update();
				else
				{
					// If stat is older than age limit, recalculate.
					if (_tickManager.TicksGame - _timestamp > maxAge)
					{
						QueueUpdate();
					}
				}
			}
			return _value;
		}

		/// <summary>
		/// Queues an update of the cached value on the LongEventHandler.
		/// </summary>
		public void QueueUpdate()
		{
			if (_cachedStatus != CacheStatus.Queued)
			{
				if (_tickManager == null)
				{
					_cachedStatus = CacheStatus.Unknown;
					return;
				}

				_cachedStatus = CacheStatus.Queued;
				LongEventHandler.ExecuteWhenFinished(Update);
			}
		}

		/// <summary>
		/// Immediately updates the cached value.
		/// </summary>
		public void Update()
		{
			T oldValue = _value;
			_value = _valueGetter.Invoke();

			if (_tickManager == null && Current.ProgramState != ProgramState.Entry)
				_tickManager = Find.TickManager;

			if (_tickManager != null)
			{
				_timestamp = _tickManager.TicksGame;
				_cachedStatus = CacheStatus.Cached;

				if (oldValue.Equals(_value) == false)
					ValueChanged?.Invoke(this, oldValue, _value);
			}
		}


		/// <summary>
		/// Initializes a new instance of timed cache.
		/// </summary>
		/// <param name="valueGetter">The callback to update the cached value.</param>
		public TimedCache(Func<T> valueGetter)
		{
			if (Current.ProgramState != ProgramState.Entry)
				_tickManager = Find.TickManager;

			_cachedStatus = CacheStatus.Unknown;
			_valueGetter = valueGetter;
		}

		/// <summary>
		/// Initializes a new instance of timed cache with a default value.
		/// </summary>
		/// <param name="valueGetter">The callback to update the cached value.</param>
		/// <param name="initialValue">The initial cached value.</param>
		public TimedCache(Func<T> valueGetter, T initialValue)
			: this(valueGetter)
		{
			_value = initialValue;
			if (_tickManager != null)
			{
				_timestamp = _tickManager.TicksGame;
				_cachedStatus = CacheStatus.Cached;
			}
		}

		public void Offset(int offset)
		{
			_timestamp += offset;
		}
	}
}
