using Google.Cloud.Firestore;
using Marafone.Application.Interfaces;
using Marafone.Domain.GameLogic;
using Marafone.Infrastructure.DatabaseModels;
using System;
using System.Text.Json;

namespace Marafone.Infrastructure.Repositories
{
    public class FirestoreMatchRepository : IMatchRepository
    {
        private readonly FirestoreDb _db;
        private const string CollectionName = "Matches";

        private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
        {
            PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public FirestoreMatchRepository(string projectId)
        {
            _db = FirestoreDb.Create(projectId);
        }

        public void Save(Game match)
        {
            var snapshot = GameSnapshotMapper.ToSnapshot(match);
            string json  = JsonSerializer.Serialize(snapshot, _jsonOpts);

            var doc = new MatchDocument
            {
                Id        = match.Id.ToString(),
                JsonState = json
            };

            DocumentReference docRef = _db.Collection(CollectionName).Document(match.Id.ToString());
            docRef.SetAsync(doc).Wait();
        }

        public Game? GetById(Guid id)
        {
            DocumentReference docRef = _db.Collection(CollectionName).Document(id.ToString());
            DocumentSnapshot  snap   = docRef.GetSnapshotAsync().Result;

            if (!snap.Exists) return null;

            MatchDocument doc         = snap.ConvertTo<MatchDocument>();
            var           gameSnapshot = JsonSerializer.Deserialize<GameSnapshot>(doc.JsonState, _jsonOpts)
                ?? throw new Exception("Deserializzazione fallita: JSON non valido");

            return GameSnapshotMapper.FromSnapshot(gameSnapshot);
        }

        public void Remove(Guid id)
        {
            DocumentReference docRef = _db.Collection(CollectionName).Document(id.ToString());
            docRef.DeleteAsync().Wait();
        }
    }
}