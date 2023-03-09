using System.IO;

namespace DDEngine
{
    public interface FileSystem
    {
        public Stream OpenFile(string path);

        public bool Exists(string path);
    }
}
