using System;
using System.Collections.Generic;
using MassTransit;

namespace Shared.Interfaces
{
    public interface IOrderCreatedEvent : CorrelatedBy<Guid>
    {
        //CorrelatedBy ilgili satırın id'si ile ilişkilendirip state'ini değiştirmek için.
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}