namespace Shared.Interfaces
{
    public interface IOrderRequestFailedEvent
    {
        public int OrderID { get; set; }
        public string Reason { get; set; }
    }
}