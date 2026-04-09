using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Marafone.Application.Interfaces;
using System;

namespace Marafone.Application.UseCases.Commands
{
    public class StartNextHandCommand
    {
        private readonly IMatchRepository _matchRepository;

        public StartNextHandCommand(IMatchRepository repo) => _matchRepository = repo;

        public void Execute(Guid matchId, Guid requestingPlayerId)
        {
            var match = _matchRepository.GetById(matchId) ?? throw new Exception("Partita non trovata!");
            var player = MatchHelper.GetPlayer(match, requestingPlayerId) ?? throw new Exception("Non sei a questo tavolo!");

            // Sicurezza: non puoi dare le carte a metà mano o se la partita è già finita
            if (match.IsGameOver)
                throw new InvalidOperationException("La partita è finita!");

            // Verifica che effettivamente le mani siano vuote prima di ridare le carte
            if (player.Hand.Count > 0)
                throw new InvalidOperationException("La mano corrente non è ancora terminata!");

            match.StartNewGame(); // Mescola e dà altre 40 carte

            _matchRepository.Save(match);
        }
    }
}