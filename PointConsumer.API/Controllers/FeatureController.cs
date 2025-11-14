using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PointConsumer.API.Configurations;
using PointConsumer.API.Services;

namespace PointConsumer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeatureController : ControllerBase
    {
        private readonly IProducerApiClient _producerApiClient;

        public FeatureController(IProducerApiClient producerApiClient)
        {
            _producerApiClient = producerApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            try
            {
                var str = await _producerApiClient.GetInitialPointsAsync();
                return Ok(str);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
