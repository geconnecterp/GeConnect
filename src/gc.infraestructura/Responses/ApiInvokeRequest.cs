

using Newtonsoft.Json.Linq;

namespace gc.infraestructura.Responses
{
    public class ApiInvokeRequest
    {
        public string Url { get; set; } = string.Empty;

        public string Method { get; set; } = string.Empty;

        public Dictionary<string, JToken>? Header { get; set; }

        public JToken? Body { get; set; }
    }
}
