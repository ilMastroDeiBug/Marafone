using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace Marafone.Domain.ValueObjects
{
    public record MatchPoints
    {
        public int Value { get; init; }

        public MatchPoints(int value = 0)
        {
            if (value < 0)
                throw new ArgumentException("I punti partita non possono scendere sotto zero.");

            Value = value;
        }
        public MatchPoints Add(int points)
        {
            return new MatchPoints(this.Value + points);
        }

        // La regola di vittoria sta qui, non sparsa nel codice!
        public bool HasWon()
        {
            return Value >= 41;
        }
    }
}
