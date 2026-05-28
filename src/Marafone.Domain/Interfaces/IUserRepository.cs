using Marafone.Domain.Entities.UsersEntities;
using System;
using System.Collections.Generic;

namespace Marafone.Application.Interfaces
{
    public interface IUserRepository
    {
        User? GetById(Guid id);
        User? GetByUsername(string username);
        void Save(User user);
        void Remove(Guid id);
        IReadOnlyList<User> GetAll();
    }
}