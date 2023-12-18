using Microsoft.AspNetCore.Mvc;

namespace MyBGList.Controllers
{
    [ApiController]
    public class ErrorController : ControllerBase
    {
        [Route("/error2")]
        [HttpGet]
        public IActionResult Error()
        {
            return Problem();
        }
    }
}
