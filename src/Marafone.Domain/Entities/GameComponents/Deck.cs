using Marafone.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Marafone.Domain.Entities.GameComponents
{
    public class Deck
    {
        public List<Card> Cards { get; private set; }
        private Random _random = new Random();
        public Deck()
        {
            Cards = new List<Card>();
            CreateDeck();
        }
        public void CreateDeck()
        {
            foreach(var r in Enum.GetValues<Rank>())
            {
                foreach(var s in Enum.GetValues<Suit>())
                {
                    Cards.Add(new Card(r, s));
                }
            }
        }
        public void ShuffleDeck()
        {
            int n = Cards.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                // Scambio (Swap) degli elementi
                Card value = Cards[k];
                Cards[k] = Cards[n];
                Cards[n] = value;
            }
        }

    }
}
