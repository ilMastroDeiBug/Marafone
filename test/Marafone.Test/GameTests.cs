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

        // --- HELPER DI TEST ---
        private Match SetupRiggedMatch()
        {
            var match = new Match(_squad1, _squad2);
            do
            {
                match.StartNewGame();
            } while (match.CurrentPlayer.Id != _p1.Id);

            _p1.Hand.Clear();
            _p2.Hand.Clear();
            _p3.Hand.Clear();
            _p4.Hand.Clear();

            _p1.Hand.Add(new Card(Rank.quattro, Suit.denara));

            return match;
        }

        [Fact]
        public void StartNewGame_DistribuisceEsattamente10CarteATutti()
        {
            var match = new Match(_squad1, _squad2);
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
            var match = new Match(_squad1, _squad2);
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

            // Evita che il tavolo inneschi la fine smazzata
            _p1.Hand.Add(new Card(Rank.cinque, Suit.bastoni));

            var c1 = _p1.Hand[0];
            var c2 = new Card(Rank.cavallo, Suit.spade); _p2.Hand.Add(c2);
            var c3 = new Card(Rank.asso, Suit.coppe); _p3.Hand.Add(c3);
            var c4 = new Card(Rank.re, Suit.spade); _p4.Hand.Add(c4);

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
            var c3 = new Card(Rank.sei, Suit.spade); _p3.Hand.Add(c3);
            var c4 = new Card(Rank.sette, Suit.spade); _p4.Hand.Add(c4);

            match.PlayCard(_p1, c1);
            match.PlayCard(_p2, c2);
            match.PlayCard(_p3, c3);
            match.PlayCard(_p4, c4);

            Assert.Empty(_p1.Hand); Assert.Empty(_p4.Hand);

            // 20 punti + 3 (punto di mazzo) = 23 grezzi -> 7 reali.
            Assert.Equal(7, match.Squadra1.MatchPoints.Value);

            Assert.Equal(0, match.Squadra1.HandPoints.RawValue);
            Assert.Equal("0", match.Squadra1.HandPoints.RealValue);
        }

        [Fact]
        public void ChiudiSmazzata_SuperatiI41Punti_DecretataFinePartitaEVincitore()
        {
            var match = SetupRiggedMatch();
            match.SetBriscola(_p1, Suit.denara);

            // Simuliamo una squadra che ha già stravinto la mano
            match.Squadra1.AddTrickPoints(130);

            var c1 = _p1.Hand[0];
            var c2 = new Card(Rank.cinque, Suit.spade); _p2.Hand.Add(c2);
            var c3 = new Card(Rank.sei, Suit.spade); _p3.Hand.Add(c3);
            var c4 = new Card(Rank.sette, Suit.spade); _p4.Hand.Add(c4);

            match.PlayCard(_p1, c1);
            match.PlayCard(_p2, c2);
            match.PlayCard(_p3, c3);
            match.PlayCard(_p4, c4);

            Assert.True(match.IsGameOver);
            Assert.NotNull(match.VincitorePartita);
            Assert.Equal(_squad1.Name, match.VincitorePartita.Name);
        }
        [Fact]
        public void PartitaIntera_SimulazioneFinoAi41Punti_Squadra1Vince()
        {
            // 1. SETUP
            var match = new Match(_squad1, _squad2);

            // --- SMAZZATA 1 ---
            match.StartNewGame();

            // PULIZIA TOTALE: Accediamo ai giocatori tramite il match per essere sicuri dei riferimenti
            var players = new[] { _p1, _p2, _p3, _p4 };
            foreach (var p in players) p.Hand.Clear();

            // Diamo il 4 di Denara a P1 (Squadra 1, Indice 0)
            _p1.Hand.Add(new Card(Rank.quattro, Suit.denara));
            // Diamo una Maraffa di Bastoni a P1
            _p1.Hand.Add(new Card(Rank.asso, Suit.bastoni));
            _p1.Hand.Add(new Card(Rank.due, Suit.bastoni));
            _p1.Hand.Add(new Card(Rank.tre, Suit.bastoni));

            // Reset turno: dobbiamo ricalcolare il turno perché abbiamo cambiato le mani
            // Usiamo un piccolo trick: chiamiamo il metodo privato tramite reflection o più semplicemente
            // simuliamo la logica di inizio.
            match.StartNewGame(); // Rilanciamo per far sì che ImpostaTurno cerchi il NOSTRO 4 di denara
            foreach (var p in players) p.Hand.Clear();
            _p1.Hand.Add(new Card(Rank.quattro, Suit.denara));
            _p1.Hand.Add(new Card(Rank.asso, Suit.bastoni));
            _p1.Hand.Add(new Card(Rank.due, Suit.bastoni));
            _p1.Hand.Add(new Card(Rank.tre, Suit.bastoni));
            // Riempiamo per arrivare a 10 carte (il dominio vuole 10 carte per giocare bene)
            for (int i = 0; i < 6; i++) _p1.Hand.Add(new Card(Rank.cinque, Suit.coppe));
            for (int i = 0; i < 10; i++)
            {
                _p2.Hand.Add(new Card(Rank.sei, Suit.coppe));
                _p3.Hand.Add(new Card(Rank.sette, Suit.coppe));
                _p4.Hand.Add(new Card(Rank.quattro, Suit.spade));
            }

            // Ora P1 è DI TURNO sicuramente
            match.SetBriscola(_p1, Suit.bastoni);

            // Gioca l'Asso (Maraffa)
            match.PlayCard(_p1, new Card(Rank.asso, Suit.bastoni));
            Assert.Equal(3, _squad1.MatchPoints.Value);

            // Facciamo 30 punti grezzi
            _squad1.AddTrickPoints(30);

            // Chiudiamo la smazzata 1
            foreach (var p in players) p.Hand.Clear();
            _p1.Hand.Add(new Card(Rank.re, Suit.bastoni));
            _p2.Hand.Add(new Card(Rank.fante, Suit.coppe));
            _p3.Hand.Add(new Card(Rank.cavallo, Suit.coppe));
            _p4.Hand.Add(new Card(Rank.asso, Suit.spade));

            match.PlayCard(_p1, _p1.Hand[0]);
            match.PlayCard(_p2, _p2.Hand[0]);
            match.PlayCard(_p3, _p3.Hand[0]);
            match.PlayCard(_p4, _p4.Hand[0]);

            Assert.Equal(16, _squad1.MatchPoints.Value);

            // --- SMAZZATA 2 ---
            match.StartNewGame();
            foreach (var p in players) p.Hand.Clear();

            // Questa volta diamo il 4 di Denara a P1 di nuovo per comodità del test
            _p1.Hand.Add(new Card(Rank.quattro, Suit.denara));
            for (int i = 0; i < 9; i++) _p1.Hand.Add(new Card(Rank.cinque, Suit.spade));
            for (int i = 0; i < 10; i++)
            {
                _p2.Hand.Add(new Card(Rank.sei, Suit.spade));
                _p3.Hand.Add(new Card(Rank.sette, Suit.spade));
                _p4.Hand.Add(new Card(Rank.quattro, Suit.spade));
            }

            match.SetBriscola(_p1, Suit.spade);
            _squad1.AddTrickPoints(72); // Punti per arrivare a 41

            // Ultime carte
            _p1.Hand.Clear(); _p1.Hand.Add(new Card(Rank.tre, Suit.spade));
            _p2.Hand.Clear(); _p2.Hand.Add(new Card(Rank.quattro, Suit.coppe));
            _p3.Hand.Clear(); _p3.Hand.Add(new Card(Rank.cinque, Suit.coppe));
            _p4.Hand.Clear(); _p4.Hand.Add(new Card(Rank.sei, Suit.coppe));

            match.PlayCard(_p1, _p1.Hand[0]);
            match.PlayCard(_p2, _p2.Hand[0]);
            match.PlayCard(_p3, _p3.Hand[0]);
            match.PlayCard(_p4, _p4.Hand[0]);

            // FINALE
            Assert.True(match.IsGameOver);
            Assert.Equal(_squad1.Name, match.VincitorePartita.Name);
        }
    }
}