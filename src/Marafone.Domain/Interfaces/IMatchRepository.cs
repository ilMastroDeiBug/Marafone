using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marafone.Domain.GameLogic;

namespace Marafone.Application.Interfaces
{
    public interface IMatchRepository
    {
        Game GetById(Guid id);
        void Save(Game match); // Se non esiste la crea, se esiste la aggiorna
        void Remove(Guid id);
    }
}