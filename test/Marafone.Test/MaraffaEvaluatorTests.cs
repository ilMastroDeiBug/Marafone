using Marafone.Domain.Entities.GameComponents;
using Marafone.Domain.GameLogic;
using Marafone.Domain.ValueObjects;
using System.Collections.Generic;
using Xunit;

namespace Marafone.Tests.Domain
{
    public class MaraffaEvaluatorTests
    {
        private readonly MaraffaEvaluator _evaluator;

        public MaraffaEvaluatorTests()
        {
            _evaluator = new MaraffaEvaluator();
        }

        [Fact]
        public void Evaluate_ManoVuota_RestituisceZero()
        {
            var hand = new List<Card>();
            int result = _evaluator.Evaluate(hand, Suit.bastoni);
            Assert.Equal(0, result);
        }

        [Fact]
        public void Evaluate_MancaAsso_RestituisceZero()
        {
            var hand = new List<Card>
            {
                new Card(Rank.due, Suit.bastoni),
                new Card(Rank.tre, Suit.bastoni),
                new Card(Rank.quattro, Suit.bastoni)
            };
            int result = _evaluator.Evaluate(hand, Suit.bastoni);
            Assert.Equal(0, result);
        }

        [Fact]
        public void Evaluate_MarafoneBaseSoloTreCarte_RestituisceTre()
        {
            var hand = new List<Card>
            {
                new Card(Rank.asso, Suit.coppe),
                new Card(Rank.due, Suit.coppe),
                new Card(Rank.tre, Suit.coppe),
                new Card(Rank.cavallo, Suit.spade) // Carta inutile
            };
            int result = _evaluator.Evaluate(hand, Suit.coppe);
            Assert.Equal(3, result);
        }

        [Fact]
        public void Evaluate_MarafoneConIlQuattro_RestituisceQuattro()
        {
            var hand = new List<Card>
            {
                new Card(Rank.asso, Suit.denara),
                new Card(Rank.due, Suit.denara),
                new Card(Rank.tre, Suit.denara),
                new Card(Rank.quattro, Suit.denara), // La scala inizia
                new Card(Rank.re, Suit.spade)
            };
            int result = _evaluator.Evaluate(hand, Suit.denara);
            Assert.Equal(4, result);
        }

        [Fact]
        public void Evaluate_ScalaInterrotta_SiFermaPrimaDellInterruzione()
        {
            // Ha Asso, 2, 3, 4 e poi salta al 6 (manca il 5).
            // La scala deve fermarsi al 4, quindi restituire 4 punti (3 base + 1 per il quattro).
            var hand = new List<Card>
            {
                new Card(Rank.asso, Suit.spade),
                new Card(Rank.due, Suit.spade),
                new Card(Rank.tre, Suit.spade),
                new Card(Rank.quattro, Suit.spade),
                new Card(Rank.sei, Suit.spade) // Manca il cinque!
            };
            int result = _evaluator.Evaluate(hand, Suit.spade);
            Assert.Equal(4, result);
        }

        [Fact]
        public void Evaluate_MarafoneMassimo_ScalaFinoAlRe_RestituisceDieci()
        {
            // Asso, 2, 3 (3 punti) + 4, 5, 6, 7, Fante, Cavallo, Re (7 carte extra) = 10 punti.
            var hand = new List<Card>
            {
                new Card(Rank.asso, Suit.bastoni),
                new Card(Rank.due, Suit.bastoni),
                new Card(Rank.tre, Suit.bastoni),
                new Card(Rank.quattro, Suit.bastoni),
                new Card(Rank.cinque, Suit.bastoni),
                new Card(Rank.sei, Suit.bastoni),
                new Card(Rank.sette, Suit.bastoni),
                new Card(Rank.fante, Suit.bastoni),
                new Card(Rank.cavallo, Suit.bastoni),
                new Card(Rank.re, Suit.bastoni)
            };
            int result = _evaluator.Evaluate(hand, Suit.bastoni);
            Assert.Equal(10, result);
        }

        [Fact]
        public void Evaluate_AssoDueTreDiSemeDiverso_RestituisceZero()
        {
            var hand = new List<Card>
            {
                new Card(Rank.asso, Suit.coppe),
                new Card(Rank.due, Suit.coppe),
                new Card(Rank.tre, Suit.coppe)
            };
            int result = _evaluator.Evaluate(hand, Suit.spade); // La briscola è spade!
            Assert.Equal(0, result);
        }

        [Fact]
        public void Evaluate_OrdineManoCasuale_RestituiscePunteggioCorretto()
        {
            // L'ordine nella lista non deve importare per l'algoritmo
            var hand = new List<Card>
            {
                new Card(Rank.tre, Suit.coppe),
                new Card(Rank.quattro, Suit.coppe),
                new Card(Rank.asso, Suit.coppe),
                new Card(Rank.due, Suit.coppe)
            };
            int result = _evaluator.Evaluate(hand, Suit.coppe);
            Assert.Equal(4, result);
        }
    }
}