
namespace VideoTools
{
    using System;
    using System.IO;

    [Flags]
    public enum DirectoryScopePolicy
    {
        None = 0x00,                // Default policy, don't overwrite output files
        WhatIf = 0x01,              // Simulate file operations
    }

    public class DirectoryScope
    {
        private bool _suppressCreate;

        public string Path { get; private set; }
        public DirectoryScopePolicy Policy { get; set; }

        public static DirectoryScope Create(string path, bool whatIf = false)
        {
            DirectoryScopePolicy policy = (whatIf) ? DirectoryScopePolicy.WhatIf : DirectoryScopePolicy.None;

            return new DirectoryScope(path, policy);
        }

        public DirectoryScope(string path)
            : this(path, DirectoryScopePolicy.None)
        {
        }

        public DirectoryScope(string path, DirectoryScopePolicy policy)
        {
            Path = path;
            Policy = policy;
        }

        public DirectoryScope ExpandEnvironmentVariables()
        {
            if (String.IsNullOrEmpty(Path) == false)
            {
                Path = Environment.ExpandEnvironmentVariables(Path);
            }
            return this;
        }

        public DirectoryScope FormatWith(object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (String.IsNullOrEmpty(Path) == false)
            {
                Path = Path.FormatWith(source);
            }
            return this;
        }

        public DirectoryScope EnsureCreated()
        {
            bool whatIf = (Policy & DirectoryScopePolicy.WhatIf) == DirectoryScopePolicy.WhatIf;

            if (_suppressCreate == false && whatIf == false && String.IsNullOrWhiteSpace(Path) == false)
            {
                Directory.CreateDirectory(Path);
            }
            _suppressCreate = true;

            return this;
        }
    }
}
