using Microsoft.AspNetCore.SignalR;

namespace DoAn.wwwroot
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message, string avatar)
        {
            await Clients.All.SendAsync("RevceiveMessage", user, message, avatar);
        }
    }
}
