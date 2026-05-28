using Marafone.Application.Interfaces;
using Marafone.Domain.Entities.UsersEntities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Marafone.Infrastructure.Repositories
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();

        public InMemoryUserRepository()
        {
            // 4 utenti di seed per test rapido
            _users.Add(new User("Pippo",     "pippo@marafone.local"));
            _users.Add(new User("Pluto",     "pluto@marafone.local"));
            _users.Add(new User("Paperino",  "paperino@marafone.local"));
            _users.Add(new User("Topolino",  "topolino@marafone.local"));
        }

        public User? GetById(Guid id)          => _users.FirstOrDefault(u => u.Id == id);
        public User? GetByUsername(string name)=> _users.FirstOrDefault(u =>
            string.Equals(u.Username, name, StringComparison.OrdinalIgnoreCase));

        public void Save(User user)
        {
            // Aggiorna se esiste, altrimenti aggiungi
            var idx = _users.FindIndex(u => u.Id == user.Id);
            if (idx >= 0) _users[idx] = user;
            else          _users.Add(user);
        }

        public void Remove(Guid id) => _users.RemoveAll(u => u.Id == id);

        public IReadOnlyList<User> GetAll() => _users.AsReadOnly();
    }
}