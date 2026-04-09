using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marafone.Application.DTOs
{
    public class SquadDTO
    {
        public string Name { get; init; }
        public PlayerDTO Player1 { get; init; }
        public PlayerDTO Player2 { get; init; }
        public int MatchPoints { get; init; }
        public string HandPointsReal { get; init; } // Qui sfruttiamo la tua genialata del "2 1/3"
    }

}
