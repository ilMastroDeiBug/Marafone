using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marafone.Application.DTOs
{
    public class MatchDTO
    {
        public Guid Id { get; init; }
        public SquadDTO Squadra1 { get; init; }
        public SquadDTO Squadra2 { get; init; }
        public string BriscolaAttuale { get; init; }
        public Guid CurrentPlayerId { get; init; }
        public List<PlayedCardDTO> Tavolo { get; init; }
        public bool IsGameOver { get; init; }
        public string VincitorePartita { get; init; }
    }
}
