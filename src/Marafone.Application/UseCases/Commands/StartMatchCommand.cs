using Marafone.Application.Interfaces;
using Marafone.Domain.Entities.UsersEntities;
using Marafone.Domain.GameLogic;
using Marafone.Domain.ValueObjects;
using System;

namespace Marafone.Application.UseCases.Commands
{
    public class StartMatchCommand
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IUserRepository _userRepository;

        public StartMatchCommand(IMatchRepository matchRepo, IUserRepository userRepo)
        {
            _matchRepository = matchRepo;
            _userRepository  = userRepo;
        }

        /// <param name="targetPoints">Punti vittoria: 21 (Corta), 31 (Media), 41 (Lunga)</param>
        public Guid Execute(Guid user1Id, Guid user2Id, Guid user3Id, Guid user4Id, int targetPoints = 41)
        {
            var u1 = _userRepository.GetById(user1Id) ?? throw new Exception($"Utente {user1Id} non trovato");
            var u2 = _userRepository.GetById(user2Id) ?? throw new Exception($"Utente {user2Id} non trovato");
            var u3 = _userRepository.GetById(user3Id) ?? throw new Exception($"Utente {user3Id} non trovato");
            var u4 = _userRepository.GetById(user4Id) ?? throw new Exception($"Utente {user4Id} non trovato");

            // Squadre: u1+u3 vs u2+u4 (compagni di fronte)
            var p1 = new Player(new Name(u1.Username)) { Id = u1.Id };
            var p2 = new Player(new Name(u2.Username)) { Id = u2.Id };
            var p3 = new Player(new Name(u3.Username)) { Id = u3.Id };
            var p4 = new Player(new Name(u4.Username)) { Id = u4.Id };

            var squad1 = new Squad(new Name($"{p1.Name.Value} & {p3.Name.Value}"), p1, p3);
            var squad2 = new Squad(new Name($"{p2.Name.Value} & {p4.Name.Value}"), p2, p4);

            var match = new Game(squad1, squad2, targetPoints);
            match.StartNewGame();

            _matchRepository.Save(match);
            return match.Id;
        }
    }
}
