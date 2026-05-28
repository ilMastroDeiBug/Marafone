using Marafone.Domain.Entities;
using Marafone.Domain.Entities.GameComponents;
using Marafone.Domain.Entities.UsersEntities;
using Marafone.Domain.GameLogic;
using Marafone.Domain.ValueObjects;
using System;
using System.Linq;
using Xunit;

namespace Marafone.Tests.Domain
{
    public class MatchTests
    {
        private Player _p1, _p2, _p3, _p4;
        private Squad _squad1, _squad2;

        public MatchTests()
        {
            _p1 = new Player(new Name("Sud (P1)"));
            _p2 = new Player(new Name("Est (P2)"));
            _p3 = new Player(new Name("Nord (P3)"));
            _p4 = new Player(new Name("Ovest (P4)"));

            _squad1 = new Squad(new Name("Team A"), _p1, _p3);
            _squad2 = new Squad(new Name("Team B"), _p2, _p4);
        }

        /// <summary>
        /// Setup rigged: avvia il gioco e forza P1 come corrente,
        /// svuota le mani e mette solo il 4 di Denari a P1.
        /// </summary>
        private Game SetupRiggedMatch()
        {
            var match = new Game(_squad1, _squad2);
            match.StartNewGame();

            _p1.Hand.Clear(); _p2.Hand.Clear();
            _p3.Hand.Clear(); _p4.Hand.Clear();

            _p1.Hand.Add(new Card(Rank.quattro, Suit.denara));
            match.ForceSetCurrentPlayer(_p1);

            return match;
        }

        [Fact]
        public void StartNewGame_DistribuisceEsattamente10CarteATutti()
        {
            var match = new Game(_squad1, _squad2);
            match.StartNewGame();
            Assert.Equal(10, _p1.Hand.Count);
            Assert.Equal(10, _p2.Hand.Count);
            Assert.Equal(10, _p3.Hand.Count);
            Assert.Equal(10, _p4.Hand.Count);
            Assert.Empty(match.Tavolo);
            Assert.Null(match.BriscolaAttuale);
            Assert.False(match.IsGameOver);
        }

        [Fact]
        public void StartNewGame_ChiHaIlQuattroDiDenaraEIlCurrentPlayer()
        {
            var match = new Game(_squad1, _squad2);
            match.StartNewGame();
            var playerConIlQuattro = new[] { _p1, _p2, _p3, _p4 }
                .First(p => p.Hand.Any(c => c.Rank == Rank.quattro && c.Suit == Suit.denara));
            Assert.Equal(playerConIlQuattro.Id, match.CurrentPlayer.Id);
        }

        [Fact]
        public void SetBriscola_FuoriTurno_LanciaEccezione()
        {
            var match = SetupRiggedMatch();
            Assert.Throws<InvalidOperationException>(() => match.SetBriscola(_p2, Suit.bastoni));
        }

        [Fact]
        public void SetBriscola_Regolare_NonPassaIlTurno()
        {
            var match = SetupRiggedMatch();
            match.SetBriscola(_p1, Suit.bastoni);
            Assert.Equal(Suit.bastoni, match.BriscolaAttuale);
            Assert.Equal(_p1.Id, match.CurrentPlayer.Id);
        }

        [Fact]
        public void SetBriscola_GiaDichiarata_LanciaEccezione()
        {
            var match = SetupRiggedMatch();
            match.SetBriscola(_p1, Suit.bastoni);
            Assert.Throws<InvalidOperationException>(() => match.SetBriscola(_p1, Suit.coppe));
        }

        [Fact]
        public void PlayCard_SenzaAverChiamatoBriscola_LanciaEccezione()
        {
            var match = SetupRiggedMatch();
            var carta = _p1.Hand[0];
            Assert.Throws<InvalidOperationException>(() => match.PlayCard(_p1, carta));
        }

        [Fact]
        public void PlayCard_FuoriTurno_LanciaEccezione()
        {
            var match = SetupRiggedMatch();
            match.SetBriscola(_p1, Suit.spade);
            _p2.Hand.Add(new Card(Rank.asso, Suit.spade));
            Assert.Throws<InvalidOperationException>(() => match.PlayCard(_p2, _p2.Hand[0]));
        }

        [Fact]
        public void PlayCard_CartaNonInMano_LanciaEccezione()
        {
            var match = SetupRiggedMatch();
            match.SetBriscola(_p1, Suit.spade);
            var cartaInventata = new Card(Rank.asso, Suit.coppe);
            Assert.Throws<InvalidOperationException>(() => match.PlayCard(_p1, cartaInventata));
        }

        [Fact]
        public void PlayCard_MossaLegale_RimuoveDaManoMetteSulTavoloEPassaTurno()
        {
            var match = SetupRiggedMatch();
            match.SetBriscola(_p1, Suit.spade);
            var carta = _p1.Hand[0];
            match.PlayCard(_p1, carta);
            Assert.Empty(_p1.Hand);
            Assert.Single(match.Tavolo);
            Assert.Equal(carta.Rank, match.Tavolo[0].Card.Rank);
            Assert.Equal(_p2.Id, match.CurrentPlayer.Id);
        }

        [Fact]
        public void PlayCard_AssoDiBriscolaConDueETre_AssegnaPuntiPartitaAllaSquadra()
        {
            var match = SetupRiggedMatch();
            match.SetBriscola(_p1, Suit.bastoni);
            var assoDiBastoni = new Card(Rank.asso, Suit.bastoni);
            _p1.Hand.Add(assoDiBastoni);
            _p1.Hand.Add(new Card(Rank.due, Suit.bastoni));
            _p1.Hand.Add(new Card(Rank.tre, Suit.bastoni));
            match.PlayCard(_p1, assoDiBastoni);
            Assert.Equal(3, match.Squadra1.MatchPoints.Value);
        }

        [Fact]
        public void PlayCard_GiocaTreDiBriscola_NonInnescaMaraffa()
        {
            var match = SetupRiggedMatch();
            match.SetBriscola(_p1, Suit.bastoni);
            var treDiBastoni = new Card(Rank.tre, Suit.bastoni);
            _p1.Hand.Add(new Card(Rank.asso, Suit.bastoni));
            _p1.Hand.Add(new Card(Rank.due, Suit.bastoni));
            _p1.Hand.Add(treDiBastoni);
            match.PlayCard(_p1, treDiBastoni);
            Assert.Equal(0, match.Squadra1.MatchPoints.Value);
        }

        [Fact]
        public void PlayCard_QuartaCarta_AssegnaPuntiAzzeraTavoloEDaTurnoAlVincitore()
        {
            var match = SetupRiggedMatch();
            match.SetBriscola(_p1, Suit.coppe);
            _p1.Hand.Add(new Card(Rank.cinque, Suit.bastoni));
            var c1 = _p1.Hand[0];
            var c2 = new Card(Rank.cavallo, Suit.spade); _p2.Hand.Add(c2);
            var c3 = new Card(Rank.asso,    Suit.coppe);  _p3.Hand.Add(c3);
            var c4 = new Card(Rank.re,      Suit.spade);  _p4.Hand.Add(c4);
            match.PlayCard(_p1, c1);
            match.PlayCard(_p2, c2);
            match.PlayCard(_p3, c3);
            Assert.Equal(3, match.Tavolo.Count);
            Assert.Equal(0, match.Squadra1.HandPoints.RawValue);
            match.PlayCard(_p4, c4);
            Assert.Empty(match.Tavolo);
            Assert.Equal(5, match.Squadra1.HandPoints.RawValue);
            Assert.Equal(0, match.Squadra2.HandPoints.RawValue);
            Assert.Equal(_p3.Id, match.CurrentPlayer.Id);
        }

        [Fact]
        public void PlayCard_UltimaPresa_PuntoDiMazzoEConversionePuntiPartita()
        {
            var match = SetupRiggedMatch();
            match.SetBriscola(_p1, Suit.denara);
            match.Squadra1.AddTrickPoints(20);
            var c1 = _p1.Hand[0];
            var c2 = new Card(Rank.cinque, Suit.spade); _p2.Hand.Add(c2);
            var c3 = new Card(Rank.sei,    Suit.spade);  _p3.Hand.Add(c3);
            var c4 = new Card(Rank.sette,  Suit.spade);  _p4.Hand.Add(c4);
            match.PlayCard(_p1, c1);
            match.PlayCard(_p2, c2);
            match.PlayCard(_p3, c3);
            match.PlayCard(_p4, c4);
            Assert.Empty(_p1.Hand);
            Assert.Equal(7, match.Squadra1.MatchPoints.Value);
            Assert.Equal(0, match.Squadra1.HandPoints.RawValue);
        }

        [Fact]
        public void ChiudiSmazzata_SuperatiI41Punti_DecretataFinePartitaEVincitore()
        {
            var match = SetupRiggedMatch();
            match.SetBriscola(_p1, Suit.denara);
            match.Squadra1.AddTrickPoints(130);
            var c1 = _p1.Hand[0];
            var c2 = new Card(Rank.cinque, Suit.spade); _p2.Hand.Add(c2);
            var c3 = new Card(Rank.sei,    Suit.spade);  _p3.Hand.Add(c3);
            var c4 = new Card(Rank.sette,  Suit.spade);  _p4.Hand.Add(c4);
            match.PlayCard(_p1, c1);
            match.PlayCard(_p2, c2);
            match.PlayCard(_p3, c3);
            match.PlayCard(_p4, c4);
            Assert.True(match.IsGameOver);
            Assert.NotNull(match.VincitorePartita);
            Assert.Equal(_squad1.Name, match.VincitorePartita!.Name);
        }

        [Fact]
        public void ObbligoRisposta_GiocaCarta_SbagliataLanciaEccezione()
        {
            var match = SetupRiggedMatch();
            match.SetBriscola(_p1, Suit.denara);
            var c1 = new Card(Rank.re, Suit.coppe); _p1.Hand.Add(c1);
            match.PlayCard(_p1, c1);
            // P2 ha un coppe ma prova a giocare uno spade (non è briscola né seme di uscita)
            var coppeP2 = new Card(Rank.asso, Suit.coppe); _p2.Hand.Add(coppeP2);
            var spadeP2 = new Card(Rank.tre,  Suit.spade); _p2.Hand.Add(spadeP2);
            Assert.Throws<InvalidOperationException>(() => match.PlayCard(_p2, spadeP2));
        }

        [Fact]
        public void ObbligoRisposta_HaBriscola_PuoGiocarla()
        {
            var match = SetupRiggedMatch();
            match.SetBriscola(_p1, Suit.denara);
            var c1 = new Card(Rank.re, Suit.coppe); _p1.Hand.Add(c1);
            match.PlayCard(_p1, c1);
            // P2 non ha coppe ma ha briscola (denara) → può giocarla
            var denaraP2 = new Card(Rank.asso, Suit.denara); _p2.Hand.Add(denaraP2);
            var exception = Record.Exception(() => match.PlayCard(_p2, denaraP2));
            Assert.Null(exception);
        }

        [Fact]
        public void PartitaIntera_SimulazioneFinoAi41Punti_Squadra1Vince()
        {
            // ── PRIMA SMAZZATA ─────────────────────────────────────────────
            // Tutti giocano bastoni (il seme di briscola) → nessun problema obbligo risposta
            _p1.Hand.Clear(); _p2.Hand.Clear(); _p3.Hand.Clear(); _p4.Hand.Clear();
            _p1.Hand.Add(new Card(Rank.quattro, Suit.denara));
            _p1.Hand.Add(new Card(Rank.asso,    Suit.bastoni));
            _p1.Hand.Add(new Card(Rank.due,     Suit.bastoni));
            _p1.Hand.Add(new Card(Rank.tre,     Suit.bastoni));
            _p1.Hand.Add(new Card(Rank.re,      Suit.bastoni));
            for (int i = 0; i < 5; i++)
            {
                _p2.Hand.Add(new Card(Rank.cinque, Suit.bastoni));
                _p3.Hand.Add(new Card(Rank.sei,    Suit.bastoni));
                _p4.Hand.Add(new Card(Rank.sette,  Suit.bastoni));
            }

            var match = new Game(_squad1, _squad2);
            match.StartNewGame(); // Distribuisce, poi sovrascriviamo subito
            _p1.Hand.Clear(); _p2.Hand.Clear(); _p3.Hand.Clear(); _p4.Hand.Clear();
            _p1.Hand.Add(new Card(Rank.quattro, Suit.denara));
            _p1.Hand.Add(new Card(Rank.asso,    Suit.bastoni));
            _p1.Hand.Add(new Card(Rank.due,     Suit.bastoni));
            _p1.Hand.Add(new Card(Rank.tre,     Suit.bastoni));
            _p1.Hand.Add(new Card(Rank.re,      Suit.bastoni));
            for (int i = 0; i < 5; i++)
            {
                _p2.Hand.Add(new Card(Rank.cinque, Suit.bastoni));
                _p3.Hand.Add(new Card(Rank.sei,    Suit.bastoni));
                _p4.Hand.Add(new Card(Rank.sette,  Suit.bastoni));
            }

            match.ForceSetCurrentPlayer(_p1);
            match.SetBriscola(_p1, Suit.bastoni);

            // GIRO 1: Marafone! Asso vince tutti i bastoni
            match.PlayCard(_p1, new Card(Rank.asso, Suit.bastoni));
            Assert.Equal(3, _squad1.MatchPoints.Value);
            match.PlayCard(_p2, _p2.Hand[0]);
            match.PlayCard(_p3, _p3.Hand[0]);
            match.PlayCard(_p4, _p4.Hand[0]);
            // Tavolo azzerato, turno torna a P1 (asso vince)

            // Simuliamo prese intermedie aggiungendo punti grezzi
            _squad1.AddTrickPoints(30);

            // GIRO 2 (ultima della prima smazzata): Re vince su cinque/sei/sette
            match.PlayCard(_p1, new Card(Rank.re, Suit.bastoni));
            match.PlayCard(_p2, _p2.Hand[0]);
            match.PlayCard(_p3, _p3.Hand[0]);
            match.PlayCard(_p4, _p4.Hand[0]);

            // Smazzata chiusa: 3(maraffa) + (30+3+1+3mazzo)/3 = 3 + 37/3 = 3+12=15
            Assert.False(match.IsGameOver);
            int ptiSm1 = _squad1.MatchPoints.Value;
            Assert.True(ptiSm1 >= 3); // almeno la maraffa

            // ── SECONDA SMAZZATA ──────────────────────────────────────────
            match.StartNewGame();
            _p1.Hand.Clear(); _p2.Hand.Clear(); _p3.Hand.Clear(); _p4.Hand.Clear();
            _p1.Hand.Add(new Card(Rank.quattro, Suit.denara));
            _p1.Hand.Add(new Card(Rank.asso,    Suit.spade));
            _p1.Hand.Add(new Card(Rank.due,     Suit.spade));
            _p1.Hand.Add(new Card(Rank.tre,     Suit.spade));
            _p1.Hand.Add(new Card(Rank.re,      Suit.spade));
            for (int i = 0; i < 5; i++)
            {
                _p2.Hand.Add(new Card(Rank.cinque, Suit.spade));
                _p3.Hand.Add(new Card(Rank.sei,    Suit.spade));
                _p4.Hand.Add(new Card(Rank.sette,  Suit.spade));
            }

            match.ForceSetCurrentPlayer(_p1);
            match.SetBriscola(_p1, Suit.spade);

            // Aggiungi punti grezzi extra (simula prese intermedie già giocate)
            _squad1.AddTrickPoints(90);

            // Ora gioca TUTTI i giri per svuotare le mani e attivare ChiudiSmazzata
            // Giro 1: Asso (Marafone!)
            match.PlayCard(_p1, new Card(Rank.asso, Suit.spade));
            match.PlayCard(_p2, _p2.Hand[0]);
            match.PlayCard(_p3, _p3.Hand[0]);
            match.PlayCard(_p4, _p4.Hand[0]);

            // Giro 2: Re
            match.PlayCard(_p1, new Card(Rank.re, Suit.spade));
            match.PlayCard(_p2, _p2.Hand[0]);
            match.PlayCard(_p3, _p3.Hand[0]);
            match.PlayCard(_p4, _p4.Hand[0]);

            // Giro 3: Tre
            match.PlayCard(_p1, new Card(Rank.tre, Suit.spade));
            match.PlayCard(_p2, _p2.Hand[0]);
            match.PlayCard(_p3, _p3.Hand[0]);
            match.PlayCard(_p4, _p4.Hand[0]);

            // Giro 4: Due
            match.PlayCard(_p1, new Card(Rank.due, Suit.spade));
            match.PlayCard(_p2, _p2.Hand[0]);
            match.PlayCard(_p3, _p3.Hand[0]);
            match.PlayCard(_p4, _p4.Hand[0]);

            // Giro 5 (ultima = punto di mazzo a P1): Quattro di Denari
            match.PlayCard(_p1, new Card(Rank.quattro, Suit.denara));
            match.PlayCard(_p2, _p2.Hand[0]);
            match.PlayCard(_p3, _p3.Hand[0]);
            match.PlayCard(_p4, _p4.Hand[0]);

            // Smazzata chiusa: ChiudiSmazzata converte i punti e verifica la vittoria
            Assert.True(match.IsGameOver);
            Assert.Equal(_squad1.Name, match.VincitorePartita!.Name);

        }
    }
}