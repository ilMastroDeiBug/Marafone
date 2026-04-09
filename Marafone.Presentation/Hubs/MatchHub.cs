using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Marafone.Presentation.Hubs
{
    public class MatchHub : Hub
    {
        // Flutter chiamerà questo metodo appena entra nella schermata del tavolo
        public async Task JoinMatch(string matchId)
        {
            // Aggiungiamo il telefono a un "Gruppo" che ha come nome l'ID della partita
            await Groups.AddToGroupAsync(Context.ConnectionId, matchId);
        }

        // Flutter chiamerà questo metodo quando esce dalla partita
        public async Task LeaveMatch(string matchId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, matchId);
        }
    }
}