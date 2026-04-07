using Marafone.Domain.Entities.GameComponents;
using Marafone.Domain.Entities.UsersEntities;
using Marafone.Domain.GameLogic;
using Marafone.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using Xunit;

namespace Marafone.Tests.Domain
{
    public class TrickEvaluatorTests
    {
        private readonly TrickEvaluator _evaluator;
        private readonly Player _dummyPlayer1;
        private readonly Player _dummyPlayer2;
        private readonly Player _dummyPlayer3;
        private readonly Player _dummyPlayer4;

        public TrickEvaluatorTests()
        {
            _evaluator = new TrickEvaluator();
            _dummyPlayer1 = new Player(new Name("Nord"));
            _dummyPlayer2 = new Player(new Name("Est"));
            _dummyPlayer3 = new Player(new Name("Sud"));
            _dummyPlayer4 = new Player(new Name("Ovest"));
        }

        [Fact]
        public void EvaluateWinner_TavoloVuoto_LanciaEccezione()
        {
            var tavoloVuoto = new List<PlayedCard>();
            Assert.Throws<ArgumentException>(() => _evaluator.EvaluateWinner(tavoloVuoto, Suit.spade));
        }

        [Fact]
        public void EvaluateWinner_BriscolaBatteSemeDiUscita_VinceBriscola()
        {
            var tavolo = new List<PlayedCard>
            {
                new PlayedCard(_dummyPlayer1, new Card(Rank.tre, Suit.coppe)),
                new PlayedCard(_dummyPlayer2, new Card(Rank.re, Suit.coppe)),
                new PlayedCard(_dummyPlayer3, new Card(Rank.quattro, Suit.spade)), // Taglio a briscola
                new PlayedCard(_dummyPlayer4, new Card(Rank.asso, Suit.coppe))
            };
            var vincitore = _evaluator.EvaluateWinner(tavolo, Suit.spade);
            Assert.Equal(_dummyPlayer3, vincitore.Player);
        }

        [Fact]
        public void EvaluateWinner_DueBriscoleSulTavolo_VinceBriscolaPiuForte()
        {
            var tavolo = new List<PlayedCard>
            {
                new PlayedCard(_dummyPlayer1, new Card(Rank.asso, Suit.denara)),
                new PlayedCard(_dummyPlayer2, new Card(Rank.cavallo, Suit.spade)), // Taglia
                new PlayedCard(_dummyPlayer3, new Card(Rank.cinque, Suit.denara)),
                new PlayedCard(_dummyPlayer4, new Card(Rank.re, Suit.spade))       // Sovrataglia
            };
            var vincitore = _evaluator.EvaluateWinner(tavolo, Suit.spade);
            Assert.Equal(_dummyPlayer4, vincitore.Player);
        }

        [Fact]
        public void EvaluateWinner_UscitaBriscola_NessunoRisponde_VinceUscita()
        {
            // Giocatore 1 esce di Briscola. Gli altri non ce l'hanno e scartano.
            var tavolo = new List<PlayedCard>
            {
                new PlayedCard(_dummyPlayer1, new Card(Rank.quattro, Suit.bastoni)), // Uscita (Briscola)
                new PlayedCard(_dummyPlayer2, new Card(Rank.tre, Suit.coppe)),       // Scarto forte
                new PlayedCard(_dummyPlayer3, new Card(Rank.asso, Suit.denara)),     // Scarto forte
                new PlayedCard(_dummyPlayer4, new Card(Rank.re, Suit.spade))         // Scarto
            };
            var vincitore = _evaluator.EvaluateWinner(tavolo, Suit.bastoni);
            Assert.Equal(_dummyPlayer1, vincitore.Player); // Vince il 4 di bastoni
        }

        [Fact]
        public void EvaluateWinner_NessunaBriscola_VinceSemeUscitaPiuForte()
        {
            var tavolo = new List<PlayedCard>
            {
                new PlayedCard(_dummyPlayer1, new Card(Rank.fante, Suit.bastoni)),
                new PlayedCard(_dummyPlayer2, new Card(Rank.asso, Suit.bastoni)),
                new PlayedCard(_dummyPlayer3, new Card(Rank.quattro, Suit.bastoni)),
                new PlayedCard(_dummyPlayer4, new Card(Rank.re, Suit.bastoni))
            };
            var vincitore = _evaluator.EvaluateWinner(tavolo, Suit.coppe);
            Assert.Equal(_dummyPlayer2, vincitore.Player);
        }

        [Fact]
        public void EvaluateWinner_TuttiSemiDiversi_NessunaBriscola_VinceSemeDiUscita()
        {
            // Ognuno butta un seme diverso. Nessuno butta briscola.
            var tavolo = new List<PlayedCard>
            {
                new PlayedCard(_dummyPlayer1, new Card(Rank.cinque, Suit.denara)), // Uscita
                new PlayedCard(_dummyPlayer2, new Card(Rank.tre, Suit.bastoni)),   // Scarto
                new PlayedCard(_dummyPlayer3, new Card(Rank.asso, Suit.coppe)),    // Scarto
                new PlayedCard(_dummyPlayer4, new Card(Rank.cavallo, Suit.spade))  // Scarto
            };
            // Passiamo una briscola fittizia che nessuno ha giocato
            var vincitore = _evaluator.EvaluateWinner(tavolo, Suit.spade); // In realtà c'è una spada! Correggiamo:

            var tavoloCorretto = new List<PlayedCard>
            {
                new PlayedCard(_dummyPlayer1, new Card(Rank.cinque, Suit.denara)),
                new PlayedCard(_dummyPlayer2, new Card(Rank.tre, Suit.bastoni)),
                new PlayedCard(_dummyPlayer3, new Card(Rank.asso, Suit.coppe)),
                new PlayedCard(_dummyPlayer4, new Card(Rank.cavallo, Suit.coppe)) // Cambiato in coppe per evitare la briscola
            };
            var vincitoreCorretto = _evaluator.EvaluateWinner(tavoloCorretto, Suit.spade);

            // Vince il 5 di Denari perché ha dettato il seme di uscita e nessuno ha tagliato
            Assert.Equal(_dummyPlayer1, vincitoreCorretto.Player);
        }

        [Fact]
        public void EvaluateWinner_GerarchiaRomagnola_TreBatteDue_DueBatteAsso()
        {
            var tavolo = new List<PlayedCard>
            {
                new PlayedCard(_dummyPlayer1, new Card(Rank.asso, Suit.coppe)),
                new PlayedCard(_dummyPlayer2, new Card(Rank.due, Suit.coppe)),
                new PlayedCard(_dummyPlayer3, new Card(Rank.tre, Suit.coppe)),  // Il Tre deve dominare
                new PlayedCard(_dummyPlayer4, new Card(Rank.re, Suit.coppe))
            };
            var vincitore = _evaluator.EvaluateWinner(tavolo, Suit.spade);

            Assert.Equal(_dummyPlayer3, vincitore.Player);
            Assert.Equal(Rank.tre, vincitore.Card.Rank);
        }

        [Fact]
        public void EvaluateWinner_ScartinaDiAltroSeme_ValeZeroAssoluto()
        {
            var tavolo = new List<PlayedCard>
            {
                new PlayedCard(_dummyPlayer1, new Card(Rank.quattro, Suit.denara)),
                new PlayedCard(_dummyPlayer2, new Card(Rank.tre, Suit.bastoni)),
                new PlayedCard(_dummyPlayer3, new Card(Rank.cinque, Suit.denara)),  // Vince
                new PlayedCard(_dummyPlayer4, new Card(Rank.asso, Suit.spade))
            };
            var vincitore = _evaluator.EvaluateWinner(tavolo, Suit.coppe);

            // Vince il 5 di Denari (Player 3)
            Assert.Equal(_dummyPlayer3, vincitore.Player);
        }
    }
}