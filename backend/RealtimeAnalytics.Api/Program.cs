using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RealtimeAnalytics.Api.Data;
using RealtimeAnalytics.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddWebSockets(options => {});

builder.Services.AddSingleton<ReadingStore>();
builder.Services.AddSingleton<StatsService>();
builder.Services.AddSingleton<WebSocketHub>();
builder.Services.AddHostedService<SensorSimulatorService>();
builder.Services.AddHostedService<DataRetentionService>();

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("dev", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();
app.UseCors("dev");
app.UseSwagger();
app.UseSwaggerUI();

app.UseWebSockets();

app.MapControllers();

app.Map("/stream", async (HttpContext ctx, WebSocketHub hub) =>
{
    if (ctx.WebSockets.IsWebSocketRequest)
    {
        using var ws = await ctx.WebSockets.AcceptWebSocketAsync();
        var clientId = Guid.NewGuid().ToString();
        await hub.AddClientAsync(clientId, ws);
        try
        {
            var buffer = new byte[1024];
            while (ws.State == System.Net.WebSockets.WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close) break;
                if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Text)
                {
                    await ws.SendAsync(System.Text.Encoding.UTF8.GetBytes("{\"type\":\"pong\"}"),
                        System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
        finally
        {
            await hub.RemoveClientAsync(clientId);
        }
    }
    else
    {
        ctx.Response.StatusCode = 400;
    }
});

app.Run();