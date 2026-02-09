using BuildingBlocks.Behaviors;
using BuildingBlocks.Messaging.MassTransit;
using Email.API.Configuration;
using Email.API.Services;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using ExceptionHandlerMiddleware = BuildingBlocks.Middlewares.ExceptionHandlerMiddleware;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add services to the container.

// Configuration SMTP depuis appsettings.json
builder.Services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));

// Service d'envoi d'email (Singleton pour réutilisation)
builder.Services.AddSingleton<IEmailService, SmtpEmailService>();

// MediatR pour CQRS (Commands/Queries)
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

// FluentValidation pour validation des commandes
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// MassTransit avec RabbitMQ - Écoute les événements OrderCreatedEvent
builder.Services.AddMessageBroker(configuration, typeof(Program).Assembly);

// Controllers pour l'API REST
builder.Services.AddControllers();

// Health Checks (vérifier la connexion RabbitMQ)
builder.Services.AddHealthChecks();

// OpenAPI / Swagger
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Middleware global de gestion des exceptions
app.UseMiddleware<ExceptionHandlerMiddleware>();

// Health check endpoint (pour Docker, Kubernetes, etc.)
app.UseHealthChecks("/health", new HealthCheckOptions()
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();