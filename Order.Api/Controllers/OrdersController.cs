using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Order.Api.DTOs;
using Order.Api.Models;
using Shared;
using Shared.Events;
using Shared.Interfaces;

namespace Order.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public OrdersController(AppDbContext appDbContext,
            ISendEndpointProvider sendEndpointProvider)
        {
            _appDbContext = appDbContext;
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

            var orderCreatedRequestEvent = new OrderCreatedRequestEvent
            {
                BuyerId = orderCreate.BuyerId,
                OrderId = newOrder.Id,
                Payment = new PaymentMessage()
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
                orderCreatedRequestEvent.OrderItems.Add(new OrderItemMessage()
                {
                    Count = item.Count,
                    ProductId = item.ProductId
                });
            });
            //@TODO: Farkl?? microserviceler ayn?? datay?? dinleyecek olsayd?? publish methodunu kullanmam??z gerekirdi. Ama tek bir microservice dinleyecekse send methodu kullanmam??z gerekir.

            //@TODO: Publish: Exchange'e gider bir kuyru??a gitmedi??inden dolay?? kal??c?? hale gelmez (havadad??r). Publish edilen datalar?? almak i??in mutalaka Subscribe olmak laz??m.
            //@TODO: Send: Direkt olarak kuyru??a g??nderir. Sadece kuyru??a subscribe olan microserviceler dinleyebilir.

            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.OrderSaga}"));
            await sendEndpoint.Send<IOrderCreatedRequestEvent>(orderCreatedRequestEvent);
            
            return Ok();
        }
    }
}