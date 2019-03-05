
namespace VideoTools
{
    using System.Collections.Generic;

    public class ConsoleClientFactory
    {
        public IDictionary<string, ConsoleClient> _items { get; set; } = new Dictionary<string, ConsoleClient>();

        public void RegisterInstance(string name, ConsoleClient item)
        {
            _items.Add(name, item);
        }

        public ConsoleClient GetInstance(string name)
        {
            return _items[name];
        }
    }
}
