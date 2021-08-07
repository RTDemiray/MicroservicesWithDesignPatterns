using System.Collections.Generic;
using EventSourcing.Api.Dtos;
using MediatR;

namespace EventSourcing.Api.Queries
{
    public class GetAllProductListByUserId : IRequest<List<ProductDto>>
    {
        public int UserId { get; set; }
    }
}