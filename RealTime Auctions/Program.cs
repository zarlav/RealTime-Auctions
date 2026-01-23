using RealTime_Auctions.Hubs;
using RealTime_Auctions.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379")
);
builder.Services.AddSingleton<RedisService>();

// Servisi
builder.Services.AddScoped<AuctionService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SignalR (jedan server, demo)
builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// SignalR hub
app.MapHub<AuctionHub>("/hubs/auction");

app.Run();
