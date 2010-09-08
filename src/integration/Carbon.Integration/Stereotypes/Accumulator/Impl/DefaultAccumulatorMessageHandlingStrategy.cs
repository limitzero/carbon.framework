using System;
using System.Collections.Generic;

namespace Carbon.Integration.Stereotypes.Accumulator.Impl
{
    /// <summary>
    /// Default accumulation strategy that will simply collect a list 
    /// of objects based on type and set the batch size limit to the 
    /// maximum (256 items). 
    /// </summary>
    /// <typeparam name="T">Type to be accumulated</typeparam>
    public class DefaultAccumulatorMessageHandlingStrategy<T> :
        AbstractAccumulatorMessageHandlingStrategy<T>
    {
        private static IList<T> _storage = null;

        public DefaultAccumulatorMessageHandlingStrategy()
        {
            MessageBatchSize = BATCH_SIZE_LIMIT;

            if (_storage == null)
                _storage = new List<T>();

            SetStorage(_storage);

            CleanUpAction = ()=>
                {
                    _storage.Clear();
                    _storage = null;
                };

        }
    }
}