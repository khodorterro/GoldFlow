using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using BussinnessLayer;
using hub;

namespace gold_flow.Controllers
{
    [Route("api/GoldPrice")]
    [ApiController]
    public class GoldPriceController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IHubContext<GoldHub> _hubContext;

        public GoldPriceController(HttpClient httpClient, IHubContext<GoldHub> hubContext)
        {
            _httpClient = httpClient;
            _hubContext = hubContext;
        }

        [HttpGet("price")]
        public async Task<IActionResult> GetGoldPrice()
        {
            string apiKey = "goldapi-1jvzsmb2l9d6o-io"; 
            string url = "https://www.goldapi.io/api/XAU/USD";

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-access-token", apiKey);
                

                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    var goldData = JsonConvert.DeserializeObject<dynamic>(result);

                    double ouncePrice = goldData?.price != null ? (double)goldData.price : 0.0;

                    // Broadcast live update if needed
                    await _hubContext.Clients.All.SendAsync("ReceiveGoldPriceUpdate", new
                    {
                        price = ouncePrice
                    });

                    return Ok(new { ouncePrice });
                }
                else
                {
                    return StatusCode((int)response.StatusCode, $"Error fetching gold price: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error: " + ex.Message);
            }
        }

    }
}
