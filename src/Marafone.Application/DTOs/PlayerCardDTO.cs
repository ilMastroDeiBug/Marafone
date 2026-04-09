
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marafone.Application.DTOs
{
    public class PlayedCardDTO
    {
        public Guid PlayerId { get; init; }
        public string PlayerName { get; init; }
        public CardDTO Card { get; init; }
    }
}
