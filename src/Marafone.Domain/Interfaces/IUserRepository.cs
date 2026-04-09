using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using Marafone.Domain.Entities.UsersEntities;

namespace Marafone.Application.Interfaces
{
    public interface IUserRepository
    {
        User GetById(Guid id);

        // Cerca un utente per nome (utile per aggiungere amici o inviti)
        User GetByUsername(string username);

        // Salva un nuovo utente (Registrazione) o aggiorna le sue statistiche
        void Save(User user);

        void Remove(Guid id);
    }
}