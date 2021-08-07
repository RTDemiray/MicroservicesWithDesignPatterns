using EventSourcing.Api.Dtos;
using MediatR;

namespace EventSourcing.Api.Commands
{
    public class CreateProductCommand : IRequest
    {
        public CreateProductDto CreateProduct { get; set; }
    }
}