namespace Carbon.File.Adapter.Strategies
{
    public class DefaultFileNameStrategy : IFileNameStrategy
    {
        public string FileName { get; set; }
        public string FileExtension { get; set; }

       
        public DefaultFileNameStrategy()
        {
            FileName = string.Concat("FILE-",System.Guid.NewGuid().ToString());
            FileExtension = ".txt";
        }

        public DefaultFileNameStrategy(string name, string extension)
        {
            FileName = name;
            FileExtension = extension;
        }
    }
}