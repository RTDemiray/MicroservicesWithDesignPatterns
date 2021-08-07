using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Api.EventStores;
using EventSourcing.Api.Models;
using EventSourcing.Shared.Events;
using EventStore.ClientAPI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventSourcing.Api.BackgroundServices
{
    public class ProductReadModelEventStore : BackgroundService
    {
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly ILogger<ProductReadModelEventStore> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ProductReadModelEventStore(IEventStoreConnection eventStoreConnection, ILogger<ProductReadModelEventStore> logger, IServiceProvider serviceProvider)
        {
            _eventStoreConnection = eventStoreConnection;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _eventStoreConnection.ConnectToPersistentSubscriptionAsync(ProductStream.StreamName,
                ProductStream.GroupName, EventAppeared,autoAck:false);
        }

        private async Task EventAppeared(EventStorePersistentSubscriptionBase arg1, ResolvedEvent arg2)
        {
            var type = Type.GetType($"{Encoding.UTF8.GetString(arg2.Event.Metadata)}, EventSourcing.Shared");
            
            _logger.LogInformation($"The message processing... : {type}");
            
            var eventData = Encoding.UTF8.GetString(arg2.Event.Data);
            var @event = JsonSerializer.Deserialize(eventData, type);

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Products products = null;

            switch (@event)
            {
                case ProductCreatedEvent productCreatedEvent:
                    products = new Products()
                    {
                        Name = productCreatedEvent.Name,
                        Id = productCreatedEvent.Id,
                        Price = productCreatedEvent.Price,
                        Stock = productCreatedEvent.Stock,
                        UserId = productCreatedEvent.UserId
                    };
                    await context.Products.AddAsync(products);
                    break;

                case ProductNameChangedEvent productNameChangedEvent:

                    products = context.Products.Find(productNameChangedEvent.Id);
                    if (products != null)
                    {
                        products.Name = productNameChangedEvent.ChangedName;
                    }
                    break;

                case ProductPriceChangedEvent productPriceChangedEvent:
                    products = context.Products.Find(productPriceChangedEvent.Id);
                    if (products != null)
                    {
                        products.Price = productPriceChangedEvent.ChangedPrice;
                    }
                    break;

                case ProductDeletedEvent productDeletedEvent:
                    products = context.Products.Find(productDeletedEvent.Id);
                    if (products != null)
                    {
                        context.Products.Remove(products);
                    }
                    break;
            }

            await context.SaveChangesAsync();
            arg1.Acknowledge(arg2.Event.EventId);
        }
    }
}