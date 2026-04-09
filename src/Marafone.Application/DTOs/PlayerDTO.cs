using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marafone.Application.DTOs
{
    public class PlayerDTO
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public List<CardDTO> Hand { get; init; }
    }
}
