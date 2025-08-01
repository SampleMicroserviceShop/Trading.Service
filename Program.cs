

using Common.Library.Identity;
using Common.Library.MassTransit;
using Common.Library.MongoDB;
using Common.Library.Settings;
using GreenPipes;
using Identity.Contracts;
using Inventory.Contracts;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using System.Reflection;
using Trading.Service.Entities;
using Trading.Service.Exceptions;
using Trading.Service.Settings;
using Trading.Service.SignalR;
using Trading.Service.StateMachines;

var builder = WebApplication.CreateBuilder(args);
const string AllowedOriginSetting = "AllowedOrigin";

// Add services to the container.
var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
builder.Services.AddMongo()
.AddMongoRepository<CatalogItem>("catalogitems")
.AddMongoRepository<InventoryItem>("inventoryitems")
.AddMongoRepository<ApplicationUser>("users")
.AddJwtBearerAuthentication();

AddMassTransit(builder.Services);


builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>()
.AddSingleton<MessageHub>()
.AddSignalR();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(_builder =>
    {
        _builder.WithOrigins(builder.Configuration[AllowedOriginSetting])
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
        //Credentials must be allowed in order for cookie-based sticky sessions to work correctly.
    });

}
// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHub<MessageHub>("/messagehub");

app.Run();
void AddMassTransit(IServiceCollection services)
{
    services.AddMassTransit(configure =>
    {
        configure.UsingCustomRabbitMQ(retryConfigurator =>
        {
            retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
            retryConfigurator.Ignore(typeof(UnknownItemException));
        });
        configure.AddConsumers(Assembly.GetEntryAssembly());
        configure.AddSagaStateMachine<PurchaseStateMachine, PurchaseState>(sagaConfigurator =>
        {
            sagaConfigurator.UseInMemoryOutbox();
        })
        .MongoDbRepository(r =>
        {
            var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
            r.Connection = mongoDbSettings.ConnectionString;
            r.DatabaseName = serviceSettings.ServiceName;
        });
    });
    var queueSettings = builder.Configuration.GetSection(nameof(QueueSettings)).Get<QueueSettings>();
    EndpointConvention.Map<GrantItems>(new Uri(queueSettings.GrantItemsQueueAddress));
    EndpointConvention.Map<DebitGil>(new Uri(queueSettings.DebitGilQueueAddress));
    EndpointConvention.Map<SubtractItems>(new Uri(queueSettings.SubtractItemsQueueAddress));
    services.AddMassTransitHostedService();
    services.AddGenericRequestClient();
}

