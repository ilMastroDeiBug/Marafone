using Marafone.Application.Interfaces;
using Marafone.Application.UseCases.Commands;
using Marafone.Application.UseCases.Queries;
using Marafone.Infrastructure.Repositories;
using Marafone.Presentation.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

// =========================================================================
// 🏗️ INJECTION CONTAINER (Clean Architecture Setup)
// =========================================================================

// 1. REPOSITORIES (Infrastructure Layer) -> SINGLETON
// Sostituisci la stringa con il vero Project ID che trovi nella console di Firebase
string firebaseProjectId = "il-tuo-project-id-firebase";
builder.Services.AddSingleton<IMatchRepository>(new FirestoreMatchRepository(firebaseProjectId));

// Visto che non abbiamo ancora scritto la repo di Firestore per gli Utenti,
// iniettiamo quella finta in memoria per ora, così il progetto compila!
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

// 2. USE CASES: COMMANDS (Application Layer) -> TRANSIENT
builder.Services.AddTransient<StartMatchCommand>();
builder.Services.AddTransient<SetBriscolaCommand>();
builder.Services.AddTransient<PlayCardCommand>();
builder.Services.AddTransient<StartNextHandCommand>();
builder.Services.AddTransient<ForfeitMatchCommand>();

// 3. USE CASES: QUERIES (Application Layer) -> TRANSIENT
builder.Services.AddTransient<GetMatchByIdQuery>();
builder.Services.AddTransient<GetMatchForPlayerQuery>();

// =========================================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHub<MatchHub>("/matchHub");

app.MapControllers();

app.Run();