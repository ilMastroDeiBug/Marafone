using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Marafone.Application.Interfaces;
using Marafone.Domain.Entities.GameComponents;
using Marafone.Domain.Entities.UsersEntities;
using Marafone.Domain.ValueObjects;
using System;

namespace Marafone.Application.UseCases.Commands
{
    public class PlayCardCommand
    {
        private readonly IMatchRepository _matchRepository;

        public PlayCardCommand(IMatchRepository repo)
        {
            _matchRepository = repo;
        }

        // Il frontend manda dati grezzi: Id partita, Id giocatore, e le stringhe della carta
        public void Execute(Guid matchId, Guid playerId, string rankString, string suitString)
        {
            // 1. Recupera l'Aggregato (La partita) dal Database/Memoria
            var match = _matchRepository.GetById(matchId);
            if (match == null)
                throw new Exception("Partita non trovata!");

            // 2. Trova il giocatore all'interno della partita
            Player playerPlaying = GetPlayerFromMatch(match, playerId);
            if (playerPlaying == null)
                throw new Exception("Giocatore non trovato in questo tavolo!");

            // 3. Converte i comandi del frontend negli oggetti del Domain
            // Ignora il maiuscolo/minuscolo usando 'true' nel parsing
            Rank rank = Enum.Parse<Rank>(rankString, true);
            Suit suit = Enum.Parse<Suit>(suitString, true);
            Card cardToPlay = new Card(rank, suit);

            // 4. Delega tutta la complessità (turni, regole, punti) al Core Domain!
            match.PlayCard(playerPlaying, cardToPlay);

            // 5. Salva il nuovo stato nel database
            _matchRepository.Save(match);
        }

        // Metodo Helper per estrarre l'istanza corretta del Player dall'aggregato
        private Player GetPlayerFromMatch(Domain.GameLogic.Game match, Guid playerId)
        {
            if (match.Squadra1.Player1.Id == playerId) return match.Squadra1.Player1;
            if (match.Squadra1.Player2.Id == playerId) return match.Squadra1.Player2;
            if (match.Squadra2.Player1.Id == playerId) return match.Squadra2.Player1;
            if (match.Squadra2.Player2.Id == playerId) return match.Squadra2.Player2;
            return null;
        }
    }
}