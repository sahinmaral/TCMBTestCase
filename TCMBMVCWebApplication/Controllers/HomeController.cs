using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

using TCMBMVCWebApplication.Models;

namespace TCMBMVCWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        public HomeController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IActionResult ErrorPage(string error)
        {
            return View(error);
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("/exportExcel")]
        public async Task<IActionResult> MaximumFiveDaysExchangesExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                string uri = "https://localhost:7162/api/currency/getAllCurrencies";
                using var httpResponse = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

                httpResponse.EnsureSuccessStatusCode(); // throws if not 200-299

                if (httpResponse.Content is object && httpResponse.Content.Headers.ContentType.MediaType == "application/json")
                {
                    var worksheet = workbook.Worksheets.Add("Kur Verileri");
                    worksheet.Cell(1,1).Value = "Kur Degiskeni";
                    worksheet.Cell(1,2).Value = "Birinci En Yüksek";
                    worksheet.Cell(1,3).Value = "İkinci En Yüksek";
                    worksheet.Cell(1,4).Value = "Üçüncü En Yüksek";
                    worksheet.Cell(1,5).Value = "Dördüncü En Yüksek";
                    worksheet.Cell(1,6).Value = "Beşinci En Yüksek";

                    using var contentStream = await httpResponse.Content.ReadAsStreamAsync();

                    using var streamReader = new StreamReader(contentStream);
                    using var jsonReader = new JsonTextReader(streamReader);

                    Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                    var serializedDictionary = serializer.Deserialize<Dictionary<string , List<CurrencyExchangeFilteredViewModel>>>(jsonReader);

                    int rowCount = 2;
                    foreach (var currencyExchange in serializedDictionary.Keys)
                    {
                        worksheet.Cell(rowCount,1).Value = serializedDictionary[currencyExchange][0].CurrencyName;
                        worksheet.Cell(rowCount,2).Value = $"{serializedDictionary[currencyExchange][0].ExchangeRecordDate} - {serializedDictionary[currencyExchange][0].CurrencyBalance}";
                        worksheet.Cell(rowCount,3).Value = $"{serializedDictionary[currencyExchange][1].ExchangeRecordDate} - {serializedDictionary[currencyExchange][1].CurrencyBalance}";
                        worksheet.Cell(rowCount,4).Value = $"{serializedDictionary[currencyExchange][2].ExchangeRecordDate} - {serializedDictionary[currencyExchange][2].CurrencyBalance}";
                        worksheet.Cell(rowCount,5).Value = $"{serializedDictionary[currencyExchange][3].ExchangeRecordDate} - {serializedDictionary[currencyExchange][3].CurrencyBalance}";
                        worksheet.Cell(rowCount,6).Value = $"{serializedDictionary[currencyExchange][4].ExchangeRecordDate} - {serializedDictionary[currencyExchange][4].CurrencyBalance}";
                        rowCount++;
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content,"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","KurDegiskenleri.xlsx");
                    }
                }
                else
                {
                    return RedirectToAction(nameof(ErrorPage), new {error = "HTTP Response was invalid and cannot be deserialised."});
                }
            }
        }

        [Route("/showChart")]
        public IActionResult MaximumFiveDaysExchangesGoogleChart()
        {
            return View();
        }

    }
}