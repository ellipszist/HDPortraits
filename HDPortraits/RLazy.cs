using System;

namespace HDPortraits
{
    public class RLazy<T>
    {
        private T value = default;
        private bool hasValue = false;
        private Func<T> getter;

        public RLazy(Func<T> Getter)
        {
            this.getter = Getter;
        }


        public T Value
        {
            get
            {
                if (!hasValue)
                {
                    value = getter();
                    hasValue = true;
                }
                return value;
            }
        }

        public void Reset()
        {
            hasValue = false;
        }
        public bool HasValue()
        {
            return hasValue;
        }
    }
}
