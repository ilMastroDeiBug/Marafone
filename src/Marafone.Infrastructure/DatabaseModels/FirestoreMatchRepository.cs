using Google.Cloud.Firestore;
using Marafone.Application.Interfaces;
using Marafone.Domain.GameLogic;
using Marafone.Infrastructure.DatabaseModels;
using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json; // Usiamo il serializzatore nativo di C#
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace Marafone.Infrastructure.Repositories
{
    public class FirestoreMatchRepository : IMatchRepository
    {
        private readonly FirestoreDb _db;
        private readonly string _collectionName = "Matches";

        public FirestoreMatchRepository(string projectId)
        {
            // Crea la connessione. Usa in automatico la variabile GOOGLE_APPLICATION_CREDENTIALS
            _db = FirestoreDb.Create(projectId);
        }

        // Il metodo Save che l'Application Layer chiama (es. dopo aver giocato una carta)
        public void Save(Game match)
        {
            // 1. Puntiamo al documento esatto nella collezione (se non c'è, lo crea)
            DocumentReference docRef = _db.Collection(_collectionName).Document(match.Id.ToString());

            // 2. Serializziamo l'oggetto puro del dominio
            string statoJson = JsonSerializer.Serialize(match);

            // 3. Creiamo l'involucro per Firestore
            var doc = new MatchDocument
            {
                Id = match.Id.ToString(),
                JsonState = statoJson
            };

            // 4. Spediamo a Google! (Uso .Wait() per sincronia, ma in app vere si usa async/Task)
            docRef.SetAsync(doc).Wait();
        }

        public Game GetById(Guid id)
        {
            DocumentReference docRef = _db.Collection(_collectionName).Document(id.ToString());
            DocumentSnapshot snapshot = docRef.GetSnapshotAsync().Result;

            if (snapshot.Exists)
            {
                MatchDocument doc = snapshot.ConvertTo<MatchDocument>();
                // Rigeneriamo l'oggetto Match dal JSON!
                return JsonSerializer.Deserialize<Game>(doc.JsonState);
            }

            return null; // Partita non trovata
        }

        public void Remove(Guid id)
        {
            DocumentReference docRef = _db.Collection(_collectionName).Document(id.ToString());
            docRef.DeleteAsync().Wait();
        }
    }
}