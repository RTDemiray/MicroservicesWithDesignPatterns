using EventSourcing.Api.Dtos;
using MediatR;

namespace EventSourcing.Api.Commands
{
    public class ChangeProductNameCommand : IRequest<Unit>
    {
        public ChangeProductNameDto ChangeProductName { get; set; }
    }
}