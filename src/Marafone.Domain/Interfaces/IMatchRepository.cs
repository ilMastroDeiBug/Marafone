using Marafone.Domain.GameLogic;
using System;

namespace Marafone.Application.Interfaces
{
    public interface IMatchRepository
    {
        Game? GetById(Guid id);
        void Save(Game match);
        void Remove(Guid id);
    }
}