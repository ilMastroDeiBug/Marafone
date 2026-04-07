using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Marafone.Domain.ValueObjects;

namespace Marafone.Domain.Entities.UsersEntities
{
    public class Squad
    {
        public Name Name { get; private set; }
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }

        // Usiamo i nostri nuovi Value Objects
        public HandPoints HandPoints { get; private set; }
        public MatchPoints MatchPoints { get; private set; }

        public Squad(Name name, Player player1, Player player2)
        {
            Name = name;
            Player1 = player1;
            Player2 = player2;

            // Inizializziamo a zero
            HandPoints = new HandPoints(0);
            MatchPoints = new MatchPoints(0);
        }

        public void AddTrickPoints(int rawPoints)
        {
            // Sovrascriviamo con il nuovo record aggiornato
            HandPoints = HandPoints.Add(rawPoints);
        }

        public void AddMatchPoints(int points)
        {
            MatchPoints = MatchPoints.Add(points);
        }

        public void ResetForNewHand()
        {
            HandPoints = new HandPoints(0);
        }
    }
}
