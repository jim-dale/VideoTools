
namespace VideoTools
{
    using System;
    using System.IO;

    [Flags]
    public enum FileScopePolicy
    {
        None = 0x00,                // Default policy, don't overwrite output files
        ForceCreate = 0x01,         // Overwrite output files
        DeleteOnDispose = 0x02,     // Delete file when FileScope object disposed
        WhatIf = 0x04,              // Simulate file operations
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

        public static FileScope Create(string path, bool forceCreate = false, bool deleteOnDispose = false, bool whatIf = false)
        {
            FileScopePolicy policy = ((forceCreate) ? FileScopePolicy.ForceCreate : FileScopePolicy.None)
                | ((deleteOnDispose) ? FileScopePolicy.DeleteOnDispose : FileScopePolicy.None)
                | ((whatIf) ? FileScopePolicy.WhatIf : FileScopePolicy.None);

            return new FileScope(path, policy);
        }

        public FileScope(string path, FileScopePolicy policy)
        {
            Path = path;
            Policy = policy;
        }

        public bool TryCreate(Func<FileScope, bool, bool> action)
        {
            bool result = (Policy & FileScopePolicy.ForceCreate) == FileScopePolicy.ForceCreate;
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
                    Helpers.RemoveFile(Path);
                }
                Path = null;
            }
        }
    }
}
