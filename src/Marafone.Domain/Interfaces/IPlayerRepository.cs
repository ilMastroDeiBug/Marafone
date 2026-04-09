using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Marafone.Domain.Entities.UsersEntities;

namespace Marafone.Application.Interfaces
{
    public interface IPlayerRepository
    {
        Player GetById(Guid id);
        void Save(Player player);
    }
}