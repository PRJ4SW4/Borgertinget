
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace backend.Hubs
{
   
    public class FeedHub : Hub
    {
        
        public override async Task OnConnectedAsync()
        {
           
            Console.WriteLine($"SignalR Client Connected: {Context.ConnectionId}"); 
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
             
             Console.WriteLine($"SignalR Client Disconnected: {Context.ConnectionId}"); 
            if (exception != null)
            {
                Console.WriteLine($"SignalR Disconnect Error: {exception.Message}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        
    }
}