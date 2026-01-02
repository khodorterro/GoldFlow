using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using hub;

public class GoldPriceBackgroundService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHubContext<GoldHub> _hubContext;

    public GoldPriceBackgroundService(IHttpClientFactory httpClientFactory, IHubContext<GoldHub> hubContext)
    {
        _httpClientFactory = httpClientFactory;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("x-access-token", "goldapi-1jvzsmb2l9d6o-io"); 

                string symbol = "XAU";
                string currency = "USD";
                string url = $"https://www.goldapi.io/api/{symbol}/{currency}";

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    dynamic goldData = JsonConvert.DeserializeObject<dynamic>(result);
                    double ouncePrice = goldData.price;

                    await _hubContext.Clients.All.SendAsync("ReceiveGoldPriceUpdate", new { price = ouncePrice });

                    Console.WriteLine($"✅ Gold price updated: {ouncePrice}$");
                }
                else
                {
                    Console.WriteLine($"❌ Failed to fetch gold price. StatusCode: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❗ Exception fetching gold price: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
