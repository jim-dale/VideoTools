
namespace VideoTools
{
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;

    public partial class ShowNameCorrection
    {
        public static IList<ShowNameCorrection> LoadFromFile(string path)
        {
            var json = File.ReadAllText(path);

            var result = JsonConvert.DeserializeObject<IList<ShowNameCorrection>>(json);

            return result;
        }
    }
}
