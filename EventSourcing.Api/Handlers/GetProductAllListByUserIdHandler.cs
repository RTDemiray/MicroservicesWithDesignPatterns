using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Api.Dtos;
using EventSourcing.Api.Models;
using EventSourcing.Api.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Api.Handlers
{
    public class GetProductAllListByUserIdHandler : IRequestHandler<GetAllProductListByUserId,List<ProductDto>>
    {
        private readonly AppDbContext _context;

        public GetProductAllListByUserIdHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductDto>> Handle(GetAllProductListByUserId request, CancellationToken cancellationToken)
        {
            var products = await _context.Products.Where(x => x.UserId == request.UserId).ToListAsync(cancellationToken: cancellationToken);
            return products.Select(x => new ProductDto { Id = x.Id, Name = x.Name, Price = x.Price, Stock = x.Stock, UserId = x.UserId }).ToList();
        }
    }
}