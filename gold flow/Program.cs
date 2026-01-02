using hub;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddSignalR();
builder.Services.AddHostedService<GoldPriceBackgroundService>();
builder.Services.AddHostedService<TransactionCleanupService>();

// initialize ConnectionString once
DataAccessLayer.ConnectionString.Initialize(builder.Configuration);
// ✅ Correct CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy
            .WithOrigins("https://localhost:5173") // 👈 use HTTPS if your frontend is on HTTPS
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});


var app = builder.Build();

// ⚡ Important Order
app.UseCors("AllowFrontend");


app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHub<GoldHub>("/goldHub");
app.MapHub<TransactionHub>("/transactionHub");
app.MapHub<WalletHub>("/wallethub");
app.MapHub<PendingTransactionsHub>("/pendingtransactionshub");



app.Run();
