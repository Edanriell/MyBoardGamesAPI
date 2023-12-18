using Microsoft.AspNetCore.Mvc;

namespace MyBGList.Controllers.v2
{
    [Route("v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [ApiController]
    public class DeveloperGreetingController : ControllerBase
    {
        private readonly ILogger<DeveloperGreetingController> _logger;

        public DeveloperGreetingController(ILogger<DeveloperGreetingController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get(string name)
        {
            return $"Hello {name} ! You are greate developer keep learning.";
        }
    }
}
