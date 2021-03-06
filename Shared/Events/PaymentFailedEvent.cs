using System;
using System.Collections.Generic;
using Shared.Interfaces;

namespace Shared
{
    public class PaymentFailedEvent : IPaymentFailedEvent
    {
        public PaymentFailedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; }
        public string Reason  { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; }
        
    }
}