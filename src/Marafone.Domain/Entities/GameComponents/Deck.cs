using Marafone.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace Marafone.Domain.Entities.GameComponents
{
    public class Deck
    {
        public List<Card> Cards { get; private set; }
        private readonly Random _random = new Random();

        public Deck()
        {
            Cards = new List<Card>();
            CreateDeck();
        }

        public void CreateDeck()
        {
            Cards.Clear();
            foreach (var r in Enum.GetValues<Rank>())
            {
                foreach (var s in Enum.GetValues<Suit>())
                {
                    Cards.Add(new Card(r, s));
                }
            }
            // Garantiamo sempre esattamente 40 carte (4 semi x 10 valori)
            if (Cards.Count != 40)
                throw new Exception($"Bug nel mazzo: {Cards.Count} carte generate invece di 40.");
        }

        public void ShuffleDeck()
        {
            int n = Cards.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                Card value = Cards[k];
                Cards[k] = Cards[n];
                Cards[n] = value;
            }
        }
    }
}
