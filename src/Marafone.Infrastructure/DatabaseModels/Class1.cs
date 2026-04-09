using Google.Cloud.Firestore;
using System.Collections.Generic;

namespace Marafone.Infrastructure.DatabaseModels
{
    // L'attributo FirestoreData dice a Google che questa classe va mappata nel database
    [FirestoreData]
    public class MatchDocument
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public string JsonState { get; set; }
        // TRUCCO DA MAESTRO: Visto che il Match di Marafone è molto complesso 
        // (ha array di sedie, polimorfismi, carte), per un MVP rapido e scalabile con NoSQL, 
        // serializziamo l'intero stato puro del dominio in una stringa JSON compatta. 
        // Firestore adora questo approccio per gli Aggregati!
    }
}