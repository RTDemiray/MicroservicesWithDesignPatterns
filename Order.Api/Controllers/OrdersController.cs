using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Order.Api.DTOs;
using Order.Api.Models;
using Shared;

namespace Order.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public OrdersController(AppDbContext appDbContext, IPublishEndpoint publishEndpoint,
            ISendEndpointProvider sendEndpointProvider)
        {
            _appDbContext = appDbContext;
            _publishEndpoint = publishEndpoint;
            _sendEndpointProvider = sendEndpointProvider;
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto orderCreate)
        {
            var newOrder = new Models.Order
            {
                BuyerId = orderCreate.BuyerId,
                Status = OrderStatus.Suspend,
                Address = new Address
                {
                    Line = orderCreate.Address.Line, Province = orderCreate.Address.Province,
                    District = orderCreate.Address.District
                },
                CreatedDate = DateTime.Now
            };

            orderCreate.OrderItems.ForEach(item =>
            {
                newOrder.Items.Add(new OrderItem() {Price = item.Price, Count = item.Count});
            });

            await _appDbContext.AddAsync(newOrder);
            await _appDbContext.SaveChangesAsync();

            var orderCreatedEvent = new OrderCreatedEvent
            {
                BuyerId = orderCreate.BuyerId,
                OrderId = newOrder.Id,
                PaymentMessage = new PaymentMessage()
                {
                    CardName = orderCreate.Payment.CardName,
                    CardNumber = orderCreate.Payment.CardNumber,
                    Expiration = orderCreate.Payment.Expiration,
                    Cvv = orderCreate.Payment.Cvv,
                    TotalPrice = orderCreate.OrderItems.Sum(x => x.Price * x.Count)
                }
            };

            orderCreate.OrderItems.ForEach(item =>
            {
                orderCreatedEvent.OrderItemMessages.Add(new OrderItemMessage()
                {
                    Count = item.Count,
                    ProductId = item.ProductId
                });
            });
            //@TODO: Farklı microserviceler aynı datayı dinleyecek olsaydı publish methodunu kullanmamız gerekirdi. Ama tek bir microservice dinleyecekse send methodu kullanmamız gerekir.

            //@TODO: Publish: Exchange'e gider bir kuyruğa gitmediğinden dolayı kalıcı hale gelmez (havadadır). Publish edilen dataları almak için mutalaka Subscribe olmak lazım.
            //@TODO: Send: Direkt olarak kuyruğa gönderir. Sadece kuyruğa subscribe olan microserviceler dinleyebilir.

            await _publishEndpoint.Publish(orderCreatedEvent);
            // await _sendEndpointProvider.Send(orderCreatedEvent);
            return Ok();
        }
    }
}