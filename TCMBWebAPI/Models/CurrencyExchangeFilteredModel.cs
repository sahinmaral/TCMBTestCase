namespace TCMBWebAPI.Models
{
    class CurrencyExchangeFilteredModel
    {
        public string ExchangeRecordDate { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public decimal CurrencyBalance { get; set; }
    }
}
