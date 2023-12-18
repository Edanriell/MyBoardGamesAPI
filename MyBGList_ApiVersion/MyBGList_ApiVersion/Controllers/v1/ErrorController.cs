using Microsoft.AspNetCore.Mvc;

namespace MyBGList.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
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
