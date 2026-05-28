using Marafone.Application.Interfaces;
using Marafone.Application.UseCases.Commands;
using Marafone.Application.UseCases.Queries;
using Marafone.Infrastructure.Repositories;
using Marafone.Presentation.Hubs;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// SERVICES
// =========================================================================
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

// Serve file statici dalla wwwroot (la UI web)
builder.Services.AddDirectoryBrowser();

// CORS — permette al frontend (es. file:// o localhost:porta diversa) di chiamare l'API
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowAll", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://localhost:5000",
                "http://localhost:5173",
                "http://127.0.0.1:5500",  // Live Server di VS Code
                "null"                     // per file:// su browser
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();           // necessario per SignalR
    });
});

// =========================================================================
// DEPENDENCY INJECTION
// =========================================================================

// REPOSITORY — in dev usiamo InMemory. Per produzione usa FirestoreMatchRepository.
var useFirestore  = builder.Configuration.GetValue<bool>("Firebase:Enabled");
var firestoreId   = builder.Configuration["Firebase:ProjectId"] ?? "";

if (useFirestore && !string.IsNullOrWhiteSpace(firestoreId))
{
    builder.Services.AddSingleton<IMatchRepository>(new FirestoreMatchRepository(firestoreId));
}
else
{
    builder.Services.AddSingleton<IMatchRepository, InMemoryMatchRepository>();
}

// Sempre InMemory per gli utenti (in attesa di FirestoreUserRepository)
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

// COMMANDS
builder.Services.AddTransient<StartMatchCommand>();
builder.Services.AddTransient<SetBriscolaCommand>();
builder.Services.AddTransient<PlayCardCommand>();
builder.Services.AddTransient<StartNextHandCommand>();
builder.Services.AddTransient<ForfeitMatchCommand>();

// QUERIES
builder.Services.AddTransient<GetMatchByIdQuery>();
builder.Services.AddTransient<GetMatchForPlayerQuery>();

// =========================================================================
var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Serve la UI web dalla cartella wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapHub<MatchHub>("/matchHub");
app.MapControllers();

app.Run();