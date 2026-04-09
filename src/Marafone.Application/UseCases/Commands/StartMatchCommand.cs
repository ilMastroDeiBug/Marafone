using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            _userRepository = userRepo;
        }

        public Guid Execute(Guid user1Id, Guid user2Id, Guid user3Id, Guid user4Id)
        {
            // 1. Recupera gli utenti reali
            var u1 = _userRepository.GetById(user1Id);
            var u2 = _userRepository.GetById(user2Id);
            var u3 = _userRepository.GetById(user3Id);
            var u4 = _userRepository.GetById(user4Id);

            // 2. Crea gli Avatar (Player) iniettando l'ID dell'utente!
            var p1 = new Player(new Name(u1.Username)) { Id = u1.Id };
            var p2 = new Player(new Name(u2.Username)) { Id = u2.Id };
            var p3 = new Player(new Name(u3.Username)) { Id = u3.Id };
            var p4 = new Player(new Name(u4.Username)) { Id = u4.Id };

            // 3. Forma le squadre (Nord-Sud vs Est-Ovest)
            var squad1 = new Squad(new Name($"{p1.Name.Value} & {p3.Name.Value}"), p1, p3);
            var squad2 = new Squad(new Name($"{p2.Name.Value} & {p4.Name.Value}"), p2, p4);

            // 4. Inizializza l'Aggregato
            var match = new Game(squad1, squad2);
            match.StartNewGame(); // Mescola e dà le prime 40 carte

            // 5. Salva a DB
            _matchRepository.Save(match);

            return match.Id;
        }
    }
}
