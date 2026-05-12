using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Selfaware.Data;
using Selfaware.Features.Quizzes;
using Selfaware.Interfaces;
using Selfaware.Models.Entities;
using Selfaware.Services;
using System.Text;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Selfaware.Extensions
{
   


    public static class ServiceExtensions
    {
        public static IServiceCollection AddIdentityAndAuth(this IServiceCollection services, IConfiguration config)
        {

            services.AddIdentityApiEndpoints<ApplicationUser>()
        .AddRoles<IdentityRole>() 
        .AddEntityFrameworkStores<AppDbContext>();

            var jwtSettings = config.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };


                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {

                        context.HandleResponse();

                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsJsonAsync(new
                        {
                            success = false,
                            message = "Authentication failed: You are not authorized to do this.",
                            data = (object)null
                        });
                    },
                    OnForbidden = async context =>
                    {

                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsJsonAsync(new
                        {
                            success = false,
                            message = "Access denied: You do not have the required permissions (Admin).",
                            data = (object)null
                        });
                    }
                };

            });

            return services;
        }

       public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<EmailSettings>(config.GetSection("EmailSettings"));

            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IQuizService, QuizService>();
            return services;
        }
        public static IMvcBuilder ConfigureCustomValidation(this IMvcBuilder mvcBuilder)
        {
            return mvcBuilder.ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    var response = new CustomResponse<string>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    };
                    return new BadRequestObjectResult(response);
                };
            });
        }
    }
}
