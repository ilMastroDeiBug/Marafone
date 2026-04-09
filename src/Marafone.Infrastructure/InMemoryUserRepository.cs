using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Marafone.Application.Interfaces;
using Marafone.Domain.Entities.UsersEntities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Marafone.Infrastructure.Repositories
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new List<User>();

        public InMemoryUserRepository()
        {
            // Creiamo 4 utenti finti "Hardcoded" per poter testare subito la creazione della partita
            var u1 = new User("Pippo", "pippo@email.com");
            var u2 = new User("Pluto", "pluto@email.com");
            var u3 = new User("Paperino", "paperino@email.com");
            var u4 = new User("Topolino", "topolino@email.com");

            // Forziamo i loro ID a dei Guid conosciuti se vuoi testarli da Postman, 
            // altrimenti lascia che se li generino da soli.
            _users.Add(u1); _users.Add(u2); _users.Add(u3); _users.Add(u4);
        }

        public User GetById(Guid id) => _users.FirstOrDefault(u => u.Id == id);
        public User GetByUsername(string username) => _users.FirstOrDefault(u => u.Username == username);
        public void Save(User user) => _users.Add(user); // Finta
        public void Remove(Guid id) => _users.RemoveAll(u => u.Id == id); // Finta

        // Un metodo extra giusto per farti stampare gli ID nella console e copiarli su Postman
        public List<User> GetAllForTest() => _users;
    }
}