using Marafone.Application.Interfaces;
using Marafone.Domain.GameLogic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Marafone.Infrastructure.Repositories
{
    /// <summary>
    /// Repository in-memory per sviluppo locale e test (thread-safe).
    /// </summary>
    public class InMemoryMatchRepository : IMatchRepository
    {
        private readonly ConcurrentDictionary<Guid, Game> _store = new();

        public void Save(Game match)
        {
            _store[match.Id] = match;
        }

        public Game? GetById(Guid id)
        {
            _store.TryGetValue(id, out var match);
            return match;
        }

        public void Remove(Guid id)
        {
            _store.TryRemove(id, out _);
        }

        /// <summary>Elenca gli ID di tutte le partite attive (per la lobby).</summary>
        public IEnumerable<Guid> GetAllIds() => _store.Keys;

        /// <summary>Elenca tutte le partite non ancora iniziate (briscola non scelta).</summary>
        public IEnumerable<Game> GetOpenMatches() =>
            _store.Values.Where(g => !g.IsGameOver && g.BriscolaAttuale == null);
    }
}
