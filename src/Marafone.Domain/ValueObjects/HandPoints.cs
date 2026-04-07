using System;

namespace Marafone.Domain.ValueObjects
{
    public record HandPoints
    {
        // Il valore interno grezzo (x3)
        public int RawValue { get; init; }

        public HandPoints(int rawValue = 0)
        {
            if (rawValue < 0)
                throw new ArgumentException("I punti della mano non possono essere negativi.");

            RawValue = rawValue;
        }

        // 1. Quello che serve alla logica pura (Match.cs) per convertire a fine mano
        // Ricorda: a fine mano, i terzi avanzati si buttano via!
        public int NumericValue => RawValue / 3;

        // 2. Quello che serve alla UI per mostrare i punti in tempo reale
        public string RealValue
        {
            get
            {
                int interi = RawValue / 3; // Quanti punti pieni
                int terzi = RawValue % 3;  // Quante figure "avanzano" (può essere 0, 1 o 2)

                // Caso 1: Zero assoluto
                if (interi == 0 && terzi == 0)
                    return "0";

                // Caso 2: Solo frazioni (es. "1/3" o "2/3")
                if (interi == 0 && terzi > 0)
                    return $"{terzi}/3";

                // Caso 3: Solo interi precisi (es. "2")
                if (interi > 0 && terzi == 0)
                    return $"{interi}";

                // Caso 4: Intero + Frazione (es. "2 1/3")
                return $"{interi} {terzi}/3";
            }
        }

        // Metodo per sommare i punti
        public HandPoints Add(int rawPoints)
        {
            return new HandPoints(this.RawValue + rawPoints);
        }
    }
}