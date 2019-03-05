
namespace VideoTools
{
    using System;
    using System.IO;

    public class FileScope : IDisposable
    {
        public string Path { get; set; }
        public bool DeleteOnDispose { get; set; }

        public FileScope(string path)
        {
            Path = path;
        }

        public bool IfNotExist(Action<string> action)
        {
            bool result = File.Exists(Path);
            if (result == false)
            {
                action.Invoke(Path);
            }

            return result;
        }

        public void Dispose()
        {
            if (DeleteOnDispose && String.IsNullOrWhiteSpace(Path))
            {
                Helpers.RemoveFile(Path);
                Path = null;
            }
        }
    }
}
