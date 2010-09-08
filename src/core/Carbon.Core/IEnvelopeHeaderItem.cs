namespace Carbon.Core
{
    public interface IEnvelopeHeaderItem
    {
        string Name { get; }
        object Value { get; }
        T GetValue<T>();
    }
}