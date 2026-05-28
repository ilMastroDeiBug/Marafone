using Marafone.Domain.Entities.GameComponents;
using Marafone.Domain.Entities.UsersEntities;
using Marafone.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Marafone.Domain.GameLogic
{
    /// <summary>
    /// Snapshot serializzabile di un Game (DTO piatto senza tipi complessi).
    /// Può risiedere nel Domain per non creare dipendenze circolari.
    /// </summary>
    public class GameSnapshot
    {
        public Guid    Id              { get; set; }
        public int     TargetPoints    { get; set; }
        public bool    IsGameOver      { get; set; }
        public int     CurrentSediaIdx { get; set; }
        public string? BriscolaAttuale { get; set; }

        public PlayerSnap Sq1P1 { get; set; } = new();
        public PlayerSnap Sq1P2 { get; set; } = new();
        public PlayerSnap Sq2P1 { get; set; } = new();
        public PlayerSnap Sq2P2 { get; set; } = new();

        public int Sq1MatchPoints { get; set; }
        public int Sq1HandRaw     { get; set; }
        public int Sq2MatchPoints { get; set; }
        public int Sq2HandRaw     { get; set; }
        public string Sq1Name     { get; set; } = "";
        public string Sq2Name     { get; set; } = "";

        public List<TavoloCardSnap> Tavolo { get; set; } = new();
    }

    public class PlayerSnap
    {
        public Guid   Id   { get; set; }
        public string Name { get; set; } = "";
        public List<CardSnap> Hand { get; set; } = new();
    }

    public class CardSnap
    {
        public string Rank { get; set; } = "";
        public string Suit { get; set; } = "";
    }

    public class TavoloCardSnap
    {
        public Guid   PlayerId { get; set; }
        public string Rank     { get; set; } = "";
        public string Suit     { get; set; } = "";
    }

    // ─────────────────────────────────────────────────────────────────────────
    public static class GameSnapshotMapper
    {
        public static GameSnapshot ToSnapshot(Game game)
        {
            return new GameSnapshot
            {
                Id              = game.Id,
                TargetPoints    = game.TargetPoints,
                IsGameOver      = game.IsGameOver,
                CurrentSediaIdx = game.CurrentPlayerIndex,
                BriscolaAttuale = game.BriscolaAttuale?.ToString(),
                Sq1Name         = game.Squadra1.Name.Value,
                Sq2Name         = game.Squadra2.Name.Value,
                Sq1MatchPoints  = game.Squadra1.MatchPoints.Value,
                Sq1HandRaw      = game.Squadra1.HandPoints.RawValue,
                Sq2MatchPoints  = game.Squadra2.MatchPoints.Value,
                Sq2HandRaw      = game.Squadra2.HandPoints.RawValue,
                Sq1P1           = ToPlayerSnap(game.Squadra1.Player1),
                Sq1P2           = ToPlayerSnap(game.Squadra1.Player2),
                Sq2P1           = ToPlayerSnap(game.Squadra2.Player1),
                Sq2P2           = ToPlayerSnap(game.Squadra2.Player2),
                Tavolo          = game.Tavolo.Select(pc => new TavoloCardSnap
                {
                    PlayerId = pc.Player.Id,
                    Rank     = pc.Card.Rank.ToString(),
                    Suit     = pc.Card.Suit.ToString()
                }).ToList()
            };
        }

        public static Game FromSnapshot(GameSnapshot snap)
        {
            var p1 = RestorePlayer(snap.Sq1P1);
            var p3 = RestorePlayer(snap.Sq1P2);
            var p2 = RestorePlayer(snap.Sq2P1);
            var p4 = RestorePlayer(snap.Sq2P2);

            var squad1 = new Squad(new Name(snap.Sq1Name), p1, p3);
            var squad2 = new Squad(new Name(snap.Sq2Name), p2, p4);

            squad1.AddTrickPoints(snap.Sq1HandRaw);
            squad2.AddTrickPoints(snap.Sq2HandRaw);
            squad1.AddMatchPoints(snap.Sq1MatchPoints);
            squad2.AddMatchPoints(snap.Sq2MatchPoints);

            var game = new Game(squad1, squad2, snap.TargetPoints, skipStartGame: true);
            game.SetId(snap.Id);
            game.ForceSetCurrentPlayerIndex(snap.CurrentSediaIdx);

            if (snap.BriscolaAttuale != null)
                game.RestoreBriscola(Enum.Parse<Suit>(snap.BriscolaAttuale, true));

            if (snap.IsGameOver)
                game.RestoreGameOver();

            var sedie = game.GetSedie();
            foreach (var tc in snap.Tavolo)
            {
                var player = sedie.First(p => p.Id == tc.PlayerId);
                var card   = new Card(Enum.Parse<Rank>(tc.Rank, true), Enum.Parse<Suit>(tc.Suit, true));
                game.RestoreAddToTavolo(player, card);
            }

            return game;
        }

        private static PlayerSnap ToPlayerSnap(Player p) => new PlayerSnap
        {
            Id   = p.Id,
            Name = p.Name.Value,
            Hand = p.Hand.Select(c => new CardSnap
            {
                Rank = c.Rank.ToString(),
                Suit = c.Suit.ToString()
            }).ToList()
        };

        private static Player RestorePlayer(PlayerSnap snap)
        {
            var p = new Player(new Name(snap.Name)) { Id = snap.Id };
            p.ReceiveHand(snap.Hand.Select(c => new Card(
                Enum.Parse<Rank>(c.Rank, true),
                Enum.Parse<Suit>(c.Suit, true)
            )).ToList());
            return p;
        }
    }
}
