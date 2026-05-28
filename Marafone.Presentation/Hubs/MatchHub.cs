using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Marafone.Presentation.Hubs
{
    public class MatchHub : Hub
    {
        /// <summary>
        /// Il client chiama questo metodo appena entra nella schermata del tavolo.
        /// Lo aggiunge al gruppo corrispondente alla partita.
        /// </summary>
        public async Task JoinMatch(string matchId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, matchId);
            // Notifica gli altri che qualcuno è entrato
            await Clients.OthersInGroup(matchId)
                .SendAsync("PlayerJoined", Context.ConnectionId);
        }

        /// <summary>
        /// Il client chiama questo metodo quando esce dalla partita.
        /// </summary>
        public async Task LeaveMatch(string matchId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, matchId);
            await Clients.OthersInGroup(matchId)
                .SendAsync("PlayerLeft", Context.ConnectionId);
        }

        /// <summary>
        /// Gestisce la disconnessione improvvisa (app chiusa, rete persa, ecc.).
        /// </summary>
        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            // Qui potresti fare il forfeit automatico se la partita è in corso.
            // Per ora semplicemente logghi la disconnessione.
            await base.OnDisconnectedAsync(exception);
        }
    }
}