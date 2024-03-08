// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Common.Extensions;
using Econolite.Ode.Authorization.Extensions;
using Econolite.Ode.Domain.Entities.Extensions;
using Econolite.Ode.Domain.Rsu.Extensions;
using Econolite.Ode.Extensions.AspNet;
using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Extensions;
using Econolite.Ode.Models.Status.Db;
using Econolite.Ode.Monitoring.Events.Extensions;
using Econolite.Ode.Monitoring.HealthChecks.Kafka.Extensions;
using Econolite.Ode.Monitoring.HealthChecks.Mongo.Extensions;
using Econolite.Ode.Monitoring.Metrics.Extensions;
using Econolite.Ode.Persistence.Mongo;
using Econolite.Ode.Repository.AuditReport;
using Econolite.Ode.Repository.ConnectedVehicle;
using Econolite.Ode.Repository.Entities;
using Econolite.Ode.Repository.Ess;
using Econolite.Ode.Repository.PavementCondition;
using Econolite.Ode.Repository.Rsu.Extensions;
using Econolite.Ode.Repository.TimService;
using Econolite.Ode.Repository.Users;
using Econolite.Ode.Repository.WrongWayDriver;
using Econolite.Ode.Services.AuditReport;
using Econolite.Ode.Services.ConnectedVehicle;
using Econolite.Ode.Services.Ess;
using Econolite.Ode.Services.EventLogger;
using Econolite.Ode.Services.EventLogger.Extensions;
using Econolite.Ode.Services.PavementCondition;
using Econolite.Ode.Services.WeatherResponsive;
using Econolite.Ode.Services.WrongWayDriver;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Models;
using Monitoring.AspNet.Metrics;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
const string allOrigins = "_allOrigins";

builder.Services.AddMessaging();

builder.Services.AddTransient<IProducer<Guid, PavementConditionStatusMessageDocument>,
    Producer<Guid, PavementConditionStatusMessageDocument>>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allOrigins,
        policy =>
        {
            policy.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin();
        });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMvc(config =>
{
    config.Filters.Add(new AuthorizeFilter());
    config.AddCommaSeparatedGuidCollectionParsing();
});

builder.Services.AddAuthenticationJwtBearer(builder.Configuration, builder.Environment.IsDevelopment());

builder.Services.AddSwaggerGen(c =>
{
    #if DEBUG
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var fileName = typeof(Program).GetTypeInfo().Assembly.GetName().Name + ".xml";
        c.IncludeXmlComments(Path.Combine(basePath, fileName));
    #endif
    
    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
    });
                
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme,
                },
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Name = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header,
            },
            new List<string>()
        },
    });
});

builder.Services.AddMongo();

builder.Services.AddMetrics(builder.Configuration, "Reports")
    .ConfigureRequestMetrics(c =>
    {
        c.RequestCounter = "Requests";
        c.ResponseCounter = "Responses";
    })
    .AddUserEventSupport(builder.Configuration, _ =>
    {
        _.DefaultSource = "Reports";
        _.DefaultLogName = Econolite.Ode.Monitoring.Events.LogName.SystemEvent;
        _.DefaultCategory = Econolite.Ode.Monitoring.Events.Category.Server;
        _.DefaultTenantId = Guid.Empty;
    });

builder.Services.AddAuditReportRepository();
builder.Services.AddAuditReportService();

builder.Services.AddEntityRepo();
builder.Services.AddEntityTypeService();
builder.Services.AddEntityService();

builder.Services.AddEssStatusRepo();
builder.Services.AddEssConfigRepo();
builder.Services.AddEssStatusService();

builder.Services.AddPavementConditionStatusRepository();
builder.Services.AddPavementConditionConfigRepository();
builder.Services.AddPavementConditionStatusService();

builder.Services.AddConnectedVehicleConfigRepository();
builder.Services.AddConnectedVehicleLogRepository();
builder.Services.AddConnectedVehicleStatusService();

builder.Services.AddConnectedVehicleArchiveLogRepository();
builder.Services.AddConnectedVehicleArchiveService();

builder.Services.AddTimRsuStatusRepository();

builder.Services.AddUsersRepo();

builder.Services.AddWeatherResponsiveReportSupport();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration[EventLoggerConsts.RedisConnection];
});
builder.Services.AddEventLoggerService();

builder.Services.AddWrongWayDriverConfigRepository();
builder.Services.AddWrongWayDriverStatusRepository();
builder.Services.AddWrongWayDriverStatusService();

builder.Services.AddRsuStatusRepo();
builder.Services.AddRsuStatusService();

builder.Services.AddHealthChecks()
    .AddProcessAllocatedMemoryHealthCheck(maximumMegabytesAllocated: 1024, name: "Process Allocated Memory", tags: new[] { "memory" })
    .AddKafkaHealthCheck()
    .AddMongoDbHealthCheck();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(allOrigins);
app.UseRouting();
if (app.Environment.IsProduction())
{
   app.UseHttpsRedirection(); 
}
app.UseAuthentication();
app.UseAuthorization();
app.UseHealthChecksPrometheusExporter("/metrics");
app.AddRequestMetrics();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/healthz", new HealthCheckOptions()
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    endpoints.MapControllers();
});

app.LogStartup();

app.Run();
