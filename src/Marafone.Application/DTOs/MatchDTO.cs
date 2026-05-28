using System;
using System.Collections.Generic;

namespace Marafone.Application.DTOs
{
    public class MatchDTO
    {
        public Guid Id { get; init; }
        public SquadDTO Squadra1 { get; init; }
        public SquadDTO Squadra2 { get; init; }
        public string BriscolaAttuale { get; init; }   // null se non ancora scelta
        public Guid CurrentPlayerId { get; init; }
        public string CurrentPlayerName { get; init; }
        public List<PlayedCardDTO> Tavolo { get; init; }
        public bool IsGameOver { get; init; }
        public string VincitorePartita { get; init; }  // null se in corso
        public int TargetPoints { get; init; }          // 21 / 31 / 41
        public string Phase { get; init; }              // "BriscolaSelection" | "Playing" | "GameOver"
    }
}
