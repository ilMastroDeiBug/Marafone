using Marafone.Application.Interfaces;
using Marafone.Application.UseCases.Commands;
using Marafone.Domain.GameLogic;
using Marafone.Infrastructure.Repositories;
using Marafone.Presentation.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Marafone.Presentation.Controllers
{
    /// <summary>
    /// Lobby: crea partite, lista partite in attesa, join.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LobbyController : ControllerBase
    {
        private readonly InMemoryMatchRepository _matchRepo;
        private readonly IUserRepository _userRepo;
        private readonly IHubContext<MatchHub> _hub;

        public LobbyController(
            IMatchRepository matchRepo,
            IUserRepository userRepo,
            IHubContext<MatchHub> hub)
        {
            // Cast sicuro — in dev usiamo InMemory
            _matchRepo = matchRepo as InMemoryMatchRepository
                ?? throw new InvalidOperationException("LobbyController richiede InMemoryMatchRepository.");
            _userRepo  = userRepo;
            _hub       = hub;
        }

        /// <summary>
        /// Lista tutti gli utenti disponibili per formare una partita.
        /// GET /api/lobby/users
        /// </summary>
        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            var users = _userRepo.GetAll();
            return Ok(users.Select(u => new { u.Id, u.Username }));
        }

        /// <summary>
        /// Crea una nuova partita e la avvia immediatamente.
        /// POST /api/lobby/create
        /// Body: { "user1Id":"...", "user2Id":"...", "user3Id":"...", "user4Id":"...", "targetPoints": 41 }
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateMatch(
            [FromBody] CreateMatchRequest request,
            [FromServices] StartMatchCommand command)
        {
            try
            {
                int target = request.TargetPoints is 21 or 31 or 41 ? request.TargetPoints : 41;
                Guid matchId = command.Execute(
                    request.User1Id, request.User2Id,
                    request.User3Id, request.User4Id,
                    target);

                // Avvisa tutti i giocatori tramite SignalR
                await _hub.Clients
                    .Users(request.User1Id.ToString(), request.User2Id.ToString(),
                           request.User3Id.ToString(), request.User4Id.ToString())
                    .SendAsync("MatchCreated", matchId);

                return Ok(new { MatchId = matchId, Message = "Partita creata!", TargetPoints = target });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Lista le partite aperte (briscola non ancora scelta = in attesa).
        /// GET /api/lobby/open
        /// </summary>
        [HttpGet("open")]
        public IActionResult GetOpenMatches()
        {
            var open = _matchRepo.GetOpenMatches().Select(g => new
            {
                g.Id,
                g.TargetPoints,
                Players = new[]
                {
                    g.Squadra1.Player1.Name.Value,
                    g.Squadra2.Player1.Name.Value,
                    g.Squadra1.Player2.Name.Value,
                    g.Squadra2.Player2.Name.Value
                }
            });
            return Ok(open);
        }

        /// <summary>
        /// Lista tutte le partite (per debug / admin).
        /// GET /api/lobby/all
        /// </summary>
        [HttpGet("all")]
        public IActionResult GetAllMatches()
        {
            var all = _matchRepo.GetAllIds().Select(id =>
            {
                var g = _matchRepo.GetById(id)!;
                return new { g.Id, g.TargetPoints, g.IsGameOver, Phase = g.BriscolaAttuale == null ? "BriscolaSelection" : (g.IsGameOver ? "GameOver" : "Playing") };
            });
            return Ok(all);
        }
    }

    public class CreateMatchRequest
    {
        public Guid User1Id     { get; set; }
        public Guid User2Id     { get; set; }
        public Guid User3Id     { get; set; }
        public Guid User4Id     { get; set; }
        public int  TargetPoints { get; set; } = 41;
    }
}
