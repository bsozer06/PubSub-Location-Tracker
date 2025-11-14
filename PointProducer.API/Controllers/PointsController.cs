using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PointProducer.API.Services;

namespace PointProducer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PointsController : ControllerBase
    {
        private readonly PointDataService _dataService;

        public PointsController(PointDataService dataService)
        {
            _dataService = dataService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPoints()
        {
            var points = _dataService.GetAllPoints;
            return Ok(points);
        }

    }

    //public record PointUpdateDto(int PointId, double X, double Y);

}
