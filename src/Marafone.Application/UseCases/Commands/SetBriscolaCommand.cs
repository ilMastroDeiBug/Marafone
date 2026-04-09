using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Marafone.Application.Interfaces;
using Marafone.Domain.Entities.UsersEntities;
using Marafone.Domain.ValueObjects;
using System;

namespace Marafone.Application.UseCases.Commands
{
    public class SetBriscolaCommand
    {
        private readonly IMatchRepository _matchRepository;

        public SetBriscolaCommand(IMatchRepository repo) => _matchRepository = repo;

        public void Execute(Guid matchId, Guid playerId, string suitString)
        {
            var match = _matchRepository.GetById(matchId) ?? throw new Exception("Partita non trovata!");
            var player = MatchHelper.GetPlayer(match, playerId) ?? throw new Exception("Non sei a questo tavolo!");

            Suit briscolaScelta = Enum.Parse<Suit>(suitString, true);

            match.SetBriscola(player, briscolaScelta);

            _matchRepository.Save(match);
        }
    }
}