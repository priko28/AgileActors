using AggregationService.Abstractions;
using AggregationService.Models.Response;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AggregatorController : ControllerBase
{
    private readonly IAggregatorService _aggregatorService;

    public AggregatorController(IAggregatorService aggregatorService)
    {
        _aggregatorService = aggregatorService;
    }

    [HttpGet("aggregated-data")]
    public async Task<ActionResult<IEnumerable<AggregatedData>>> GetAggregatedData(
       [FromQuery] string filter = null,
       [FromQuery] string sort = null)
    {
        var result = await _aggregatorService.GetAggregatedDataAsync(filter, sort);
        return Ok(result);
    }
}