
namespace VideoTools
{
    using System;
    using System.IO;
    using Newtonsoft.Json;

    public partial class ShowNameMapEntry
    {
        public static ShowNameMapEntry[] LoadFromFile(string path, bool optional = false)
        {
            var result = Array.Empty<ShowNameMapEntry>();

            if ((optional == false) || File.Exists(path))
            {
                var json = File.ReadAllText(path);

                result = JsonConvert.DeserializeObject<ShowNameMapEntry[]>(json);
            }
            return result;
        }
    }
}
