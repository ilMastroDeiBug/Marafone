using Marafone.Domain.Entities.GameComponents;
using Marafone.Domain.ValueObjects;
using System.Collections.Generic;
using System.Linq;

namespace Marafone.Domain.GameLogic
{
    public class MaraffaEvaluator
    {
        public int Evaluate(List<Card> hand, Suit briscola)
        {
            // Prendiamo solo le briscole del giocatore
            var briscolaRanks = hand
                .Where(c => c.Suit == briscola)
                .Select(c => c.Rank)
                .ToList();

            // Controllo base: se manca anche solo uno tra Asso, Due o Tre, niente Cricca!
            if (!briscolaRanks.Contains(Rank.asso) ||
                !briscolaRanks.Contains(Rank.due) ||
                !briscolaRanks.Contains(Rank.tre))
            {
                return 0;
            }

            // Abbiamo il Marafone! Partiamo da 3 punti partita.
            int puntiPartita = 3;

            // Definiamo l'ordine della scala FISICA per capire i punti extra
            // (La scala per le accuse è visiva, non segue la gerarchia della presa)
            Rank[] scaleOrder = {
                Rank.quattro, Rank.cinque, Rank.sei, Rank.sette,
                Rank.fante, Rank.cavallo, Rank.re
            };

            // Controlliamo fin dove si estende la scala in mano
            foreach (var rank in scaleOrder)
            {
                if (briscolaRanks.Contains(rank))
                {
                    puntiPartita++; // Punto extra per ogni carta successiva in scala
                }
                else
                {
                    break; // Se salta un gradino, la scala si ferma
                }
            }

            return puntiPartita;
        }
    }
}