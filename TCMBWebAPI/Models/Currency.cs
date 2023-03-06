

using Newtonsoft.Json;

namespace TCMBWebAPI.Models
{
    public class Currency
    {
        [JsonProperty("SERIE_CODE")]
        public string Code { get; set; }
        [JsonProperty("SERIE_NAME")]
        public string Name { get; set; }
        [JsonIgnore]
        public string Balance { get; set; }
    }
}
