using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace PointConsumer.API.Services
{
    public class WebSocketManager
    {
        private ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        public void AddSocket(WebSocket socket)
        {
            var socketId = Guid.NewGuid().ToString();
            _sockets.TryAdd(socketId, socket);
        }

        public async Task SendMessageToAllAsync(string message)
        {
            var socketsToRemove = new List<string>();
            foreach (var pair in _sockets)
            {
                var socket = pair.Value;
                if (socket.State == WebSocketState.Open)
                {
                    var encodedMessage = Encoding.UTF8.GetBytes(message);
                    await socket.SendAsync(new ArraySegment<byte>(encodedMessage, 0, encodedMessage.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else if (socket.State != WebSocketState.Connecting)
                {
                    socketsToRemove.Add(pair.Key);
                }
            }

            foreach (var key in socketsToRemove)
            {
                _sockets.TryRemove(key, out _);
            }
        }
    }
}
