using Marafone.Domain.Entities.GameComponents;
using Marafone.Domain.Entities.UsersEntities;
using Marafone.Domain.ValueObjects;
using Marafone.Domain.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Marafone.Domain.GameLogic
{
    public class Match
    {
        // Le due squadre
        public Squad Squadra1 { get; private set; }
        public Squad Squadra2 { get; private set; }
        public Guid Id { get; private set; } = Guid.NewGuid();

        // I giocatori seduti al tavolo in ordine antiorario
        private readonly Player[] _sedie;

        // Lo stato del tavolo
        public Deck Mazzo { get; private set; }
        public Suit? BriscolaAttuale { get; private set; }

        // IL SEGRETO DEL TURNO
        public int CurrentPlayerIndex { get; private set; }
        public Player CurrentPlayer => _sedie[CurrentPlayerIndex];

        // Le carte attualmente buttate sul tavolo (max 4)
        private List<PlayedCard> _tavolo;
        public IReadOnlyList<PlayedCard> Tavolo => _tavolo.AsReadOnly();

        // --- STATO DELLA PARTITA GLOBALE ---
        public bool IsGameOver { get; private set; }
        public Squad VincitorePartita { get; private set; }

        // I nostri arbitri
        private readonly TrickEvaluator _trickEvaluator;
        private readonly MaraffaEvaluator _maraffaEvaluator;

        public Match(Squad sq1, Squad sq2)
        {
            Squadra1 = sq1;
            Squadra2 = sq2;

            _sedie = new Player[] { sq1.Player1, sq2.Player1, sq1.Player2, sq2.Player2 };
            _tavolo = new List<PlayedCard>();

            _trickEvaluator = new TrickEvaluator();
            _maraffaEvaluator = new MaraffaEvaluator();

            IsGameOver = false;
            VincitorePartita = null;
        }

        // --- FASE 1: INIZIO PARTITA E 4 DI DENARI ---
        public void StartNewGame()
        {
            if (IsGameOver) throw new InvalidOperationException("La partita è finita!");

            Mazzo = new Deck();
            for (int i = 0; i < 10; i++)
            {
                Mazzo.ShuffleDeck();
            }
            BriscolaAttuale = null;
            _tavolo.Clear();

            // Svuotiamo le mani in caso di nuova smazzata
            foreach (var player in _sedie)
            {
                player.Hand.Clear();
            }

            // FIX: Filtriamo via il seme finto dei test. Teniamo ESATTAMENTE le 40 carte reali.
            var carteValide = Mazzo.Cards.Where(c => c.Suit != Suit.nessuna_briscola_per_test_finto).ToList();

            if (carteValide.Count != 40)
            {
                throw new Exception($"Bug nel mazzo! Trovate {carteValide.Count} carte valide invece di 40.");
            }

            // Distribuiamo esattamente 10 carte valide a testa
            int sediaCorrente = 0;
            foreach (var carta in carteValide)
            {
                _sedie[sediaCorrente].Hand.Add(carta);
                sediaCorrente = (sediaCorrente + 1) % 4;
            }

            ImpostaTurnoPerIlQuattroDiDenari();
        }

        private void ImpostaTurnoPerIlQuattroDiDenari()
        {
            for (int i = 0; i < 4; i++)
            {
                bool haIlQuattro = _sedie[i].Hand.Any(c => c.Rank == Rank.quattro && c.Suit == Suit.denara);

                if (haIlQuattro)
                {
                    CurrentPlayerIndex = i;
                    return;
                }
            }
            throw new Exception("Nessuno ha il 4 di denara. C'è un bug nel mazzo!");
        }

        // --- FASE 2: SCELTA DELLA BRISCOLA ---
        public void SetBriscola(Player player, Suit scelta)
        {
            if (IsGameOver) throw new InvalidOperationException("La partita è finita!");
            if (player.Id != CurrentPlayer.Id) throw new InvalidOperationException("Non è il tuo turno, non puoi scegliere la briscola!");
            if (BriscolaAttuale != null) throw new InvalidOperationException("La briscola è già stata scelta!");

            BriscolaAttuale = scelta;
        }

        // --- FASE 3: IL GIOCO DELLE CARTE ---
        public void PlayCard(Player player, Card cardDaGiocare)
        {
            if (IsGameOver) throw new InvalidOperationException("La partita è finita!");
            if (player.Id != CurrentPlayer.Id) throw new InvalidOperationException("Non è il tuo turno!");
            if (BriscolaAttuale == null) throw new InvalidOperationException("La briscola non è ancora stata dichiarata!");

            var cartaInMano = player.Hand.FirstOrDefault(c => c.Rank == cardDaGiocare.Rank && c.Suit == cardDaGiocare.Suit);
            if (cartaInMano == null) throw new InvalidOperationException("Non hai questa carta in mano!");

            // Controllo Maraffa
            if (cartaInMano.Suit == BriscolaAttuale && cartaInMano.Rank == Rank.asso)
            {
                int puntiMaraffa = _maraffaEvaluator.Evaluate(player.Hand, BriscolaAttuale.Value);
                if (puntiMaraffa > 0)
                {
                    GetSquadOfPlayer(player).AddMatchPoints(puntiMaraffa);
                    ControllaVittoriaGlobale();
                }
            }

            // Rimuoviamo la carta e mettiamola sul tavolo
            player.Hand.Remove(cartaInMano);
            _tavolo.Add(new PlayedCard(player, cartaInMano));

            if (IsGameOver) return; // Se qualcuno ha vinto con l'accusa, blocca il gioco

            if (_tavolo.Count < 4)
            {
                PassaAlProssimoGiocatore();
            }
            else
            {
                ResolveTrick();
            }
        }

        private void ResolveTrick()
        {
            PlayedCard cardVincente = _trickEvaluator.EvaluateWinner(_tavolo, BriscolaAttuale.Value);
            Player vincitore = cardVincente.Player;

            int puntiGrezziTavolo = _tavolo.Sum(pc => pc.Card.GetScore());
            Squad squadraVincente = GetSquadOfPlayer(vincitore);
            squadraVincente.AddTrickPoints(puntiGrezziTavolo);

            CurrentPlayerIndex = Array.IndexOf(_sedie, vincitore);
            _tavolo.Clear();

            if (_sedie.All(p => p.Hand.Count == 0))
            {
                ChiudiSmazzata(squadraVincente);
            }
        }

        private void ChiudiSmazzata(Squad squadraUltimaPresa)
        {
            squadraUltimaPresa.AddTrickPoints(3);

            Squadra1.AddMatchPoints(Squadra1.HandPoints.NumericValue);
            Squadra2.AddMatchPoints(Squadra2.HandPoints.NumericValue);

            Squadra1.ResetForNewHand();
            Squadra2.ResetForNewHand();

            ControllaVittoriaGlobale();
        }

        private void ControllaVittoriaGlobale()
        {
            bool sq1Vince = Squadra1.MatchPoints.Value >= 41;
            bool sq2Vince = Squadra2.MatchPoints.Value >= 41;

            if (sq1Vince || sq2Vince)
            {
                IsGameOver = true;

                if (sq1Vince && sq2Vince)
                {
                    VincitorePartita = Squadra1.MatchPoints.Value >= Squadra2.MatchPoints.Value ? Squadra1 : Squadra2;
                }
                else
                {
                    VincitorePartita = sq1Vince ? Squadra1 : Squadra2;
                }
            }
        }

        // --- METODI DI SUPPORTO ---
        private void PassaAlProssimoGiocatore()
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % 4;
        }

        private Squad GetSquadOfPlayer(Player p)
        {
            if (Squadra1.Player1.Id == p.Id || Squadra1.Player2.Id == p.Id) return Squadra1;
            if (Squadra2.Player1.Id == p.Id || Squadra2.Player2.Id == p.Id) return Squadra2;
            throw new Exception("Giocatore non trovato in nessuna squadra!");
        }

        // --- HELPERS PER I TEST / DEBUG: permettono di forzare il giocatore di turno ---
        /// <summary>
        /// Forza l'indice del giocatore corrente (0..3). Utile nei test che manipolano le mani.
        /// </summary>
        public void ForceSetCurrentPlayerIndex(int index)
        {
            if (index < 0 || index > 3) throw new ArgumentOutOfRangeException(nameof(index));
            CurrentPlayerIndex = index;
        }

        /// <summary>
        /// Forza il giocatore corrente passando l'istanza Player (deve essere uno dei 4 seduti).
        /// </summary>
        public void ForceSetCurrentPlayer(Player player)
        {
            int idx = Array.IndexOf(_sedie, player);
            if (idx == -1) throw new ArgumentException("Giocatore non seduto al tavolo.", nameof(player));
            CurrentPlayerIndex = idx;
        }
    }
}