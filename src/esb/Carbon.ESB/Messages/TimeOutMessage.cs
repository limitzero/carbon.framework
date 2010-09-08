using System;
using Carbon.Core.Stereotypes.For.Components.Message;
using System.Xml.Serialization;

namespace Carbon.ESB.Messages
{
    [Message]
    public class TimeoutMessage
    {
        /// <summary>
        /// (Read-Write). The unqiue identifier for the current timeout message.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// (Read-Write). Time for the message to be singled that it has "timed-out".
        /// </summary>
        public DateTime At { get; set; }

        /// <summary>
        /// (Read-Write). Interval to delay message delivery.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// (Read-Write). The channel to send the message to. 
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// (Read-Write). The uri of the channel adapter to send the message to. 
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// (Read-Write). The message that will be sent after the time out period.
        /// </summary>
        public object DelayedMessage { get; set;}

        /// <summary>
        /// (Read-Write). The name of the message that is marked for delay transmission.
        /// </summary>
        public string DelayedMessageType { get; set; }

        /// <summary>
        /// (Read-Write). Date/time the timeout message was requested.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// default .ctor for serialization engine
        /// </summary>
        public TimeoutMessage()
        {
            
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="duration">Time that the message should be held from processing</param>
        /// <param name="message">Message to hold from processing.</param>
        public TimeoutMessage(TimeSpan duration, object message)
        {
            Id = Guid.NewGuid();
            Duration = duration;
            Created = System.DateTime.Now;
            At = CreateDateTimeFromTimespan(duration);
            DelayedMessage = message;
            DelayedMessageType = message.GetType().FullName;
        }

        public bool HasExpired()
        {
            return DateTime.Now > this.At;
        }

        private DateTime CreateDateTimeFromTimespan(TimeSpan span)
        {
            var dateTime = this.Created;
            dateTime = dateTime.AddDays(span.Days);
            dateTime = dateTime.AddHours(span.Hours);
            dateTime = dateTime.AddMinutes(span.Minutes);
            dateTime = dateTime.AddSeconds(span.Seconds);
            dateTime = dateTime.AddMilliseconds(span.Milliseconds);
            return dateTime;
        }

    }
}