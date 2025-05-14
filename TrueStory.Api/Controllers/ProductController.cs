using Microsoft.AspNetCore.Mvc;
using Products.Application;
using Products.Domain;

[ApiController]
[Route("api/products")]
public class ProductController(IProductService service) : ControllerBase
{    
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? name, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await service.GetAsync(name, page, pageSize);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Product product)
    {
        var result = await service.CreateAsync(product);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await service.DeleteAsync(id);
        return Ok(result);
    }
}