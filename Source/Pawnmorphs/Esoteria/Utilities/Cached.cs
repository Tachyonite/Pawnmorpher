using System;
namespace Pawnmorph.Utilities
{
    /// <summary>
    /// A class to cache generic values rather than calculating them every time.
    /// </summary>
    public class Cached<T> where T : struct
    {

        private readonly Func<T> valueGetter;

        private T? _value;
        public T Value
        {
            get
            {
                if (_value == null) _value = valueGetter.Invoke();
                return (T)_value;
            }
        }

        public Cached(Func<T> valueGetter)
        {
            this.valueGetter = valueGetter;
        }

        /// <summary>
        /// Purges the cache and causes the value to be recalculated the next time
        /// it's accessed
        /// </summary>
        public void Recalculate()
        {
            _value = null;
        }
    }
}
