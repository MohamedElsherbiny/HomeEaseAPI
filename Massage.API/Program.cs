using Massage.Application;
using Massage.Application.Interfaces;
using Massage.Application.Interfaces.Services;
using Massage.Domain.Entities;
using Massage.Domain.Repositories;
using Massage.Infrastructure.Data;
using Massage.Infrastructure.Repos;
using Massage.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Massage.Infrastructure.Data.Seeding;
using Massage.Application.Middlewares;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using Serilog;
using Serilog.Formatting.Json;
using OpenTelemetry.Trace;
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using Massage.Infrastructure.FileStorage;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") 
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); 
        });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Massage API", Version = "v1" });

    // ? JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid JWT token.\n\nExample: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddApplication(); 
builder.Services.AddInfrastructure(builder.Configuration); 
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));



builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProviderService, ProviderService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAppDbContext, AppDbContext>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGeolocationService, GeolocationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IProviderRepository, ProviderRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddApplicationInsightsTelemetry();


// Configure Blob Storage
var blobStorageConfig = builder.Configuration.GetSection("BlobStorage");
var blobConnectionString = blobStorageConfig["ConnectionString"];
var containerName = blobStorageConfig["ContainerName"];

// Configure file storage
builder.Services.AddScoped<IFileStorageClient>(sp =>
    new BlobContainerServiceClient(
        sp.GetRequiredService<BlobServiceClient>(),
        containerName
    )
);


builder.Services.AddIdentityCore<User>(options =>
{
    // Password options here if needed
})
.AddRoles<IdentityRole<Guid>>()
.AddEntityFrameworkStores<AppDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

builder.Services.AddIdentity<User, IdentityRole<Guid>>() 
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();



builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddDataProtection();

// Configure Serilog with explicit async logging
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
        .Enrich.FromLogContext();

    // Check environment to configure the appropriate async sink
    if (context.HostingEnvironment.IsDevelopment())
    {
        // Development: Async console logging with JSON
        configuration.WriteTo.Async(a => a.Console(
                formatter: new JsonFormatter(renderMessage: true)),
            bufferSize: 1000);
    }
    else
    {
        // Production: Async Application Insights logging (no formatter needed)
        configuration.WriteTo.Async(a => a.ApplicationInsights(
                connectionString: context.Configuration["ApplicationInsights:ConnectionString"],
                telemetryConverter: new TraceTelemetryConverter()),
            bufferSize: 1000);
    }
});
// Configure OpenTelemetry for tracing and metrics
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("Pensell_Api"))
    .WithTracing(tracing => tracing
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
        .AddAspNetCoreInstrumentation()
        .AddAzureMonitorTraceExporter(o => o.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"])
        .AddSource("MyApp.Tracing"))
    .WithMetrics(metrics => metrics
        //.AddRuntimeInstrumentation()
        .AddHttpClientInstrumentation()
        .AddAzureMonitorMetricExporter(o => o.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"])
        .AddMeter("MyApp.Metrics"));
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});



builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
    options.AddPolicy("ProviderOnly", policy =>
        policy.RequireRole("Provider"));
    options.AddPolicy("UserOnly", policy =>
        policy.RequireRole("User"));
});



var app = builder.Build();

// Use CORS
app.UseCors(MyAllowSpecificOrigins);

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed admin data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await AdminDataSeeder.SeedAdminUserAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.UseMiddleware<ExceptionMiddleware>();

app.Run();
