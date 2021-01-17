using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Refactored.Api.Models;
using Refactored.Api.Repositories;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Refactored.Api.Controllers
{
    [Route("api/products/{productId:guid}/options")]
    [ApiController]
    public class ProductOptionsController : ControllerBase
    {
        private ProductOptionsRepository _productOptionsRepository;

        public ProductOptionsController(ProductOptionsRepository productOptionsRepository)
        {
            _productOptionsRepository = productOptionsRepository;
        }

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<ProductOption>))]
        public async Task<IActionResult> GetOptions(Guid productId)
        {
            var productOpions = await _productOptionsRepository.GetProductOptionsAsync(productId);
            return Ok(productOpions);
        }

        [HttpGet("{id:guid}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductOption))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOption(Guid productId, Guid id)
        {
            var productOpion = await _productOptionsRepository.GetProductOptionAsync(productId, id);
            return productOpion != null ? Ok(productOpion) : NotFound();
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ProductOption))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostOption(Guid productId, [FromBody] NewProductOption newProductOption)
        {
            newProductOption.ProductId = productId;
            try
            {
                var savedProductOption = await _productOptionsRepository.AddProductOptionAsync(newProductOption);
                return CreatedAtAction(nameof(GetOption), new { id = savedProductOption.Id }, savedProductOption);
            }
            catch (InvalidOperationException)
            {
                return BadRequest();
            }            
        }

        [HttpPut("{id:guid}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PutOption(Guid productId, Guid id, [FromBody] ProductOption productOption)
        {
            productOption.Id = id;
            productOption.ProductId = productId;
            try 
            {
                await _productOptionsRepository.UpdateOrAddProductOptionAsync(productOption);
                return NoContent();
            }
            catch (InvalidOperationException)
            {
                return BadRequest();
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteOption(Guid productId, Guid id)
        {
            await _productOptionsRepository.DeleteProductOptionAsync(productId, id);
            return NoContent();
        }
    }
}
