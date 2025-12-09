using GomokuApi.Data;
using GomokuApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Database
builder.Services.AddDbContext<GomokuDbContext>(options => {
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    options.UseSqlServer(connectionString);
});

// Add Authentication with Microsoft Entra External ID
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options => {
        builder.Configuration.Bind("AzureAd", options);
        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "roles";

        options.Events = new JwtBearerEvents {
            OnAuthenticationFailed = context => {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context => {
                Console.WriteLine($"Token validated for: {context.Principal?.Identity?.Name}");
                return Task.CompletedTask;
            }
        };
    }, options => {
        builder.Configuration.Bind("AzureAd", options);
    });

builder.Services.AddAuthorization();

// Add CORS
string[] corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ??
                 ["http://localhost:5173"]; // Default to localhost:5173 (react app URL) if not set
builder.Services.AddCors(options => {
    options.AddPolicy("AllowReactApp",
        policyBuilder => {
            policyBuilder.WithOrigins(corsOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader();
        });
});

// Register services
builder.Services.AddScoped<IPositionCacheService, PositionCacheService>();
builder.Services.AddScoped<IGomokuService, GomokuService>();
builder.Services.AddScoped<IUserService, UserService>();

WebApplication app = builder.Build();

// Middleware pipeline configuration
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();

    // Apply database migrations on startup
    using IServiceScope scope = app.Services.CreateScope();
    GomokuDbContext context = scope.ServiceProvider.GetRequiredService<GomokuDbContext>();
    if (context.Database.GetPendingMigrations().Any()) {
        Console.WriteLine("Applying database migrations...");
        await context.Database.MigrateAsync();
        Console.WriteLine("Database migrations applied successfully.");
    }
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();



app.Run();

