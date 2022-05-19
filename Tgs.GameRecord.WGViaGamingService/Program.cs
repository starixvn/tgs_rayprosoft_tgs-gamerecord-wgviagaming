using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.GameRecord.WGViaGamingService;
using Tgs.MessageQueue.MongoDB.Configuration;
using MongoMessageQueue = MongoDB.Messaging.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("/config/appsettings.json", true)
    .AddEnvironmentVariables();

builder.Logging
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.ParseStateValues = true;
        _ = options.AddFluentdForwardExporter(fluentOptions =>
        {
            var nodeIp = builder.Configuration.GetValue<string>("NODE_IP");
            fluentOptions.Host = nodeIp;
            fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME");
            fluentOptions.UseMessagePack();
        });
    });

// Add services to the container.

builder.Services
    .AddRouting(options => options.LowercaseUrls = true)
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//
builder.Services.AddMemoryCache();

builder.Services.AddGameRecordCore((options, sp) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    configuration.GetSection("GameRecordInfo").Bind(options);
},
(options, sp) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    options.BaseUrl = configuration.GetValue<Uri>("GameRecord:GameProvidersQueryServerBaseUrl");
},
(options, sp) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    options.BaseUrl = configuration.GetValue<Uri>("GameRecord:GameRecordSchedulesBaseUrl");
},
(options, sp) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    options.BaseUrl = configuration.GetValue<Uri>("GameRecord:GameProvidersBaseUrl");
});

builder.Services
    .AddMessageQueue()
    .AddRabbitMessageQueue(config =>
    {
        _ = config
            .ConfigQueueOptions((options, sp) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();

                configuration.GetSection("RabbitMQ").Bind(options);

                options.OnSetupQueueAndExchange = channel =>
                {
                    _ = channel.QueueDeclare(
                        queue: "gp.gamerecord.wgvia",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    channel.QueueBind(
                        "gp.gamerecord.wgvia",
                        "amq.direct",
                        $"tgs.gp.gamerecord.wgvia",
                        new Dictionary<string, object>());
                };
            })
            .AddHandler<WGViaGamingMessageHandler>("gp.gamerecord.wgvia", false);
    })
    .AddMongoMessageQueue(config =>
    {
        _ = config
        .DefineMessageQueue(
            "gp.gamerecord.database",
            sp => new MongoMessageQueueBuilderOptions
            {
                MessageQueue = MongoMessageQueue.Default,
                ConnectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("MongoMessaging")
            });
    })
    .ClearAllExchanges()
    .AddMongoGlobPatternExchange("tgs.gp.gamerecord.database.round.wgvia", "gp.gamerecord.database", "tgs.gp.gamerecord.database.round.wgvia");

builder.Services
     .AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
        sp => new ConfigureNamedOptions<OtlpExporterOptions>(
            Microsoft.Extensions.Options.Options.DefaultName,
            exporterOptions =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var nodeIp = config.GetValue<string>("NODE_IP");

                if (!string.IsNullOrWhiteSpace(nodeIp))
                    exporterOptions.Endpoint = new Uri($"http://{nodeIp}:4317");

                config.GetSection("Opentelemetry:Tracing").Bind(exporterOptions);
            }))
    .AddOpenTelemetryTracing(traceBuilder => traceBuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddEnvironmentVariableDetector())
        .AddSource("Tgs.*")
        .AddSource("TGS.*")
        .AddGrpcClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddProfileViewExporter()
        .AddOtlpExporter())
    .AddProfileViewer()
    .Configure<AspNetCoreInstrumentationOptions>(options =>
    {
        options.Filter = httpContext =>
        {
            return !httpContext.Request.Path.HasValue
                || !httpContext.Request.Path.StartsWithSegments("/swagger")
                && !httpContext.Request.Path.StartsWithSegments("/healthz")
                && !httpContext.Request.Path.StartsWithSegments("/profiler/")
                && !new[] { "html", "ico", "png", "jpg" }.Any(
                    ext => httpContext.Request.Path.Value.EndsWith($".{ext}"));
        };
    });

builder.Services
    .AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

_ = app.UseSwagger();
_ = app.UseSwaggerUI();

_ = app.UseAuthorization();

_ = app.MapControllers();

app.MapProfileViewer();

app.MapHealthChecks("/healthz");

app.Run();
