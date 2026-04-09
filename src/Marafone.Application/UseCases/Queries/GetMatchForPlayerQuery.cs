using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Marafone.Application.DTOs;
using Marafone.Application.Interfaces;
using Marafone.Application.Mappers;
using System;

namespace Marafone.Application.UseCases.Queries
{
    public class GetMatchForPlayerQuery
    {
        private readonly IMatchRepository _matchRepository;

        public GetMatchForPlayerQuery(IMatchRepository repository)
        {
            _matchRepository = repository;
        }

        public MatchDTO Execute(Guid matchId, Guid requestingPlayerId)
        {
            // 1. Recuperiamo la partita pura (con tutte le carte in chiaro)
            var match = _matchRepository.GetById(matchId);
            if (match == null)
                return null;

            // 2. Traduciamo tutto in DTO (copia in memoria da spedire su Internet)
            var dto = MatchMapper.ToDTO(match);

            // 3. LA CENSURA (Il Firewall Anti-Cheat)
            // Nascondiamo le carte di chiunque non sia l'utente che sta facendo la richiesta.
            OscuraManoAvversario(dto.Squadra1.Player1, requestingPlayerId);
            OscuraManoAvversario(dto.Squadra1.Player2, requestingPlayerId);
            OscuraManoAvversario(dto.Squadra2.Player1, requestingPlayerId);
            OscuraManoAvversario(dto.Squadra2.Player2, requestingPlayerId);

            return dto;
        }

        private void OscuraManoAvversario(PlayerDTO playerDto, Guid requestingPlayerId)
        {
            // Se il giocatore è proprio l'utente che ha fatto la richiesta, lasciamo le carte in chiaro!
            if (playerDto.Id == requestingPlayerId)
                return;

            // Altrimenti, è un avversario o il compagno. Dobbiamo nascondere le carte.
            // Invece di fare playerDto.Hand.Clear() (che farebbe sparire visivamente le carte),
            // le sostituiamo con delle carte "Coperte" generiche. 
            // In questo modo l'app Flutter sa QUANTE carte ha in mano l'avversario per poter 
            // disegnare il dorso delle carte sullo schermo!

            var numeroCarteInMano = playerDto.Hand.Count;
            playerDto.Hand.Clear();

            for (int i = 0; i < numeroCarteInMano; i++)
            {
                playerDto.Hand.Add(new CardDTO
                {
                    Rank = "Dorso", // Valore fittizio che il tuo frontend capirà come "carta coperta"
                    Suit = "Nessuno"
                });
            }
        }
    }
}