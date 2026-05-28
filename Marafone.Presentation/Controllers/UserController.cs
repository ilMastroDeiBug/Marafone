using Marafone.Application.Interfaces;
using Marafone.Domain.Entities.UsersEntities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Marafone.Presentation.Controllers
{
    /// <summary>
    /// Gestione utenti: registrazione, login semplice (username-based) e lista.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Registra un nuovo utente o recupera quello esistente (login semplice).
        /// POST /api/user/register
        /// Body: { "username": "Mario", "email": "mario@example.com" }
        /// </summary>
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                return BadRequest(new { Error = "Username obbligatorio." });

            // Controlla se esiste già
            var existing = _userRepository.GetByUsername(request.Username);
            if (existing != null)
            {
                // Login: restituisce l'utente esistente
                return Ok(new UserResponse
                {
                    Id       = existing.Id,
                    Username = existing.Username,
                    Email    = existing.Email,
                    Message  = "Bentornato!"
                });
            }

            // Nuovo utente
            var user = new User(request.Username, request.Email ?? $"{request.Username}@marafone.local");
            _userRepository.Save(user);

            return Ok(new UserResponse
            {
                Id       = user.Id,
                Username = user.Username,
                Email    = user.Email,
                Message  = "Registrazione completata!"
            });
        }

        /// <summary>
        /// Login per username (senza password per ora — modalità demo).
        /// POST /api/user/login
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                return BadRequest(new { Error = "Username obbligatorio." });

            var user = _userRepository.GetByUsername(request.Username);
            if (user == null)
                return NotFound(new { Error = $"Utente '{request.Username}' non trovato. Registrati prima." });

            return Ok(new UserResponse
            {
                Id       = user.Id,
                Username = user.Username,
                Email    = user.Email,
                Message  = "Login effettuato!"
            });
        }

        /// <summary>
        /// Lista tutti gli utenti registrati (utile per la lobby e i test).
        /// GET /api/user
        /// </summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userRepository.GetAll();
            return Ok(users.Select(u => new UserResponse
            {
                Id       = u.Id,
                Username = u.Username,
                Email    = u.Email
            }));
        }
    }

    // ── Request / Response DTO ─────────────────────────────────────────────
    public class RegisterRequest
    {
        public string Username { get; set; } = "";
        public string? Email   { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = "";
    }

    public class UserResponse
    {
        public Guid   Id       { get; set; }
        public string Username { get; set; } = "";
        public string Email    { get; set; } = "";
        public string? Message { get; set; }
    }
}
