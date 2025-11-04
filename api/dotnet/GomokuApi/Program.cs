using GomokuApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ??
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

var app = builder.Build();

// Middleware pipeline configuration
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers();

app.Run();

