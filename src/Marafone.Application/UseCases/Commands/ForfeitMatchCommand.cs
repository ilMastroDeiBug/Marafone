using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Marafone.Application.Interfaces;
using System;

namespace Marafone.Application.UseCases.Commands
{
    public class ForfeitMatchCommand
    {
        private readonly IMatchRepository _matchRepository;

        public ForfeitMatchCommand(IMatchRepository repo) => _matchRepository = repo;

        public void Execute(Guid matchId, Guid playerIdThatQuit)
        {
            var match = _matchRepository.GetById(matchId) ?? throw new Exception("Partita non trovata!");
            var quitter = MatchHelper.GetPlayer(match, playerIdThatQuit);

            if (quitter == null || match.IsGameOver)
                return; // Ignora se il giocatore non è lì o la partita è già finita

            // Questa logica "sporca" va nell'Application Layer o nel Domain? 
            // Essendo una rottura delle regole, possiamo gestirla forzando il punteggio.
            var squadraVincente = match.Squadra1.Player1.Id == playerIdThatQuit || match.Squadra1.Player2.Id == playerIdThatQuit
                ? match.Squadra2 // Vince la 2 se uno della 1 quitta
                : match.Squadra1;

            // Forza la vittoria assegnando 41 punti
            squadraVincente.AddMatchPoints(41);

            // Forza il ricalcolo per settare IsGameOver a true
            // (Nota: Per fare questo alla perfezione, dovresti aggiungere un metodo 'Forfeit(Player p)' nel tuo Match.cs)

            _matchRepository.Save(match);
        }
    }
}