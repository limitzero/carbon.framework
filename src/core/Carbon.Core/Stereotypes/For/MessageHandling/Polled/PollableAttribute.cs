using System;

namespace Kharbon.Stereotypes.For.MessageHandling.Polled
{
    /// <summary>
    /// Attribute for a message that will broadcasted at a specific interval or a method that will be called 
    /// on a specific interval.
    /// </summary>
    /// <example>
    /// [Polled(2)] or [Polled(new TimeSpan(0, 0, 2)]
    /// public Heartbeat SendHeartbeat()
    /// {
    ///     return new Heartbeat();
    /// }
    /// 
    /// or if wanting to see if a file is present in a directory over a period 
    /// of time: (i.e. check every ten seconds to see if a file is present)
    /// 
    /// [Polled(10)]
    /// public FileInfo GetPayrollFile()
    /// {
    ///     if(Directory(@"C:\inputFiles\payroll").GetFiles() > 0)
    ///       return new FileInfo(Directory(@"C:\inputFiles\payroll").GetFiles[0]);
    /// }
    /// 
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PolledAttribute : Attribute
    {
        ///<summary>
        /// This will indicate that a method can be invoked over a given 
        /// interval (i.e. the method can be called every X seconds)
        ///</summary>
        ///<param name="seconds">The frequency, in seconds, that the method should be polled (i.e. invoked).</param>
        public PolledAttribute(int seconds)
        {
            Interval = TimeSpan.FromSeconds(seconds);
        }


        ///<summary>
        /// This will indicate that a method can be invoked over a given 
        /// interval (i.e. the method can be called every X seconds, Y minutes, Z hours)
        ///</summary>
        ///<param name="interval">The <seealso cref="TimeSpan">time span</seealso> to which the method will be polled (i.e. invoked).</param>
        public PolledAttribute(TimeSpan interval)
        {
            Interval = interval;
        }

        ///<summary>
        /// (Read-Write). The interval to which a method will be polled (i.e. invoked) over a given timeframe.
        ///</summary>
        public TimeSpan Interval { get; set; }
    }
}