using Microsoft.AspNetCore.SignalR;
using SuperModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgainServer.Hubs
{
    public class MessageHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task GetList(List<string> getMess)
        {
            await Clients.Caller.SendAsync("ReceiveList", getMess);
        }

        public async Task SendChunkedFile(ChunkedData fileChunk, string pluginName, int count, int itemsCount, ConfigModel model)
        {
            await Clients.All.SendAsync("ReceiveChunks", fileChunk, pluginName, count, itemsCount, model);
        }
    }
}