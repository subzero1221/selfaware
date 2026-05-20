using Microsoft.EntityFrameworkCore;
using Selfaware.Middleware;
using Selfaware.Extensions;
using Selfaware.Infrastructure.Data;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentityAndAuth(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddControllers().ConfigureCustomValidation();

var app = builder.Build();
app.UseRouting();
app.UseCors("FrontendPolicy");

app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
