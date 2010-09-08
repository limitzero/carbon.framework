using System;

namespace Carbon.Core
{
    public interface IEnvelopeBody
    {
        /// <summary>
        /// (Read-Only). The current payload for the message.
        /// </summary>
        object Payload { get; }

        /// <summary>
        /// This will retreive the payload for the current message
        /// and cast it to a user-defined type for inspection.
        /// </summary>
        /// <typeparam name="T">Type to cast the payload to.</typeparam>
        /// <returns></returns>
        T GetPayload<T>();

        /// <summary>
        /// This will set the payload for the current message.
        /// </summary>
        /// <param name="payload">Message payload to set.</param>
        void SetPayload(object payload);
    }
}