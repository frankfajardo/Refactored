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
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private ProductsRepository _productsRepository;

        public ProductsController(ProductsRepository productsRepository)
        {
            _productsRepository = productsRepository;
        }

        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<Product>))]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _productsRepository.GetProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id:guid}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Product))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            var product = await _productsRepository.GetProductAsync(id);
            return product != null ? Ok(product) : NotFound();
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Product))]
        public async Task<IActionResult> PostProduct([FromBody] NewProduct newProduct)
        {
            var savedProduct = await _productsRepository.AddProductAsync(newProduct);
            return CreatedAtAction(nameof(GetProduct), new { id = savedProduct.Id }, savedProduct);
        }

        [HttpPut("{id:guid}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PutProduct(Guid id, [FromBody] Product product)
        {
            product.Id = id;
            await _productsRepository.UpdateOrAddProductAsync(product);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            await _productsRepository.DeleteProductAsync(id);
            return NoContent();
        }
    }
}
