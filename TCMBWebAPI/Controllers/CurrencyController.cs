using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;

using TCMBWebAPI.Models;

namespace TCMBWebAPI.Controllers
{


    [Route("api/currency")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        public CurrencyController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        [HttpGet("getAllCurrencies")]
        public async Task<IActionResult> GetAllCurrencies()
        {
            string uri = "https://evds2.tcmb.gov.tr/service/evds/series=TP.DK.USD.A.YTL-TP.DK.USD.S.YTL-TP.DK.EUR.A.YTL-TP.DK.EUR.S.YTL-TP.DK.AED.A.YTL-TP.DK.AED.S.YTL-TP.DK.AUD.A.YTL-TP.DK.AUD.S.YTL-TP.DK.AZN.A.YTL-TP.DK.AZN.S.YTL-TP.DK.BGN.A.YTL-TP.DK.BGN.S.YTL-TP.DK.CNY.A.YTL-TP.DK.CNY.S.YTL-TP.DK.DKK.A.YTL-TP.DK.DKK.S.YTL-TP.DK.KRW.A.YTL-TP.DK.KRW.S.YTL-TP.DK.GBP.A.YTL-TP.DK.GBP.S.YTL-TP.DK.IRR.A.YTL-TP.DK.IRR.S.YTL-TP.DK.SEK.A.YTL-TP.DK.SEK.S.YTL-TP.DK.CHF.A.YTL-TP.DK.CHF.S.YTL-TP.DK.JPY.A.YTL-TP.DK.JPY.S.YTL-TP.DK.CAD.A.YTL-TP.DK.CAD.S.YTL-TP.DK.QAR.A.YTL-TP.DK.QAR.S.YTL-TP.DK.KWD.A.YTL-TP.DK.KWD.S.YTL-TP.DK.NOK.A.YTL-TP.DK.NOK.S.YTL-TP.DK.PKR.A.YTL-TP.DK.PKR.S.YTL-TP.DK.RON.A.YTL-TP.DK.RON.S.YTL-TP.DK.RUB.A.YTL-TP.DK.RUB.S.YTL-TP.DK.SAR.A.YTL-TP.DK.SAR.S.YTL&startDate=03-02-2023&endDate=05-03-2023&type=json&key=qMrxRCCCqO";
            using var httpResponse = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

            httpResponse.EnsureSuccessStatusCode(); // throws if not 200-299

            if (httpResponse.Content is object && httpResponse.Content.Headers.ContentType.MediaType == "application/json")
            {
                using var contentStream = await httpResponse.Content.ReadAsStreamAsync();

                using var streamReader = new StreamReader(contentStream);
                using var jsonReader = new JsonTextReader(streamReader);

                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                CurrencyExchangeViewModel currencyExchangeViewModel;

                currencyExchangeViewModel = serializer.Deserialize<CurrencyExchangeViewModel>(jsonReader);

                List<Currency> currencies = await GetCurrencyDetailAsync();

                Dictionary<string, List<CurrencyExchangeFilteredModel>> currencyMaximumExchangesDictionary = new Dictionary<string, List<CurrencyExchangeFilteredModel>>();

                foreach (var currency in currencies)
                {
                    var filtered = currencyExchangeViewModel.ExchangesPerDate.FindAll(x => x.TP_DK_USD_A_YTL != null).Select(x => new CurrencyExchangeFilteredModel()
                    {
                        ExchangeRecordDate = x.ExchangeRecordDate,
                        CurrencyCode = x.GetType().GetProperty(currency.Code.Replace('.', '_')).Name,
                        CurrencyName = currencies.Single(k=> k.Code.Replace('.','_') == x.GetType().GetProperty(currency.Code.Replace('.', '_')).Name).Name,
                        CurrencyBalance = Convert.ToDecimal(x.GetType().GetProperty(currency.Code.Replace('.', '_')).GetValue(x, null).ToString()),
                    }).OrderByDescending(r => r.GetType().GetProperty("CurrencyBalance").GetValue(r, null)).Take(5).ToList();

                    currencyMaximumExchangesDictionary.Add(currency.Code, filtered);
                }

                return Ok(currencyMaximumExchangesDictionary);
            }
            else
            {
                return BadRequest("HTTP Response was invalid and cannot be deserialised.");
            }
        }

        [NonAction]
        public async Task<List<Currency>> GetCurrencyDetailAsync(){

            var propertyInfos = new ExchangesPerDate().GetType().GetProperties();

            List<Currency> currencies = new List<Currency>();

            foreach (var currencyPropertyInfo in propertyInfos)
            {
                if(currencyPropertyInfo.Name == "ExchangeRecordDate")
                {
                    continue;
                }     

                string uri = $"https://evds2.tcmb.gov.tr/service/evds/serieList/key=qMrxRCCCqO&type=json&code={currencyPropertyInfo.Name.Replace("_", ".")}";
                using var httpResponseSecond = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

                if (httpResponseSecond.Content is object && httpResponseSecond.Content.Headers.ContentType.MediaType == "application/json")
                {
                    using var contentStreamSecond = await httpResponseSecond.Content.ReadAsStreamAsync();

                    using var streamReaderSecond = new StreamReader(contentStreamSecond);
                    using var jsonReaderSecond = new JsonTextReader(streamReaderSecond);

                    Newtonsoft.Json.JsonSerializer serializerSecond = new Newtonsoft.Json.JsonSerializer();
                    Currency currency = serializerSecond.Deserialize<List<Currency>>(jsonReaderSecond).First();
                    currencies.Add(currency);
                }
                else
                {
                    throw new BadHttpRequestException("HTTP Response was invalid and cannot be deserialised.");
                }
            }

            return currencies;
        }
    }
}
