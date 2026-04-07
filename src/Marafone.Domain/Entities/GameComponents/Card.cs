using Marafone.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marafone.Domain.Entities.GameComponents
{
    public class Card
    {
        public Rank Rank { get; private set; }
        public Suit Suit { get; private set; }

        public Card(Rank rank, Suit suit)
        {
            Rank = rank;
            Suit = suit;
        }

        public int GetScore()
        {
            switch (Rank)
            {
                case Rank.asso:
                    return 3;
                case Rank.tre:
                    return 1;
                case Rank.due:
                    return 1;
                case Rank.re:
                    return 1;
                case Rank.cavallo:
                    return 1;
                case Rank.fante:
                    return 1;
                default:
                    return 0;
            }
        }

        public int GetStrength()
        {
            switch (Rank)
            {
                case Rank.tre:
                    return 10;
                case Rank.due:
                    return 9;
                case Rank.asso:
                    return 8;
                case Rank.re:
                    return 7;
                case Rank.cavallo:
                    return 6;
                case Rank.fante:
                    return 5;
                case Rank.sette:
                    return 4;
                case Rank.sei:
                    return 3;
                case Rank.cinque:
                    return 2;
                case Rank.quattro:
                    return 1;
                default:
                    return 0;
            }
        }
    }
}
