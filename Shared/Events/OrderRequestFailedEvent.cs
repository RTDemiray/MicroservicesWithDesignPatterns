using Shared.Interfaces;

namespace Shared.Events
{
    public class OrderRequestFailedEvent : IOrderRequestFailedEvent
    {
        public int OrderID { get; set; }
        public string Reason { get; set; }
    }
}