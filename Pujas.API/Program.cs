using DotNetEnv;
using log4net;
using log4net.Config;
using MassTransit;
using System.Reflection;

using Pujas.Application.Handlers;

using Pujas.Domain.Events;
using Pujas.Domain.Repositories;

using Pujas.Infrastructure.Configurations;
using Pujas.Infrastructure.Consumer;
using Pujas.Infrastructure.Interfaces;
using Pujas.Infrastructure.Persistence.Repository.MongoRead;
using Pujas.Infrastructure.Persistence.Repository.MongoWrite;
using Pujas.Infrastructure.Queries.QueryHandlers;
using Pujas.Application.Events;
using FluentValidation.AspNetCore;
using FluentValidation;
using Pujas.Application.Validations;
using RestSharp;
using System.Net.WebSockets;
using MediatR;
using Pujas.Application.DTOs;
using Pujas.Infrastructure.Queries;
using Pujas.Infrastructure.Commands.CommandHandlers;
using Pujas.Infrastructure.Services;
using Hangfire;
using Pujas.Infrastructure.interfaces;
using Hangfire.MemoryStorage;
using Pujas.API.Controllers;

var loggerRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(loggerRepository, new FileInfo("log4net.config"));

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IRestClient>(new RestClient());
builder.Services.AddSingleton<ICronJobService, CronJobService>();
builder.Services.AddSingleton<IBidHub, BidHub>();


// Registrar configuración de MongoDB
builder.Services.AddSingleton<MongoWriteDbConfig>();
builder.Services.AddSingleton<MongoReadDbConfig>();

// Registrar configuración de Log4Net
builder.Services.AddSingleton(LogManager.GetLogger(typeof(Program)));

// REGISTRA EL REPOSITORIO ANTES DE MediatR
builder.Services.AddScoped<IBidRepository, MongoWriteBidRepository>();
builder.Services.AddScoped<IReadBidRepository, MongoReadBidRepository>();
builder.Services.AddScoped<IReadAutomaticBidRepository, MongoAutomaticReadBidRepository>();

// REGISTRA MediatR PARA TODOS LOS HANDLERS
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateBidCommandHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(BidMadeCommandHandler).Assembly));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetBidByIdQueryHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAuctionBidsQueryHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetUserAutomaticBidQueryHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAuctionLastBidQueryHandler).Assembly));

builder.Services.AddValidatorsFromAssemblyContaining<CreateAutomaticBidDtoValidation>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddMassTransit(busConfigurator =>
{

    busConfigurator.SetKebabCaseEndpointNameFormatter();
    busConfigurator.AddConsumers(typeof(Program).Assembly);
    busConfigurator.AddConsumer<CreateBidConsumer>();
    busConfigurator.AddConsumer<CreateAutomaticBidConsumer>();

    busConfigurator.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(new Uri(Environment.GetEnvironmentVariable("RABBIT_URL")), h =>
        {
            h.Username(Environment.GetEnvironmentVariable("RABBIT_USERNAME"));
            h.Password(Environment.GetEnvironmentVariable("RABBIT_PASSWORD"));
        });

        configurator.ReceiveEndpoint(Environment.GetEnvironmentVariable("RABBIT_QUEUE"), e =>
        {
            e.ConfigureConsumer<CreateBidConsumer>(context);
        });
        configurator.ReceiveEndpoint(Environment.GetEnvironmentVariable("RABBIT_QUEUE_AUTOMATIC"), e =>
        {
            e.ConfigureConsumer<CreateAutomaticBidConsumer>(context);
        });

        configurator.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        configurator.ConfigureEndpoints(context);
    });
});
EndpointConvention.Map<BidCreatedEvent>(new Uri("queue:" + Environment.GetEnvironmentVariable("RABBIT_QUEUE")));
EndpointConvention.Map<AutomaticBidCreatedEvent>(new Uri("queue:" + Environment.GetEnvironmentVariable("RABBIT_QUEUE_AUTOMATIC")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddHangfire(config =>
{
    config.UseMemoryStorage();
});

builder.Services.AddHangfireServer();

var app = builder.Build();
app.UseRouting();


app.UseSwagger();
app.UseSwaggerUI();

app.UseWebSockets();
app.Map("/bidWs", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var webSocketHub = context.RequestServices.GetRequiredService<IBidHub>() as BidHub;
        await webSocketHub.HandleWebSocketConnection(webSocket);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
