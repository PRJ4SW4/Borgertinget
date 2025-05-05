using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace backend.Hubs
{
// SignalR er et bibliotek til realtidskommunikation mellem server og klienter. "week 7 bad signalR"
// 
// FeedHub klassen fungerer som et centralt kommunikationspunkt mellem server og de tilsluttede
// klienter (frontend). Den er baseret på observer pattern, hvilket betyder at den kan sende 
// beskeder til alle tilsluttede klienter, når der sker ændringer i dataene.
//
// I vores applikation bruges den primært til at sende live opdateringer om afstemninger 
// og andre poll events.

    public class FeedHub : Hub  // FeedHub arver fra SignalR klasse
    {
        // Håndterer når en bruger tilslutter sig
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"SignalR Client Connected: {Context.ConnectionId}"); 
            await base.OnConnectedAsync();
        }

        // Håndterer når en bruger afbryder forbindelsen
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"SignalR Client Disconnected: {Context.ConnectionId}"); 
            // Logning af eventuelle fejl ved afbrydelse
            if (exception != null)
            {
                Console.WriteLine($"SignalR Disconnect Error: {exception.Message}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}