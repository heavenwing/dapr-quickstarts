using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
            HttpResponseMessage response;
            try
            {
                response = await httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                await Response.WriteAsync(ex.Message);
                return new StatusCodeResult(500);
            }

            if (response.StatusCode != HttpStatusCode.OK)
                return NotFound(response.ReasonPhrase);

            var content = await response.Content?.ReadAsStringAsync();
            Debug.WriteLine($"Respone content: {content}");
            var orders = JsonSerializer.Deserialize<List<Order>>(content);
            if (orders?.Count > 0)
                return Ok(orders);
            return NotFound();
        }

        [HttpPost("/neworder")]
        public async Task<IActionResult> Post([FromBody] Order data)
        {
            Debug.WriteLine("Got a new order! Order ID: " + data.Id);

            var state = new List<object>
            {
                new
                {
                    key="order",
                    value=data
                }
            };

            using var httpClient = _clientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, $"{stateUrl}");
            request.Content = new StringContent(JsonSerializer.Serialize(state));
            HttpResponseMessage response;
            try
            {
                response = await httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                await Response.WriteAsync(ex.Message);
                return new StatusCodeResult(500);
            }

            if (!response.IsSuccessStatusCode)
                return BadRequest(response.ReasonPhrase);

            Debug.WriteLine("Successfully persisted state.");
            return Ok();
        }
    }
}
