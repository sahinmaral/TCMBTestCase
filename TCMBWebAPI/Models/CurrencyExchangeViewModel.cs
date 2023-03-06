using Newtonsoft.Json;

using System.Text.Json.Serialization;

namespace TCMBWebAPI.Models
{
    public class CurrencyExchangeViewModel
    {
        [JsonProperty("items")]
        public List<ExchangesPerDate> ExchangesPerDate { get; set; }
    }


}
