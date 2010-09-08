namespace Domain
{
    public interface IAggregateRoot
    {
        /// <summary>
        /// (Read-Write). The current version of the aggregate root.
        /// </summary>
        int Version { get; set; }

        /// <summary>
        /// (Read-Write). The name of the aggregate root (typically the domain object).
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// (Read-Write). The current event that caused a change in the aggregate root.
        /// </summary>
        string Event { get; set; }
    }
}