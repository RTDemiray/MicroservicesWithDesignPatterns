using EventSourcing.Api.Dtos;
using MediatR;

namespace EventSourcing.Api.Commands
{
    public class ChangeProductPriceCommand : IRequest
    {
        public ChangeProductPriceDto ChangeProductPrice { get; set; }
    }
}