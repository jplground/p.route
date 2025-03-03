using Microsoft.AspNetCore.Mvc;
using Tfl.Api.Presentation.Entities;

namespace p.route.service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RouteController : ControllerBase
    {
        // Id's of some stations (StopType=NaptanMetroStation):
        // Northfields: 940GZZLUNFD
        // Ealing Broadway: 940GZZLUEBY
        // (StopType=NaptanRailStation)
        // West Ealing: 910GWEALING

        // Bus stops:
        // Northfield Station (Towards Ealing): 490000159B
        // Northfield Station (coming home): 490000159A
        // Dean Gardens / Mattock Lane: 490G00013447

        private readonly ILogger<RouteController> _logger;
        private readonly HttpClient _httpClient;

        public RouteController(ILogger<RouteController> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
        }

        [HttpGet]
        [Route("AllLines")]
        public async Task<ActionResult<IEnumerable<string>>> GetAllLines(string mode)
        {
            var lineClient = new LineClient(_httpClient);
            var route = await lineClient.RouteAsync([Anonymous3.Regular]);
            return route
                .Where(r => r.ModeName == mode)
                .Select(r => r.Name)
                .ToArray();
        }

        [HttpGet]
        [Route("AllModes")]
        public async Task<ActionResult<IEnumerable<string>>> GetModesLines()
        {
            var lineClient = new LineClient(_httpClient);
            var route = await lineClient.RouteAsync([Anonymous3.Regular]);
            return route
                .Select(r => r.ModeName)
                .Distinct()
                .ToArray();
        }

        [HttpGet]
        [Route("Route Sections")]
        public async Task<ActionResult<IEnumerable<string>>> GetRouteSections(string mode, string line)
        {
            var lineClient = new LineClient(_httpClient);
            var route = await lineClient.RouteAsync([Anonymous3.Regular]);
            return route
                .Where(r => r.ModeName == mode)
                .Where(r => r.Name == line)
                .SelectMany(r => r.RouteSections)
                .Select(rs => rs.Name)
                .ToArray();
        }

        [HttpGet]
        [Route("Tube Stops")]
        public async Task<ActionResult<IEnumerable<string>>> GetTubeStopes(string line)
        {
            var stopPointClient = new StopPointClient(_httpClient);
            //var stopTypes = await stopPointClient.GetByTypeAsync(["NaptanMetroStation"]);
            var stopTypes = await stopPointClient.GetByTypeAsync(["NaptanRailStation"]);
            return stopTypes.Where(st => st.Lines.Any(l => l.Name == line)).Select(st => $"{st.CommonName} ({st.Id})").ToArray();
        }
    }
}
