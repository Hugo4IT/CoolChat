using System.Text;
using CoolChat.Domain.Interfaces;
using CoolChat.Server.ASPNET;
using CoolChat.Server.ASPNET.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

internal static class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders()
            .AddColorConsoleLogger(options =>
            {
                options.Mask.Add(LogLevel.Debug);
                options.Mask.Add(LogLevel.Trace);
            });

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "http://localhost:3000/",
                    ValidAudience = "http://localhost:3000/",
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["SecretSigningKey"]!))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        string? accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/signalr/chathub"))
                            context.Token = accessToken;
                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddIdentityCore<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Password.RequiredLength = 12;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
            })
            .AddEntityFrameworkStores<DataContext>();

        builder.Services.AddDbContext<DataContext>(options =>
            options.UseLazyLoadingProxies()
                .UseSqlite(builder.Configuration["ConnectionStrings:Default"]));

        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<IGroupService, GroupService>();
        builder.Services.AddScoped<IResourceService, ResourceService>();
        builder.Services.AddScoped<IChatService, ChatService>();
        builder.Services.AddScoped<IInviteService, InviteService>();
        builder.Services.AddSingleton<IWebPushService, WebPushService>();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("allowApi", policy =>
            {
                policy.WithOrigins("http://localhost:3000",
                        "http://10.10.1.28:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        builder.Services.AddSignalR();

        builder.Services.AddExceptionHandler((ExceptionHandlerOptions options, ILogger<ExecutionContext> logger) =>
            options.ExceptionHandler = async context =>
            {
                var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();

                StringBuilder builder = new();
                if (exceptionFeature != null)
                {
                    builder.Append($"An error occurred while trying to serve {exceptionFeature.Path}");
                    builder.AppendLine();

                    builder.Append(exceptionFeature.Error.Message);
                    builder.AppendLine();
                }

                if (context.Request.ContentLength > 0)
                {
                    var buffer = new byte[(int)context.Request.ContentLength!];
                    await context.Request.Body.ReadExactlyAsync(buffer, 0, (int)context.Request.ContentLength!);

                    var error = Encoding.UTF8.GetString(buffer);
                    builder.AppendLine();
                    builder.Append("Request body:");
                    builder.AppendLine();
                    builder.Append(error);
                    builder.AppendLine();
                }

                if (builder.Length == 0)
                    builder.Append("An unknown error occurred :/");

                logger.LogError(builder.ToString());
            }
        );

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // app.UseHttpLogging();
        app.UseExceptionHandler();
        app.UseCors("allowApi");
        // app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapHub<ChatHub>("/signalr/chathub");

        app.Run();
    }
}