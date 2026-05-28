using Marafone.Application.DTOs;
using Marafone.Application.UseCases.Commands;
using Marafone.Application.UseCases.Queries;
using Marafone.Presentation.Hubs; // Assicurati di avere questo
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Marafone.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchController : ControllerBase
    {
        private readonly IHubContext<MatchHub> _hubContext;

        // Iniettiamo l'Hub nel costruttore del Controller
        public MatchController(IHubContext<MatchHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // 1. CREA PARTITA
        // POST: api/match/start
        [HttpPost("start")]
        public IActionResult StartMatch(
            [FromBody] StartMatchRequest request,
            [FromServices] StartMatchCommand command)
        {
            try
            {
                int target = request.TargetPoints is 21 or 31 or 41 ? request.TargetPoints : 41;
                Guid matchId = command.Execute(
                    request.User1Id, request.User2Id, request.User3Id, request.User4Id, target);

                return Ok(new { MatchId = matchId, Message = "Partita creata con successo!", TargetPoints = target });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // 2. LEGGI STATO PARTITA (Il Radar censurato)
        // GET: api/match/{matchId}/player/{playerId}
        [HttpGet("{matchId}/player/{playerId}")]
        public ActionResult<MatchDTO> GetMatch(
            Guid matchId,
            Guid playerId,
            [FromServices] GetMatchForPlayerQuery query)
        {
            var matchDto = query.Execute(matchId, playerId);

            if (matchDto == null)
                return NotFound(new { Error = "Partita non trovata" });

            return Ok(matchDto);
        }

        // 3. IMPOSTA LA BRISCOLA
        // POST: api/match/{matchId}/briscola
        [HttpPost("{matchId}/briscola")]
        public async Task<IActionResult> SetBriscola(
            Guid matchId,
            [FromBody] SetBriscolaRequest request,
            [FromServices] SetBriscolaCommand command)
        {
            try
            {
                command.Execute(matchId, request.PlayerId, request.Suit);

                // PING SIGNALR: Avvisa tutti i giocatori al tavolo di ricaricare lo stato
                await _hubContext.Clients.Group(matchId.ToString()).SendAsync("MatchUpdated");

                return Ok(new { Message = $"Briscola impostata a {request.Suit}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // 4. GIOCA UNA CARTA
        // POST: api/match/{matchId}/play
        [HttpPost("{matchId}/play")]
        public async Task<IActionResult> PlayCard(
            Guid matchId,
            [FromBody] PlayCardRequest request,
            [FromServices] PlayCardCommand command)
        {
            try
            {
                command.Execute(matchId, request.PlayerId, request.Rank, request.Suit);

                // PING SIGNALR
                await _hubContext.Clients.Group(matchId.ToString()).SendAsync("MatchUpdated");

                return Ok(new { Message = "Carta giocata con successo!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // 5. PROSSIMA MANO
        // POST: api/match/{matchId}/next-hand
        [HttpPost("{matchId}/next-hand")]
        public async Task<IActionResult> StartNextHand(
            Guid matchId,
            [FromBody] PlayerActionRequest request,
            [FromServices] StartNextHandCommand command)
        {
            try
            {
                command.Execute(matchId, request.PlayerId);

                // PING SIGNALR
                await _hubContext.Clients.Group(matchId.ToString()).SendAsync("MatchUpdated");

                return Ok(new { Message = "Nuova smazzata iniziata!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // 6. ABBANDONA PARTITA
        // POST: api/match/{matchId}/forfeit
        [HttpPost("{matchId}/forfeit")]
        public async Task<IActionResult> Forfeit(
            Guid matchId,
            [FromBody] PlayerActionRequest request,
            [FromServices] ForfeitMatchCommand command)
        {
            try
            {
                command.Execute(matchId, request.PlayerId);

                // PING SIGNALR
                await _hubContext.Clients.Group(matchId.ToString()).SendAsync("MatchUpdated");

                return Ok(new { Message = "Partita abbandonata. Vittoria assegnata agli avversari." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }

    // ====================================================================
    // REQUEST DTOs
    // ====================================================================

    public class StartMatchRequest
    {
        public Guid User1Id      { get; set; }
        public Guid User2Id      { get; set; }
        public Guid User3Id      { get; set; }
        public Guid User4Id      { get; set; }
        public int  TargetPoints { get; set; } = 41;
    }

    public class SetBriscolaRequest
    {
        public Guid PlayerId { get; set; }
        public string Suit { get; set; }
    }

    public class PlayCardRequest
    {
        public Guid PlayerId { get; set; }
        public string Rank { get; set; }
        public string Suit { get; set; }
    }

    public class PlayerActionRequest
    {
        public Guid PlayerId { get; set; }
    }
}