// BusinessLayer/TransactionCleanupService.cs
using Microsoft.AspNetCore.SignalR;
using hub;
using Microsoft.Extensions.Hosting;
using DataAccessLayer;// your hub

public class TransactionCleanupService : BackgroundService
{
    private readonly Transactionsdata _transactionsData;

    public TransactionCleanupService(IHubContext<TransactionHub> hubContext)
    {
        _transactionsData = new Transactionsdata(hubContext);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                Console.WriteLine($"[Background Service] Checking for expired transactions at {DateTime.Now}");

                _transactionsData.CancelExpiredTransactions();

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Background Service Error] {ex.Message}");
            }
        }
    }
}
