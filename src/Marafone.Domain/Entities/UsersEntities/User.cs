using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace Marafone.Domain.Entities.UsersEntities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }

        // Eventuali statistiche globali (fuori dalla singola partita)
        public int PartiteVinte { get; private set; }
        public int PartiteGiocate { get; private set; }

        public User(string username, string email)
        {
            Id = Guid.NewGuid();
            Username = username;
            Email = email;
            CreatedAtUtc = DateTime.UtcNow;

            PartiteVinte = 0;
            PartiteGiocate = 0;
        }

        // Metodi di business per l'utente
        public void AggiungiVittoria()
        {
            PartiteVinte++;
            PartiteGiocate++;
        }

        public void AggiungiSconfitta()
        {
            PartiteGiocate++;
        }
    }
}