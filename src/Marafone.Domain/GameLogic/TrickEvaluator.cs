using Marafone.Domain.Entities.GameComponents;
using Marafone.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marafone.Domain.GameLogic
{
    public class TrickEvaluator
    {
        public PlayedCard EvaluateWinner(List<PlayedCard> table, Suit briscola)
        {
            if (table.Count == 0)
                throw new ArgumentException("Il tavolo è vuoto.");

            // Il seme di uscita (leading suit) è stabilito dalla prima carta in tavola
            Suit leadingSuit = table[0].Card.Suit;
            PlayedCard currentWinner = table[0];

            for (int i = 1; i < table.Count; i++)
            {
                if (IsCardStronger(table[i].Card, currentWinner.Card, leadingSuit, briscola))
                {
                    currentWinner = table[i];
                }
            }

            return currentWinner;
        }

        private bool IsCardStronger(Card challenger, Card champion, Suit leadingSuit, Suit briscola)
        {
            // Regola 1: Briscola taglia tutto
            if (challenger.Suit == briscola && champion.Suit != briscola)
                return true;

            // Regola 2: Il campione è briscola e tu no? Hai perso.
            if (champion.Suit == briscola && challenger.Suit != briscola)
                return false;

            // Regola 3: Stesso seme (o entrambe briscole, o entrambe seme di uscita).
            // A questo punto, lasciamo decidere alla TUA gerarchia (Tre > Due > Asso...)
            if (challenger.Suit == champion.Suit)
                return challenger.GetStrength() > champion.GetStrength();

            // Regola 5: Lo sfidante ha "buttato via" una carta (né briscola, né seme uscita). Zero assoluto.
            return false;
        }
    }
}