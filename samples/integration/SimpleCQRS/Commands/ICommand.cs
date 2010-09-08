using System;

namespace SimpleCQRS.Commands
{
    public interface ICommand<TKey>
    {
        TKey Id { get; }
    }

    public class Command : ICommand<Guid>
    {
        public Guid Id
        {
            get;
            private set;
        }

    }
}