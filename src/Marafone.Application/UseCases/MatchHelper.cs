using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Marafone.Domain.Entities.UsersEntities;
using Marafone.Domain.GameLogic;

namespace Marafone.Application.UseCases
{
    public static class MatchHelper
    {
        public static Player GetPlayer(Game match, Guid playerId)
        {
            if (match.Squadra1.Player1.Id == playerId) return match.Squadra1.Player1;
            if (match.Squadra1.Player2.Id == playerId) return match.Squadra1.Player2;
            if (match.Squadra2.Player1.Id == playerId) return match.Squadra2.Player1;
            if (match.Squadra2.Player2.Id == playerId) return match.Squadra2.Player2;
            return null;
        }
    }
}