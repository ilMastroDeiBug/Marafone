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
            var match   = _matchRepository.GetById(matchId) ?? throw new Exception("Partita non trovata!");
            var quitter = MatchHelper.GetPlayer(match, playerIdThatQuit);

            if (quitter == null || match.IsGameOver)
                return; // Ignora se già finita

            // Delega la logica al Domain (ora Forfeit è nel Game)
            match.Forfeit(quitter);

            _matchRepository.Save(match);
        }
    }
}