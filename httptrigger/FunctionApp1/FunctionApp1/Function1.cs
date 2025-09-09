using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace httptriggersample1
{
    public class httpTrigSample
    {
        private readonly ILogger<httpTrigSample> _logger;

        public httpTrigSample(ILogger<httpTrigSample> logger)
        {
            _logger = logger;
        }

        [Function("GetUser")]
        public async Task<HttpResponseData> GetUser(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            var response = req.CreateResponse();

            var user = new { Id = 1, Name = "John" };
            await response.WriteStringAsync(JsonSerializer.Serialize(user));

            return response;

        }

        [Function("CreateUser")]
        public async Task<HttpResponseData> CreateUser(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            var body = await req.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<User>(body);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteStringAsync($"Created user: {user.Name}");

            return response;
        }
    }
    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
