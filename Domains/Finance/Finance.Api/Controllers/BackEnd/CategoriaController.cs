using Finance.Api.Application;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using Microsoft.AspNetCore.Mvc;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Controllers.BackEnd
{
    [Route("Finance/BackEnd/[controller]")]
    [ApiController]
    public class CategoriaController(ICategoriaService service) : FinanceBackEndControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoriaRequest request)
        {
            try
            {
                Categoria categoria = await service.Create(request.Name, request.DisplayName);

                return CreatedAtAction(nameof(Get), new { name = categoria.Name }, Map(categoria, new CategoriaUsage(0, 0, 0)));
            }
            catch (DuplicateNameException exception)
            {
                return Conflict(new ProblemDetails { Title = "Name already exists", Detail = exception.Message, Extensions = { ["field"] = "Name" } });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid category", Detail = exception.Message });
            }
        }

        [HttpDelete("{name}")]
        public async Task<IActionResult> Delete(string name, [FromQuery] bool confirmReferences = false)
        {
            try
            {
                await using IApplicationOperation operation = await service.BeginOperation();
                CategoriaDeleteResult result = await service.Delete(name, confirmReferences);

                if (!result.Deleted)
                {
                    return Conflict(Map(result.Usage));
                }

                await operation.Complete();

                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> Get(string name)
        {
            (Categoria Categoria, CategoriaUsage Usage)? result = await service.GetByName(name);

            return result is null ? NotFound() : Ok(Map(result.Value.Categoria, result.Value.Usage));
        }

        [HttpGet("List")]
        public async Task<IReadOnlyList<CategoriaDto>> GetList()
            => [.. (await service.GetAll()).Select(item => Map(item.Categoria, item.Usage))];

        [HttpPatch("{name}")]
        public async Task<IActionResult> Update(string name, [FromBody] UpdateCategoriaRequest request)
        {
            try
            {
                Categoria categoria = await service.Update(name, request.DisplayName);
                CategoriaUsage usage = (await service.GetByName(categoria.Name))!.Value.Usage;

                return Ok(Map(categoria, usage));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid category", Detail = exception.Message });
            }
        }

        private static CategoriaDto Map(Categoria categoria, CategoriaUsage usage)
            => new(categoria.Name, categoria.DisplayName, usage.Total);

        private static CategoriaUsageDto Map(CategoriaUsage usage)
            => new(usage.VociRicorrenti, usage.Pianificazioni, usage.Movimenti);
    }
}
