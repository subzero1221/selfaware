using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Selfaware.Features.Auth;
using Selfaware.Features.Quizzes;
using Selfaware.Features.User.Entities;
using Selfaware.Shared.Models;
using Selfaware.Infrastructure.Data;
using Selfaware.Infrastructure.Messaging;
using System.Security.Claims;
using System.Text;
using Selfaware.Features.User;


namespace Selfaware.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddIdentityAndAuth(this IServiceCollection services, IConfiguration config)
        {

            services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>();

            var jwtSettings = config.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    RoleClaimType = ClaimTypes.Role,
                   
                };

                options.IncludeErrorDetails = true;
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {

                        context.HandleResponse();

                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        var response = CustomResponse<object>.ErrorResponse("Authentication failed: You are not authorized.");

                        await context.Response.WriteAsJsonAsync(response);
                    },
                    OnForbidden = async context =>
                    {

                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";

                        var response = CustomResponse<object>.ErrorResponse("Access denied: You do not have the required permissions.");

                        await context.Response.WriteAsJsonAsync(response);
                    }
                };

            });

            return services;
        }

       public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<EmailSettings>(config.GetSection("EmailSettings"));

            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<CreateQuizDtoValidator>();
            services.AddFluentValidationAutoValidation(config =>
            {
                config.DisableDataAnnotationsValidation = true;
            });
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IQuizService, QuizService>();
            services.AddScoped<IUserService, UserService>();
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
