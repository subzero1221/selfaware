using Microsoft.EntityFrameworkCore;
using Selfaware.Data;
using Selfaware.Middleware;
using Selfaware.Extensions;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentityAndAuth(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddControllers().ConfigureCustomValidation();

var app = builder.Build();


app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
