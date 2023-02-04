using System;
using Verse;

namespace Pawnmorph.Utilities
{
    internal class TimedCache<T>
    {
        private TickManager _tickManager;
        private T _value;
        private readonly Func<T> _valueGetter;
        private int _timestamp;
        private bool _requestedUpdate;

        public delegate void ValueChangedHandler(TimedCache<T> sender, T oldValue, T newValue);
        public event ValueChangedHandler ValueChanged;

        /// <summary>
        /// Timestamp in ticks for when the stat was last recalculated.
        /// </summary>
        public int Timestamp => _timestamp;

        public bool RequestedUpdate => _requestedUpdate;

        public T GetValue(int maxAge)
        {
            // If stat has not already been queued for update, then check if it should be updated.
            if (_requestedUpdate == false)
            {
                // If stat is older than age limit, recalculate.
                if (_tickManager.TicksGame - _timestamp > maxAge)
                {
                    QueueUpdate();
                }
            }
            return _value;
        }

        public void QueueUpdate()
        {
            if (_requestedUpdate == false)
            {
                _requestedUpdate = true;
                LongEventHandler.ExecuteWhenFinished(Update);
            }
        }

        public void Update()
        {
            T oldValue = _value;
            _value = _valueGetter.Invoke();
            _timestamp = _tickManager.TicksGame;
            _requestedUpdate = false;

            if (oldValue.Equals(_value) == false)
                ValueChanged?.Invoke(this, oldValue, _value);
        }

        public TimedCache(Func<T> valueGetter)
        {
            _tickManager = Find.TickManager;
            _timestamp = _tickManager.TicksGame;
            _requestedUpdate = false;
            _valueGetter = valueGetter;
        }

        public TimedCache(Func<T> valueGetter, T initialValue)
            : this(valueGetter)
        {
            _value = initialValue;
        }

        public void Offset(int offset)
        {
            _timestamp += offset;
        }
    }
}
