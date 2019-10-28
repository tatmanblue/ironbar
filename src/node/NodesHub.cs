using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace node
{
    public class NodesHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
