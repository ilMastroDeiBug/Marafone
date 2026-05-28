using Marafone.Domain.Entities.GameComponents;
using Marafone.Domain.Entities.UsersEntities;
using Marafone.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Marafone.Domain.GameLogic
{
    /// <summary>
    /// Punti partita obiettivo. Le tre modalità del Marafone.
    /// </summary>
    public enum GameTarget
    {
        Corta = 21,
        Media = 31,
        Lunga = 41
    }

    public class Game
    {
        // Le due squadre
        public Squad Squadra1 { get; private set; }
        public Squad Squadra2 { get; private set; }
        public Guid Id { get; private set; }

        // Obiettivo punti partita (21 / 31 / 41)
        public int TargetPoints { get; private set; }

        // I giocatori seduti al tavolo in senso ANTIORARIO
        // Posizione: 0=Sud(P1sq1), 1=Ovest(P1sq2), 2=Nord(P2sq1), 3=Est(P2sq2)
        private readonly Player[] _sedie;

        // Lo stato del tavolo
        public Deck Mazzo { get; private set; }
        public Suit? BriscolaAttuale { get; private set; }

        // Turno corrente
        public int CurrentPlayerIndex { get; private set; }
        public Player CurrentPlayer => _sedie[CurrentPlayerIndex];

        // Carte attualmente sul tavolo (max 4)
        private List<PlayedCard> _tavolo;
        public IReadOnlyList<PlayedCard> Tavolo => _tavolo.AsReadOnly();

        // Stato globale
        public bool IsGameOver { get; private set; }
        public Squad? VincitorePartita { get; private set; }

        // Arbitri
        private readonly TrickEvaluator _trickEvaluator;
        private readonly MaraffaEvaluator _maraffaEvaluator;

        public Game(Squad sq1, Squad sq2, int targetPoints = 41, bool skipStartGame = false)
        {
            if (targetPoints != 21 && targetPoints != 31 && targetPoints != 41)
                throw new ArgumentException("TargetPoints deve essere 21, 31 o 41.");

            Squadra1 = sq1;
            Squadra2 = sq2;
            TargetPoints = targetPoints;

            // Senso antiorario: Sq1P1(Sud) → Sq2P1(Ovest) → Sq1P2(Nord) → Sq2P2(Est) → ...
            _sedie = new Player[] { sq1.Player1, sq2.Player1, sq1.Player2, sq2.Player2 };
            _tavolo = new List<PlayedCard>();

            _trickEvaluator = new TrickEvaluator();
            _maraffaEvaluator = new MaraffaEvaluator();

            IsGameOver = false;
            VincitorePartita = null;
            Id = Guid.NewGuid();
        }

        // ──────────────────────────────────────────────────────────────────
        // FASE 1: INIZIO SMAZZATA E TURNO AL 4 DI DENARI
        // ──────────────────────────────────────────────────────────────────
        public void StartNewGame()
        {
            if (IsGameOver) throw new InvalidOperationException("La partita è finita!");

            Mazzo = new Deck();
            for (int i = 0; i < 10; i++) Mazzo.ShuffleDeck();

            BriscolaAttuale = null;
            _tavolo.Clear();

            foreach (var player in _sedie)
                player.Hand.Clear();

            // Distribuzione in senso antiorario: carta per carta, un giro alla volta
            int sedia = 0;
            foreach (var carta in Mazzo.Cards)
            {
                _sedie[sedia].Hand.Add(carta);
                sedia = (sedia + 1) % 4;
            }

            ImpostaTurnoPerIlQuattroDiDenari();
        }

        private void ImpostaTurnoPerIlQuattroDiDenari()
        {
            for (int i = 0; i < 4; i++)
            {
                if (_sedie[i].Hand.Any(c => c.Rank == Rank.quattro && c.Suit == Suit.denara))
                {
                    CurrentPlayerIndex = i;
                    return;
                }
            }
            throw new Exception("Nessuno ha il 4 di denara. Bug nel mazzo!");
        }

        // ──────────────────────────────────────────────────────────────────
        // FASE 2: SCELTA DELLA BRISCOLA (solo chi ha il 4 di Denari)
        // ──────────────────────────────────────────────────────────────────
        public void SetBriscola(Player player, Suit scelta)
        {
            if (IsGameOver) throw new InvalidOperationException("La partita è finita!");
            if (player.Id != CurrentPlayer.Id)
                throw new InvalidOperationException("Non è il tuo turno — non puoi scegliere la briscola!");
            if (BriscolaAttuale != null)
                throw new InvalidOperationException("La briscola è già stata scelta!");

            BriscolaAttuale = scelta;
            // Il turno rimane allo stesso giocatore (gioca per primo)
        }

        // ──────────────────────────────────────────────────────────────────
        // FASE 3: GIOCO CARTE (con obbligo di rispondere al seme)
        // ──────────────────────────────────────────────────────────────────
        public void PlayCard(Player player, Card cardDaGiocare)
        {
            if (IsGameOver) throw new InvalidOperationException("La partita è finita!");
            if (player.Id != CurrentPlayer.Id)
                throw new InvalidOperationException("Non è il tuo turno!");
            if (BriscolaAttuale == null)
                throw new InvalidOperationException("La briscola non è ancora stata dichiarata!");

            var cartaInMano = player.Hand.FirstOrDefault(c =>
                c.Rank == cardDaGiocare.Rank && c.Suit == cardDaGiocare.Suit);
            if (cartaInMano == null)
                throw new InvalidOperationException("Non hai questa carta in mano!");

            // ── OBBLIGO DI RISPONDERE AL SEME ────────────────────────────
            // Se il tavolo non è vuoto (qualcuno ha già giocato), si deve
            // rispondere al seme di uscita SE si hanno carte di quel seme in mano.
            // Unica eccezione: si può sempre giocare briscola al posto del seme.
            if (_tavolo.Count > 0)
            {
                Suit semeUscita = _tavolo[0].Card.Suit;

                // Controlla se il giocatore ha carte del seme di uscita
                bool haCarte_semeUscita = player.Hand.Any(c => c.Suit == semeUscita);

                if (haCarte_semeUscita)
                {
                    // Deve giocare il seme di uscita (o briscola se il seme di uscita È la briscola)
                    bool staRispondendoAlSeme = cartaInMano.Suit == semeUscita;
                    bool staGiocandoBriscola  = cartaInMano.Suit == BriscolaAttuale;

                    if (!staRispondendoAlSeme && !staGiocandoBriscola)
                        throw new InvalidOperationException(
                            $"Devi rispondere al seme ({semeUscita}) oppure giocare una briscola!");
                }
                // Se non ha carte del seme di uscita, può giocare qualsiasi carta
            }

            // ── CONTROLLO MARAFONE (solo giocando l'Asso di briscola) ─────
            if (cartaInMano.Suit == BriscolaAttuale && cartaInMano.Rank == Rank.asso)
            {
                int puntiMaraffa = _maraffaEvaluator.Evaluate(player.Hand, BriscolaAttuale.Value);
                if (puntiMaraffa > 0)
                {
                    GetSquadOfPlayer(player).AddMatchPoints(puntiMaraffa);
                    ControllaVittoriaGlobale();
                }
            }

            // Rimuovi carta dalla mano e mettila sul tavolo
            player.Hand.Remove(cartaInMano);
            _tavolo.Add(new PlayedCard(player, cartaInMano));

            if (IsGameOver) return; // Vittoria per accusa

            if (_tavolo.Count < 4)
            {
                PassaAlProssimoGiocatore();
            }
            else
            {
                ResolveTrick();
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // ABBANDONO PARTITA
        // ──────────────────────────────────────────────────────────────────
        public void Forfeit(Player playerCheAbbandona)
        {
            if (IsGameOver) return;

            Squad squadraVincente = GetSquadOfPlayer(playerCheAbbandona) == Squadra1
                ? Squadra2
                : Squadra1;

            // Assegna abbastanza punti da superare sicuramente il target
            squadraVincente.AddMatchPoints(TargetPoints + 1);
            IsGameOver = true;
            VincitorePartita = squadraVincente;
        }

        // ──────────────────────────────────────────────────────────────────
        // LOGICA INTERNA
        // ──────────────────────────────────────────────────────────────────
        private void ResolveTrick()
        {
            PlayedCard cardVincente = _trickEvaluator.EvaluateWinner(_tavolo, BriscolaAttuale!.Value);
            Player vincitore = cardVincente.Player;

            int puntiGrezziTavolo = _tavolo.Sum(pc => pc.Card.GetScore());
            Squad squadraVincente = GetSquadOfPlayer(vincitore);
            squadraVincente.AddTrickPoints(puntiGrezziTavolo);

            // Il vincitore della presa gioca per primo (senso antiorario mantenuto)
            CurrentPlayerIndex = Array.IndexOf(_sedie, vincitore);
            _tavolo.Clear();

            if (_sedie.All(p => p.Hand.Count == 0))
            {
                ChiudiSmazzata(squadraVincente);
            }
        }

        private void ChiudiSmazzata(Squad squadraUltimaPresa)
        {
            // Punto di mazzo: +3 punti grezzi a chi vince l'ultima presa
            squadraUltimaPresa.AddTrickPoints(3);

            // Conversione: ogni 3 punti grezzi = 1 punto partita (i resti vengono persi)
            Squadra1.AddMatchPoints(Squadra1.HandPoints.NumericValue);
            Squadra2.AddMatchPoints(Squadra2.HandPoints.NumericValue);

            Squadra1.ResetForNewHand();
            Squadra2.ResetForNewHand();

            ControllaVittoriaGlobale();
        }

        private void ControllaVittoriaGlobale()
        {
            bool sq1Vince = Squadra1.MatchPoints.Value >= TargetPoints;
            bool sq2Vince = Squadra2.MatchPoints.Value >= TargetPoints;

            if (sq1Vince || sq2Vince)
            {
                IsGameOver = true;
                if (sq1Vince && sq2Vince)
                {
                    VincitorePartita = Squadra1.MatchPoints.Value >= Squadra2.MatchPoints.Value
                        ? Squadra1 : Squadra2;
                }
                else
                {
                    VincitorePartita = sq1Vince ? Squadra1 : Squadra2;
                }
            }
        }

        // Avanza in senso antiorario (indice + 1 nella lista _sedie già ordinata antiorariamente)
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

        // ──────────────────────────────────────────────────────────────────
        // HELPERS PER TEST / DEBUG
        // ──────────────────────────────────────────────────────────────────
        public void ForceSetCurrentPlayerIndex(int index)
        {
            if (index < 0 || index > 3) throw new ArgumentOutOfRangeException(nameof(index));
            CurrentPlayerIndex = index;
        }

        public void ForceSetCurrentPlayer(Player player)
        {
            int idx = Array.IndexOf(_sedie, player);
            if (idx == -1) throw new ArgumentException("Giocatore non seduto al tavolo.", nameof(player));
            CurrentPlayerIndex = idx;
        }

        /// <summary>
        /// Espone i 4 giocatori in ordine di seduta (utile per la serializzazione).
        /// </summary>
        public Player[] GetSedie() => _sedie;

        // ── METODI DI RESTORE (usati da GameFactory durante deserializzazione) ──
        internal void SetId(Guid id) => Id = id;
        internal void RestoreBriscola(Suit briscola) => BriscolaAttuale = briscola;
        internal void RestoreGameOver()
        {
            IsGameOver = true;
            bool sq1Vince = Squadra1.MatchPoints.Value >= TargetPoints;
            bool sq2Vince = Squadra2.MatchPoints.Value >= TargetPoints;
            VincitorePartita = sq1Vince ? Squadra1 : Squadra2;
        }
        internal void RestoreAddToTavolo(Entities.UsersEntities.Player player, Entities.GameComponents.Card card)
        {
            _tavolo.Add(new PlayedCard(player, card));
        }
    }
}