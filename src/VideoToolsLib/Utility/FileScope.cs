
namespace VideoTools
{
    using System;
    using System.IO;

    [Flags]
    public enum FileScopePolicy
    {
        None = 0x00,                // Default policy, don't overwrite output files
        WhatIf = 0x01,              // Simulate file operations
        Overwrite = 0x02,         // Overwrite output files
        DeleteOnDispose = 0x04,     // Delete file when FileScope object disposed
    }

    public class FileScope : IDisposable
    {
        private bool _created;

        public string Path { get; set; }
        public FileScopePolicy Policy { get; set; }

        public FileScope(string path)
            : this(path, FileScopePolicy.None)
        {
        }

        public static FileScope Create(string path, bool whatIf = false, bool overwrite = false, bool deleteOnDispose = false)
        {
            FileScopePolicy policy = ((whatIf) ? FileScopePolicy.WhatIf : FileScopePolicy.None)
                                   | ((overwrite) ? FileScopePolicy.Overwrite : FileScopePolicy.None)
                                   | ((deleteOnDispose) ? FileScopePolicy.DeleteOnDispose : FileScopePolicy.None);

            return new FileScope(path, policy);
        }

        public FileScope(string path, FileScopePolicy policy)
        {
            Path = path;
            Policy = policy;
        }

        public bool TryCreate(Func<FileScope, bool, bool> action)
        {
            bool result = (Policy & FileScopePolicy.Overwrite) == FileScopePolicy.Overwrite;
            if (result == false)
            {
                // Only try to create the file if it doesn't exist
                result = (File.Exists(Path) == false);
            }
            if (result)
            {
                var whatIf = (Policy & FileScopePolicy.WhatIf) == FileScopePolicy.WhatIf;

                result = _created = action.Invoke(this, whatIf);
            }

            return result;
        }

        public void Dispose()
        {
            if (_created
                && (Policy & FileScopePolicy.DeleteOnDispose) == FileScopePolicy.DeleteOnDispose
                && String.IsNullOrWhiteSpace(Path) == false)
            {
                if ((Policy & FileScopePolicy.WhatIf) == FileScopePolicy.None)
                {
                    File.Delete(Path);
                }
                Path = null;
            }
        }
    }
}
