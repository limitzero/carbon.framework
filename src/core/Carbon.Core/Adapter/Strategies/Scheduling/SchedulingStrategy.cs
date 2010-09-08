namespace Carbon.Core.Adapter.Strategies.Scheduling
{
    public class SchedulingStrategy : ISchedulingStrategy
    {
        public int Interval { get; set;}

        public SchedulingStrategy()
            :this(1)
        {

        }

        public SchedulingStrategy(int interval)
        {
            this.Interval = interval;
        }
    }
}