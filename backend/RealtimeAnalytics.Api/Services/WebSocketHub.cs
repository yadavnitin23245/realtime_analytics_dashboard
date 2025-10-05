using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimeAnalytics.Api.Services
{
    public class WebSocketHub
    {
        private readonly ConcurrentDictionary<string, WebSocket> _clients = new();

        public Task AddClientAsync(string id, WebSocket ws)
        {
            _clients[id] = ws;
            return Task.CompletedTask;
        }

        public async Task RemoveClientAsync(string id)
        {
            if (_clients.TryRemove(id, out var ws))
            {
                try { await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None); }
                catch { }
                ws.Dispose();
            }
        }

        public async Task BroadcastAsync(string preSerializedJson, CancellationToken ct = default)
        {
            var bytes = Encoding.UTF8.GetBytes(preSerializedJson);
            var seg = new ArraySegment<byte>(bytes);
            var dead = new List<string>();
            foreach (var kv in _clients)
            {
                try
                {
                    if (kv.Value.State == WebSocketState.Open)
                        await kv.Value.SendAsync(seg, WebSocketMessageType.Text, true, ct);
                    else
                        dead.Add(kv.Key);
                }
                catch
                {
                    dead.Add(kv.Key);
                }
            }
            foreach (var id in dead) _clients.TryRemove(id, out _);
        }
    }
}