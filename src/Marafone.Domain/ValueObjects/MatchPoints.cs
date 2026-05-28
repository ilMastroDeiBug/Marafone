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

        public MatchPoints Add(int points) => new MatchPoints(this.Value + points);

        /// <summary>
        /// Verifica vittoria con target dinamico (21 / 31 / 41).
        /// </summary>
        public bool HasWon(int targetPoints) => Value >= targetPoints;
    }
}
