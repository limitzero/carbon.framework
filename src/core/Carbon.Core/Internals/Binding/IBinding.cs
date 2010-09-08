namespace Carbon.Core.Internals.Binding
{
    public interface IBinding
    {
        string InputChannel { get; set; }
        string OutputChannel { get; set; }
        string MethodName { get; set; }
    }
}