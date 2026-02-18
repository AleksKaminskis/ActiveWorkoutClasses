using ActiveWorkoutClasses.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (builder.Environment.IsDevelopment())
{
    // For development, use In-Memory database
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("ActiveWorkoutClassesDb"));
}
else
{
    // For production, use SQL Server (Azure SQL Database)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// Configure CORS for Blazor app
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

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Seed database in development
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // TODO: Add seed data
    }
}

app.UseHttpsRedirection();

app.UseCors("AllowBlazorApp");

// app.UseAuthentication(); // Uncomment when Azure AD B2C is configured
app.UseAuthorization();

app.MapControllers();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.Run();

