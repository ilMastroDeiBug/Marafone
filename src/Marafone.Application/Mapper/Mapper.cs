using Marafone.Application.DTOs;
using Marafone.Domain.Entities.GameComponents;
using Marafone.Domain.Entities.UsersEntities;
using Marafone.Domain.GameLogic;
using Marafone.Domain.ValueObjects;
using System.Linq;

namespace Marafone.Application.Mappers
{
    public static class MatchMapper
    {
        public static CardDTO ToDTO(Card card)
        {
            return new CardDTO
            {
                Rank = card.Rank.ToString(),
                Suit = card.Suit.ToString()
            };
        }

        public static PlayedCardDTO ToDTO(PlayedCard playedCard)
        {
            return new PlayedCardDTO
            {
                PlayerId   = playedCard.Player.Id,
                PlayerName = playedCard.Player.Name.Value,
                Card       = ToDTO(playedCard.Card)
            };
        }

        public static PlayerDTO ToDTO(Player player)
        {
            return new PlayerDTO
            {
                Id   = player.Id,
                Name = player.Name.Value,
                Hand = player.Hand.Select(c => ToDTO(c)).ToList()
            };
        }

        public static SquadDTO ToDTO(Squad squad)
        {
            return new SquadDTO
            {
                Name           = squad.Name.Value,
                Player1        = ToDTO(squad.Player1),
                Player2        = ToDTO(squad.Player2),
                MatchPoints    = squad.MatchPoints.Value,
                HandPointsReal = squad.HandPoints.RealValue
            };
        }

        public static MatchDTO ToDTO(Game match)
        {
            string phase;
            if (match.IsGameOver)
                phase = "GameOver";
            else if (match.BriscolaAttuale == null)
                phase = "BriscolaSelection";
            else
                phase = "Playing";

            return new MatchDTO
            {
                Id                = match.Id,
                Squadra1          = ToDTO(match.Squadra1),
                Squadra2          = ToDTO(match.Squadra2),
                BriscolaAttuale   = match.BriscolaAttuale?.ToString(),
                CurrentPlayerId   = match.CurrentPlayer.Id,
                CurrentPlayerName = match.CurrentPlayer.Name.Value,
                Tavolo            = match.Tavolo.Select(pc => ToDTO(pc)).ToList(),
                IsGameOver        = match.IsGameOver,
                VincitorePartita  = match.VincitorePartita?.Name.Value,
                TargetPoints      = match.TargetPoints,
                Phase             = phase
            };
        }
    }
}