using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marafone.Domain.Entities.UsersEntities
{
    public class Squad
    {
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }
        
        public Squad(Player player1, Player player2)
        {
            Player1 = player1;
            Player2 = player2;
        }
        public int GetSquadPoints()
        {

        }
    }
}
