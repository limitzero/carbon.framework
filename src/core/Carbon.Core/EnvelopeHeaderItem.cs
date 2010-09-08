using System;

namespace Carbon.Core
{
    public class EnvelopeHeaderItem : IEnvelopeHeaderItem
    {
        public string Name { get; private set; }
        public object Value { get; private set; }

        public EnvelopeHeaderItem(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public T GetValue<T>()
        {
            var result = default(T);

            try
            {
                result = (T)Value;
            }
            catch
            {
            }

            return result;
        }
    }
}