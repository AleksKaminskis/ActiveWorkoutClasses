using ActiveWorkoutClasses.ApiService.Middleware;
using ActiveWorkoutClasses.Application.Interfaces;
using ActiveWorkoutClasses.Application.Services;
using ActiveWorkoutClasses.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();


// ========================================
// Add services to the container
// ========================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Automatically return 400 with validation errors
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(er => er.ErrorMessage))
                .ToList();

            return new BadRequestObjectResult(new
            {
                Message = "Validation failed",
                Errors = errors
            });
        };
    });

// Register Application Services (Dependency Injection)
builder.Services.AddScoped<IWorkoutClassService, WorkoutClassService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// ========================================
// Configure Database
// ========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (builder.Environment.IsDevelopment())
{
    // For development, use In-Memory database
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("ActiveWorkoutClassesDb"));
    Console.WriteLine("[!] Using In-Memory Database");
}
else
{
    if (string.IsNullOrEmpty(connectionString))
        throw new InvalidOperationException("SQL connection string is missing in Production.");

    // For production, use SQL Server (Azure SQL Database)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(60);
        }));
}

// Test the database connection in production

if (builder.Environment.IsProduction())
{
    Console.WriteLine("Running in PRODUCTION");
    Console.WriteLine($"Using SQL Server: {!string.IsNullOrEmpty(connectionString)}");
}


// ========================================
// Configure CORS (allow frontend to call API)
// ========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins(
            "https://localhost:7001", // Blazor app HTTPS
            "http://localhost:5001"   // Blazor app HTTP
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();


// ========================================
// Global Exception Handling (catches all errors)
// ========================================
app.UseExceptionHandling();
app.UseExceptionHandler();


// ========================================
// Seed Database in Development
// ========================================
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            // Apply any pending migrations
            Console.WriteLine("[i] Ensuring database is created...");
            await context.Database.EnsureCreatedAsync();

            // Seed test data
            Console.WriteLine("[i] Seeding database...");
            await DbSeeder.SeedAsync(context);
            Console.WriteLine("[+] Database ready!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[X] Error seeding database: {ex.Message}");
            // Don't crash the app, just log the error
        }
    }
}


// ========================================
// Configure the HTTP request pipeline
// ========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Active Workout Classes API v1");
        c.RoutePrefix = string.Empty; // Swagger UI at root URL
        c.DocumentTitle = "Active Workout Classes API";
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowBlazorApp");

// app.UseAuthentication(); // Uncomment when Azure AD B2C is configured
app.UseAuthorization();


app.MapControllers();
app.MapDefaultEndpoints();

app.Run();

