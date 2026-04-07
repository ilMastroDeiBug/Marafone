using Marafone.Domain.Entities.GameComponents;
using Marafone.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marafone.Domain.Entities.UsersEntities
{
    public class Player
    {
        public Guid Id { get; init; }
        public Name Name { get; init; }
        public List<Card> Hand { get; private set; }
        public Player(Name name)
        {
            Id = Guid.NewGuid();
            Name = name;
            Hand = new List<Card>();
        }
        public void ReceiveHand(List<Card> cards)
        {
            Hand = cards;
        }
        public void PlayTurn(Card card)
        {
            Hand.Remove(card);
        }
    }
}
