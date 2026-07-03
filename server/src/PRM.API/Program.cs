using Microsoft.EntityFrameworkCore;
using PRM.API.Middleware;
using PRM.Core.Interfaces;
using PRM.Infrastructure.Auth;
using PRM.Infrastructure.Data;
using PRM.Infrastructure.Data.Seed;
using PRM.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register infrastructure services (DbContext, repositories, auth, JWT)
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

// Register Background Scheduler
builder.Services.AddHostedService<PRM.Infrastructure.BackgroundJobs.SchedulerService>();

// Add controllers
builder.Services.AddControllers();

// Register CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Adding 3000 just in case
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Swagger (development only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PrmDbContext>();
    await context.Database.MigrateAsync();

    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await AdminSeed.SeedAsync(context, passwordHasher);
    await MockDataSeeder.SeedMockDataAsync(context, passwordHasher);
}

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("FrontendCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
