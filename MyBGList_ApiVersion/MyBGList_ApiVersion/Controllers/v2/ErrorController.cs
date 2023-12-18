using Microsoft.AspNetCore.Mvc;

namespace MyBGList.Controllers.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    public class ErrorController : ControllerBase
    {
        [Route("v{version:apiVersion}/error2")]
        [HttpGet]
        public IActionResult Error()
        {
            return Problem();
        }
    }
}
