using Finance.Api.Application;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers.BackEnd
{
    [Route("Finance/BackEnd/[controller]")]
    [ApiController]
    public class MovimentoController(IMovimentoService service) : FinanceBackEndControllerBase
    {
        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMovimentoRequest request)
        {
            try
            {
                Movimento movimento = await service.Update(
                    id,
                    request.Date,
                    request.Description,
                    request.Formula,
                    request.CategoryName,
                    request.ClearCategory);

                return Ok(new MovimentoConfigurationDto(movimento));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid movement", Detail = exception.Message });
            }
        }
    }
}
