namespace CTLite
{
    public class CompositeUploadedFile
    {
        private readonly byte[] _content = null;

        public CompositeUploadedFile(string name, string filename, byte[] content, string contentType)
        {
            Name = name;
            FileName = filename;
            _content = content;
            ContentType = contentType;
        }

        public string FileName { get; }
        public string Name { get; }
        public byte[] GetContent()
        {
            return _content;
        }
        public string ContentType { get; }
    }
}
