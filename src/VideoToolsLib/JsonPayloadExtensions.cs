
namespace VideoTools
{
    using System.Linq;
    using Newtonsoft.Json.Linq;

    public static partial class JsonPayloadExtensions
    {
        public static bool TryGetThumbnailStreamIndex(this JsonPayload metadata, out int result)
        {
            bool success = false;
            result = Helpers.InvalidStreamIndex;

            var token = (from s in metadata.GetObject()["streams"]
                         where s["disposition"]["attached_pic"].Value<int>() == 1
                         select s["index"]).FirstOrDefault();

            if (token != null)
            {
                result = token.Value<int>();
                success = true;
            }

            return success;
        }
    }
}
