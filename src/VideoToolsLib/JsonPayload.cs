
namespace VideoTools
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class JsonPayload
    {
        private JObject _object;
        public string Text { get; set; }

        public JsonPayload(string text)
        {
            Text = text;
        }

        public void Reset()
        {
            _object = null;
            Text = null;
        }

        public bool TryGetValue<T>(string path, out T value)
        {
            value = default(T);
            var result = false;

            var obj = GetObject();
            var token = obj.SelectToken(path);
            if (token != null)
            {
                value = token.Value<T>();
                result = true;
            }

            return result;
        }

        public JObject GetObject()
        {
            if (_object is null)
            {
                _object = JsonConvert.DeserializeObject<JObject>(Text);
            }

            return _object;
        }
    }
}
