using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace hello_world_dotnet.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DaprController : ControllerBase
    {
        static string daprPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3500";
        const string stateStoreName = "statestore";
        static string stateUrl = $"http://localhost:{daprPort}/v1.0/state/{stateStoreName}";

        private readonly ILogger<DaprController> _logger;
        private readonly IHttpClientFactory _clientFactory;

        public DaprController(ILogger<DaprController> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }

        [HttpGet("/order")]
        public async Task<IActionResult> Get()
        {
            using var httpClient = _clientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{stateUrl}/order");
            var response = await httpClient.SendAsync(request);
            var content = await response.Content?.ReadAsStringAsync();
            Debug.WriteLine($"Respone content: {content}");
            if (!response.IsSuccessStatusCode
                || string.IsNullOrWhiteSpace(content))
            {
                return NotFound(content);
            }
            
            var orders = await response.Content.ReadFromJsonAsync<List<Order>>();
            if (orders?.Count > 0)
                return Ok(orders);
            return NotFound();
        }
    }
}
