using Microsoft.EntityFrameworkCore;
using Selfaware.Extensions;
using Selfaware.Features.Game.GameSession;
using Selfaware.Infrastructure.Data;
using Selfaware.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
       /* .EnableSensitiveDataLogging()
        .LogTo(Console.WriteLine, LogLevel.Information)*/
);

builder.Services.AddIdentityAndAuth(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddControllers().ConfigureCustomValidation();

var app = builder.Build();
app.UseRouting();
app.UseWebSockets();
app.UseCors("FrontendPolicy");

app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<GameSessionHub>("/game");

app.Run();
