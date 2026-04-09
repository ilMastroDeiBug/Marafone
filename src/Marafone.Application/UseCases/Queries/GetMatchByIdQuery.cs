using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Marafone.Application.DTOs;
using Marafone.Application.Interfaces;
using Marafone.Application.Mappers;

namespace Marafone.Application.UseCases.Queries
{
    public class GetMatchByIdQuery
    {
        private readonly IMatchRepository _matchRepository;

        public GetMatchByIdQuery(IMatchRepository repository)
        {
            _matchRepository = repository;
        }

        public MatchDTO Execute(Guid id)
        {
            var match = _matchRepository.GetById(id);
            if (match != null)
            {
                return MatchMapper.ToDTO(match);
            }
            return null;
        }
    }
}