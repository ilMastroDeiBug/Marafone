using Marafone.Domain.Entities.GameComponents;
using Marafone.Domain.Entities.UsersEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marafone.Domain.ValueObjects
{
    public record PlayedCard(Player Player, Card Card);
}
