namespace Carbon.File.Adapter.Strategies
{
    public interface IFileNameStrategy
    {
        string FileName { get; set; }
        string FileExtension { get; set; }
    }
}