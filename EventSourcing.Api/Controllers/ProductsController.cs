using System;
using System.Threading.Tasks;
using EventSourcing.Api.Commands;
using EventSourcing.Api.Dtos;
using EventSourcing.Api.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventSourcing.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{userId:int}")]
        public async Task<IActionResult> Get(int userId)
        {
            return Ok(await _mediator.Send(new GetAllProductListByUserId{UserId = userId}));
        } 

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto createProduct)
        {
            await _mediator.Send(new CreateProductCommand
            {
                CreateProduct = createProduct
            });
            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> ChangeName(ChangeProductNameDto changeProductName)
        {
            await _mediator.Send(new ChangeProductNameCommand
            {
                ChangeProductName = changeProductName
            });
            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> ChangePrice(ChangeProductPriceDto changeProductPrice)
        {
            await _mediator.Send(new ChangeProductPriceCommand
            {
                ChangeProductPrice = changeProductPrice
            });
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteProductCommand
            {
                Id = id
            });
            return NoContent();
        }
    }
}