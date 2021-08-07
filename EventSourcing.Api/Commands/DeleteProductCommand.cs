using System;
using MediatR;

namespace EventSourcing.Api.Commands
{
    public class DeleteProductCommand : IRequest
    {
        public Guid Id { get; set; }
    }
}